use super::body::GravityBody;
use godot::{
    classes::{MeshInstance3D, notify::Node3DNotification},
    prelude::*,
};
use itertools::Itertools;
use proc::editor;
use rayon::prelude::*;

/// Manages gravity interactions between [`GravityBody`] instances in a 3D space.
///
/// The `GravityController` is responsible for:
///
/// - Tracking all gravity-affected bodies in the scene
/// - Calculating gravitational forces between bodies
/// - Simulating and displaying trajectory predictions
/// - Managing gravity-related properties like the gravitational constant
///
/// # Related Modules
///
/// See the [`trajectories`](super::trajectories) module for details on trajectory visualization.
///
/// # Example
///
/// ```no_run
/// GravityController // Used as a parent node for `GravityBody`
/// |- GravityBody
/// |- GravityBody
/// |- SomeNode // Nesting is allowed! (even multiple levels)
///     |- GravityBody
/// ```
#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
pub struct GravityController {
    base: Base<Node3D>,

    /// The gravitational constant used in force calculations
    #[export]
    #[init(val = 1.0)]
    pub grav_const: f32,

    /// Collection of all gravity bodies managed by this controller
    pub bodies: Vec<Gd<GravityBody>>,

    /// Number of steps to compute when simulating trajectories
    #[export]
    #[init(val = 1000)]
    pub simulation_steps: u32,

    /// Time increment (in seconds) between each simulation step
    #[export]
    #[init(val = 0.3)]
    pub simulation_step_delta: f32,

    /// Whether to automatically recalculate trajectories on certain changes
    #[export]
    #[init(val = true)]
    pub auto_update_trajectories: bool,

    /// Optional body to use as the reference point for trajectory calculations
    #[export]
    pub sim_center_body: Option<Gd<GravityBody>>,

    /// Mesh instances representing the currently displayed trajectories
    pub trajectories: Vec<Gd<MeshInstance3D>>,
}

#[derive(Clone)]
pub struct SimulatedBody {
    pub body_instance_id: InstanceId,
    pub mass: f32,
    pub pos: Vector3,
    pub vel: Vector3,
}

impl From<&Gd<GravityBody>> for SimulatedBody {
    fn from(body: &Gd<GravityBody>) -> Self {
        let b = body.bind();
        Self {
            body_instance_id: body.instance_id(),
            mass: b.mass,
            vel: b.velocity,
            pos: b.base().get_position(),
        }
    }
}

impl GravityController {
    #[editor(only)]
    fn setup_editor(&mut self) {
        // Disable physics in editor
        self.base_mut().set_physics_process(false);

        let callable = self
            .base()
            .callable(__gdext_GravityController_Funcs::_on_update_trajectories);

        self.base_mut()
            .connect(Self::UPDATE_TRAJECTORY_SIGNAL, &callable);
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

    pub fn step_time(
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
        Self::step_time(
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
