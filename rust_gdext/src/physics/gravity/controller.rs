use super::{
    Massive, NBodyGravityCalculator, Spacial, body::GravityBody, direct_summation::DirectSummation,
    trajectories::TrajectoryWorker,
};
use crate::{
    octree::{morton_based::MortonBasedOctree, visualize::OctreeVisualizer},
    to_glam_vec3,
};
use glam::Vec3A;
use godot::{
    classes::{MeshInstance3D, notify::Node3DNotification},
    prelude::*,
};
use itertools::Itertools;
use proc::editor;

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
    #[init(val = 4000)]
    pub simulation_steps: u32,

    /// Time increment (in seconds) between each simulation step
    #[export]
    #[init(val = 0.3)]
    pub simulation_step_delta: f32,

    /// Whether to automatically recalculate trajectories on certain changes
    #[export]
    #[init(val = true)]
    pub auto_update_trajectories: bool,

    pub trajectory_worker: Option<TrajectoryWorker>,

    /// Optional body to use as the reference point for trajectory calculations
    #[export]
    pub sim_center_body: Option<Gd<GravityBody>>,

    #[export]
    pub octree_visualizer: Option<Gd<OctreeVisualizer>>,

    /// Mesh instances representing the currently displayed trajectories
    pub trajectories: Vec<Gd<MeshInstance3D>>,

    /// Whether to use leapfrog integration for trajectory calculations
    #[export]
    #[init(val = false)]
    pub leapfrog_integration: bool,
}

/// Simulated representation of a [`GravityBody`] for physics calculations.
///
/// This struct acts as a lightweight, detached copy of a [`GravityBody`] for efficient
/// simulation of gravitational interactions without accessing the original nodes during
/// physics calculations.
///
///  ### Usage
///
/// `SimulatedBody` instances are created from [`GravityBody`] nodes during physics
/// calculations and trajectory predictions, then their updated states can be
/// transferred back to the original nodes.
#[derive(Clone, Debug)]
pub struct SimulatedBody {
    /// Unique identifier of the original `GravityBody` node
    pub body_instance_id: InstanceId,

    /// Mass of the body
    pub mass: f32,

    /// Current position in 3D space
    pub pos: Vec3A,

    /// Current velocity vector
    pub vel: Vec3A,

    pub acc: Vec3A,
}

impl Spacial for SimulatedBody {
    #[inline(always)]
    fn get_pos(&self) -> Vec3A {
        self.pos
    }
}

impl Massive for SimulatedBody {
    #[inline(always)]
    fn get_mass(&self) -> f32 {
        self.mass
    }
}

/// Converts a gravity body node reference into its simulation representation.
///
/// This implementation provides a clean way to extract the essential physical properties
/// from a [`GravityBody`] node for use in physics simulations.
impl From<&Gd<GravityBody>> for SimulatedBody {
    #[inline]
    fn from(body: &Gd<GravityBody>) -> Self {
        let b = body.bind();

        Self {
            body_instance_id: body.instance_id(),
            mass: b.mass,
            vel: to_glam_vec3(b.velocity),
            pos: to_glam_vec3(body.get_position()),
            acc: b.acc.unwrap_or_default(),
        }
    }
}

impl GravityController {
    /// Configures the controller for use in the Godot editor.
    ///
    /// This method:
    /// - Disables physics processing to prevent simulation while in the editor
    ///
    /// Only runs in the editor context due to the `#[editor(only)]` attribute.
    #[editor(only)]
    fn setup_editor(&mut self) {
        // Simulate less steps in the editor for "snappyness"
        self.simulation_steps = 1000;

        // Disable physics in editor
        self.base_mut().set_physics_process(false);
    }

    /// Recursively collects all [`GravityBody`] instances that are descendants of this controller.
    ///
    /// This method traverses the scene tree starting from the controller node and adds all
    /// [`GravityBody`] instances it finds to the controller's internal `bodies` collection.
    /// It handles nested nodes at any depth, allowing for flexible scene organization.
    ///
    /// # Implementation Details
    ///
    /// - Uses depth-first traversal to find all gravity bodies
    /// - Clears and rebuilds the entire collection each time it's called
    /// - Automatically called when the node is ready or when child nodes change
    fn get_gravity_bodies(&mut self) {
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

        self.calc_initial_accelerations();

        self.bodies = bodies;
    }

