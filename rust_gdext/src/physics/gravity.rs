use std::mem;

use godot::{
    classes::{base_material_3d::ShadingMode, mesh, ArrayMesh, Engine, MeshInstance3D, StandardMaterial3D, SurfaceTool},
    prelude::*,
};
use itertools::Itertools;
use rayon::prelude::*;

const GROUP_NAME: &str = "gravity_body";

/// Imposes gravity on all [Massive] nodes next to or below this node
#[derive(GodotClass)]
#[class(tool, init, base = Node)]
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

    bodies: Vec<Gd<GravityBody>>,

    trajectories: Vec<Gd<MeshInstance3D>>,
    base: Base<Node>,
}

#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
struct GravityBody {
    #[export]
    #[init(val = 1.0)]
    pub mass: f32,

    #[export]
    pub velocity: Vector3,

    #[export]
    pub trajectory_color: Color,

    base: Base<Node3D>,
}

#[derive(Clone)]
struct SimulatedBody {
    mass: f32,
    pos: Vector3,
    vel: Vector3,
}

struct Trajectory {
    color: Color,
    points: Vec<Vector3>,
}

impl GravityBody {
    fn update_from_sim(&mut self, sim: &SimulatedBody) {
        self.mass = sim.mass;
        self.velocity = sim.vel;
        self.base_mut().set_position(sim.pos);
    }
}

impl From<&Gd<GravityBody>> for SimulatedBody {
    fn from(body: &Gd<GravityBody>) -> Self {
        let b = body.bind();
        Self {
            mass: b.mass,
            vel: b.velocity,
            pos: b.base().get_position(),
        }
    }
}

#[godot_api]
impl INode3D for GravityBody {
    fn ready(&mut self) {
        self.base_mut().add_to_group(GROUP_NAME);
    }
}

#[godot_api]
impl GravityController {
    fn get_massive_nodes(&mut self) {
        let bodies = self
            .base_mut()
            .get_tree()
            .expect("Expected to get SceneTree")
            .get_nodes_in_group(GROUP_NAME)
            .iter_shared()
            .map(|n| n.cast::<GravityBody>())
            .collect::<Vec<_>>();

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

    #[func]
    fn simulate_trajectory(&mut self) {
        let n_steps = self.simulation_steps as usize;
        let delta = self.simulation_step_delta;
        let grav_const = self.grav_const;

        self.ready();

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

        let n_bodies = bodies_sim.len();
        let mut accelerations = Vec::with_capacity(n_bodies);

        for _ in 1..n_steps {
            // Step
            Self::simulate_step(grav_const, delta, &mut bodies_sim, &mut accelerations);

            // Store positions
            for (i, body) in bodies_sim.iter().enumerate() {
                trajectories[i].points.push(body.pos);
            }
        }

        let new_meshes = trajectories
            .iter()
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

        // Free the old trajectories
        let old_meshes = mem::replace(&mut self.trajectories, new_meshes);
        old_meshes.into_iter().for_each(|instance| instance.free());
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
        // Commit to an ArrayMesh
        (surface_tool.commit().unwrap(), material)
    }
}

#[godot_api]
impl INode for GravityController {
    fn ready(&mut self) {
        self.get_massive_nodes();
    }

    fn physics_process(&mut self, delta: f64) {
        // Don't run physics in editor
        if Engine::singleton().is_editor_hint() {
            return;
        }

        // Update list of nodes influenced by gravity
        self.get_massive_nodes();

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
