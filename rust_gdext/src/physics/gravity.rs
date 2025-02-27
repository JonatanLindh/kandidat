use godot::{
    builtin::math::ApproxEq,
    classes::{
        ArrayMesh, MeshInstance3D, StandardMaterial3D, SurfaceTool, base_material_3d::ShadingMode,
        mesh, notify::Node3DNotification,
    },
    prelude::*,
};
use itertools::Itertools;
use proc::editor;
use rayon::prelude::*;
use std::mem;

const BUS_PATH: &str = "/root/Bus";
const UPDATE_TRAJECTORY_SIGNAL: &str = "UPDATE_TRAJECTORY";

/// Imposes gravity on all [GravityBody] in this node's tree
#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
struct GravityController {
    #[export]
    #[init(val = 1.0)]
    grav_const: f32,

    #[export]
    #[init(val = 1000)]
    simulation_steps: u32,

    #[export]
    #[init(val = 0.3)]
    simulation_step_delta: f32,

    #[export]
    #[init(val = true)]
    auto_update_simulation: bool,

    #[export]
    sim_center_body: Option<Gd<GravityBody>>,

    bodies: Vec<Gd<GravityBody>>,

    trajectories: Vec<Gd<MeshInstance3D>>,
    base: Base<Node3D>,
}

#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
struct GravityBody {
    #[export]
    #[var(get, set = set_mass)]
    #[init(val = 1.0)]
    pub mass: f32,

    #[export]
    #[var(get, set = set_initial_velocity)]
    pub initial_velocity: Vector3,

    #[export]
    pub trajectory_color: Color,

    // Track the last position to detect changes
    #[export]
    last_position: Vector3,

    base: Base<Node3D>,
}

#[derive(Clone)]
struct SimulatedBody {
    body_instance_id: InstanceId,
    mass: f32,
    pos: Vector3,
    vel: Vector3,
}

struct Trajectory {
    color: Color,
    points: Vec<Vector3>,
}

#[godot_api]
impl INode3D for GravityBody {
    fn on_notification(&mut self, notification: Node3DNotification) {
        use Node3DNotification::*;

        match notification {
            READY => {
                // Initialize last_position
                self.last_position = self.base().get_position();

                self.setup_editor();
            }

            // Will only happen in editor
            TRANSFORM_CHANGED => {
                let current_pos = self.base().get_position();

                if current_pos.approx_eq(&self.last_position) {
                    return;
                }

                self.last_position = current_pos;

                self.emit_update_simulation();
            }

            _ => {}
        }
    }
}

#[godot_api]
impl GravityBody {
    #[func]
    pub fn set_initial_velocity(&mut self, value: Vector3) {
        self.initial_velocity = value;
        self.emit_update_simulation();
    }

    #[func]
    pub fn set_mass(&mut self, value: f32) {
        self.mass = value;
        self.emit_update_simulation();
    }
}

impl GravityBody {
    #[editor(only)]
    fn setup_editor(&mut self) {
        // Notify on transforms in editor (for simulation updates)
        self.base_mut().set_notify_transform(true);
    }

    fn update_from_sim(&mut self, sim: &SimulatedBody) {
        self.mass = sim.mass;
        self.initial_velocity = sim.vel;
        self.base_mut().set_position(sim.pos);
    }

    #[editor(only)]
    fn emit_update_simulation(&mut self) {
        if !self.base().is_inside_tree() {
            return;
        }

        self.base()
            .get_node_as::<Node>(BUS_PATH)
            .call_deferred("emit_signal", &[UPDATE_TRAJECTORY_SIGNAL.to_variant()]);
    }
}

impl From<&Gd<GravityBody>> for SimulatedBody {
    fn from(body: &Gd<GravityBody>) -> Self {
        let b = body.bind();
        Self {
            body_instance_id: body.instance_id(),
            mass: b.mass,
            vel: b.initial_velocity,
            pos: b.base().get_position(),
        }
    }
}

#[godot_api]
impl GravityController {
    #[func]
    fn simulate_trajectory(&mut self) {
        let n_steps = self.simulation_steps as usize;
        let delta = self.simulation_step_delta;
        let grav_const = self.grav_const;

        let (mut bodies_sim, mut trajectories): (Vec<_>, Vec<_>) = self
            .bodies
            .iter()
            .map(|b| (SimulatedBody::from(b), b.bind().trajectory_color))
            .map(|(b, color)| {
                let mut points = Vec::with_capacity(n_steps);
                points.push(b.pos);

                (b, Trajectory { color, points })
            })
            .unzip();

        let offset_info = self
            .sim_center_body
            .as_ref()
            .and_then(|b| {
                let id = b.instance_id();
                bodies_sim
                    .iter()
                    .find_position(|b| id == b.body_instance_id)
            })
            .map(|(idx, b)| (idx, b.pos));

        let n_bodies = bodies_sim.len();
        let mut accelerations = Vec::with_capacity(n_bodies);

        for _ in 1..n_steps {
            // Step
            Self::simulate_step(grav_const, delta, &mut bodies_sim, &mut accelerations);

            let offset = offset_info
                .map(|(idx, init)| bodies_sim[idx].pos - init)
                .unwrap_or(Vector3::ZERO);

            // Store positions
            for (i, body) in bodies_sim.iter().enumerate() {
                trajectories[i].points.push(body.pos - offset);
            }
        }

        self.replace_trajectories(trajectories);
    }