    /// Advances the physical simulation by one time step.
    ///
    /// This method:
    /// 1. Calculates acceleration for each body in parallel
    /// 2. Updates velocities based on the calculated accelerations
    /// 3. Updates positions based on the new velocities
    ///
    /// Uses parallel processing through Rayon to optimize performance for many bodies.
    ///
    /// # Parameters
    /// - `grav_const`: The gravitational constant to use in calculations
    /// - `delta`: The time step duration in seconds
    /// - `bodies_sim`: The bodies to simulate, will be updated in-place
    pub fn step_time(grav_const: f32, delta: f32, bodies_sim: &mut [SimulatedBody]) {
        // 1: Calculate accelerations
        let accelerations = if bodies_sim.len() < 100 {
            // Use sequential direct summation for small number of bodies
            DirectSummation::calculate_accelerations::<false>(grav_const, bodies_sim)
        } else {
            MortonBasedOctree::calculate_accelerations::<true>(grav_const, bodies_sim)
        };

        // 2: Update velocities and positions
        bodies_sim
            .iter_mut()
            .zip(accelerations)
            .for_each(|(body, acc)| {
                body.vel += acc * delta;
                body.pos += body.vel * delta;
            });
    }

    pub fn step_time_leapfrog(grav_const: f32, delta: f32, bodies_sim: &mut [SimulatedBody]) {
        // 1: x_{i+1} = x_i + v_i * delta + 0.5 * a_i * delta^2
        bodies_sim.iter_mut().for_each(|body| {
            body.pos += body.vel * delta + 0.5 * body.acc * delta * delta;
        });

        // 2: Calculate accelerations (a_{i+1})
        let a_next = if bodies_sim.len() < 100 {
            // Use sequential direct summation for small number of bodies
            DirectSummation::calculate_accelerations::<false>(grav_const, bodies_sim)
        } else {
            MortonBasedOctree::calculate_accelerations::<true>(grav_const, bodies_sim)
        };

        // 3: Update velocities and positions using leapfrog integration
        bodies_sim.iter_mut().zip(a_next).for_each(|(body, a_ip1)| {
            // v_{n+1} = v_n + 0.5 * (a_n + a_{n+1}) * delta
            body.vel += 0.5 * (body.acc + a_ip1) * delta;
            body.acc = a_ip1;
        });
    }

    fn calc_initial_accelerations(&mut self) {
        let bodies_sim = self.bodies.iter().map(SimulatedBody::from).collect_vec();

        let a0 = if self.bodies.len() < 100 {
            // Use sequential direct summation for small number of bodies
            DirectSummation::calculate_accelerations::<false>(self.grav_const, &bodies_sim)
        } else {
            MortonBasedOctree::calculate_accelerations::<true>(self.grav_const, &bodies_sim)
        };

        self.bodies.iter_mut().zip(a0).for_each(|(body, acc)| {
            let mut b = body.bind_mut();
            b.acc = Some(acc);
        });
    }
}

#[godot_api]
impl INode3D for GravityController {
    /// Handles node notifications from the Godot engine.
    fn on_notification(&mut self, notification: Node3DNotification) {
        use Node3DNotification::*;

        if let READY = notification {
            self.setup_editor();
        }

        if let READY | CHILD_ORDER_CHANGED = notification {
            self.get_gravity_bodies();
        }
    }

    /// Performs the physics update for all gravity bodies.
    ///
    /// This method is called every physics frame by the Godot engine and:
    /// 1. Creates lightweight simulation counterparts for all managed bodies
    /// 2. Simulates a single physics step using the current gravity constant
    /// 3. Applies the simulation results back to the actual bodies in the scene
    ///
    /// If no bodies are present, this method returns early to avoid unnecessary processing.
    ///
    /// # Parameters
    /// - `delta`: Time elapsed since the previous physics frame in seconds
    fn physics_process(&mut self, delta: f64) {
        if self.bodies.is_empty() {
            return;
        }

        // Create simulation counterparts of real bodies
        let mut bodies_sim = self.bodies.iter().map(SimulatedBody::from).collect_vec();

        // Simulate a physics step
        if self.leapfrog_integration {
            Self::step_time_leapfrog(self.grav_const, delta as f32, &mut bodies_sim);
        } else {
            Self::step_time(self.grav_const, delta as f32, &mut bodies_sim);
        }

        if let Some(ov) = self.octree_visualizer.as_mut() {
            // Update the octree visualizer with the simulated bodies
            let octree = MortonBasedOctree::new(&bodies_sim);

            ov.bind_mut().update_visualization(&octree);
        }

        // Apply simulated step to real bodies
        self.bodies
            .iter_mut()
            .zip(bodies_sim)
            .for_each(|(body, sim)| body.bind_mut().update_from_sim(&sim));
    }
}