    #[func]
    fn clear_trajectories(&mut self) {
        self.replace_trajectories(Vec::new());
    }

    #[func]
    fn trigger_auto_update_simulation(&mut self) {
        if self.auto_update_simulation {
            self.simulate_trajectory();
        }
    }
}

impl GravityController {
    #[editor(only)]
    fn setup_editor(&mut self) {
        // Disable physics in editor
        self.base_mut().set_physics_process(false);

        let callable = self.base().callable("trigger_auto_update_simulation");

        self.base()
            .get_node_as::<Node>(BUS_PATH)
            .connect(UPDATE_TRAJECTORY_SIGNAL, &callable);
    }

    fn get_massive_nodes(&mut self) {
        fn collect_bodies_rec(node: Gd<Node>, bodies: &mut Vec<Gd<GravityBody>>) {
            match node.try_cast::<GravityBody>() {
                Ok(body) => bodies.push(body),
                Err(node) => {
                    node.get_children()
                        .iter_shared()
                        .for_each(|child| collect_bodies_rec(child, bodies));
                }
            }
        }

        let mut bodies = Vec::new();

        // Start recursive collection with the controller node
        collect_bodies_rec(self.to_gd().upcast(), &mut bodies);

        self.bodies = bodies;
    }

    fn calc_acc(grav_const: f32, body: &SimulatedBody, bodies: &[SimulatedBody]) -> Vector3 {
        bodies
            .iter()
            .map(|other| (other.pos - body.pos, other.mass))
            .filter(|(diff, _)| !diff.is_zero_approx())
            .map(|(diff, other_mass)| {
                let r2 = diff.length_squared();
                let dir = diff.normalized();

                grav_const * dir * (other_mass) / r2
            })
            .sum()
    }

    fn simulate_step(
        grav_const: f32,
        delta: f32,
        bodies_sim: &mut [SimulatedBody],
        accelerations: &mut Vec<Vector3>,
    ) {
        // 1: Calculate accelerations
        bodies_sim
            .par_iter()
            .map(|body| Self::calc_acc(grav_const, body, bodies_sim))
            .collect_into_vec(accelerations);

        // 2: Update velocities and positions
        bodies_sim
            .iter_mut()
            .zip(accelerations)
            .for_each(|(body, acc)| {
                body.vel += *acc * delta;
                body.pos += body.vel * delta;
            });
    }

    fn replace_trajectories(&mut self, new_trajectories: Vec<Trajectory>) {
        let new_meshes = new_trajectories
            .iter()
            .filter(|traj| traj.points.len() >= 2)
            .map(|traj| {
                // Build the mesh for the trajectory
                let (mesh, material) = Self::build_trajectory_mesh(traj);

                // Create a MeshInstance3D, set mesh and color
                let mut instance = MeshInstance3D::new_alloc();
                instance.set_mesh(&mesh);
                instance.set_material_override(&material);

                // Add mesh to tree
                self.base_mut().add_child(&instance);

                instance
            })
            .collect_vec();

        let old_meshes = mem::replace(&mut self.trajectories, new_meshes);

        // Queue old meshes for deletion
        for mut instance in old_meshes {
            instance.queue_free();
        }
    }

    fn build_trajectory_mesh(trajectory: &Trajectory) -> (Gd<ArrayMesh>, Gd<StandardMaterial3D>) {
        let mut surface_tool = SurfaceTool::new_gd();
        surface_tool.begin(mesh::PrimitiveType::LINE_STRIP);

        for &point in &trajectory.points {
            surface_tool.add_vertex(point);
        }

        // Create material with color
        let mut material = StandardMaterial3D::new_gd();
        material.set_albedo(trajectory.color);
        material.set_shading_mode(ShadingMode::UNSHADED);

        // Commit
        match surface_tool.commit() {
            Some(mesh) => (mesh, material),
            None => {
                // Return an empty mesh if commit fails
                let empty_mesh = ArrayMesh::new_gd();
                (empty_mesh, material)
            }
        }
    }
}

#[godot_api]
impl INode3D for GravityController {
    fn ready(&mut self) {
        self.setup_editor();
    }

    fn on_notification(&mut self, notification: Node3DNotification) {
        use Node3DNotification::*;

        match notification {
            READY | CHILD_ORDER_CHANGED => {
                // Update list of gravity bodies
                self.get_massive_nodes();
            }

            _ => {}
        }
    }

    fn physics_process(&mut self, delta: f64) {
        if self.bodies.is_empty() {
            return;
        }

        // Create simulation counterparts of real bodies
        let mut bodies_sim = self.bodies.iter().map(SimulatedBody::from).collect_vec();

        // Simulate a physics step
        Self::simulate_step(
            self.grav_const,
            delta as f32,
            &mut bodies_sim,
            &mut Vec::new(),
        );

        // Apply simulated step to real bodies
        self.bodies
            .iter_mut()
            .zip(bodies_sim)
            .for_each(|(body, sim)| body.bind_mut().update_from_sim(&sim));
    }
}
