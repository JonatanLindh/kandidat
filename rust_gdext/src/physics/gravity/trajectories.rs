//! Trajectory visualization for the n-body gravity simulation.
//!
//! This module provides functionality to simulate and visualize the orbital paths
//! of celestial bodies in a gravitational system. It extends the `GravityController`
//! with methods to:
//!
//! - Generate predicted orbital trajectories through n-body simulation
//! - Visualize these trajectories as colored 3D paths
//! - Manage trajectory display including automatic updates
//!
//! Trajectories are represented as sequences of points in 3D space and rendered
//! using Godot's mesh rendering capabilities. The simulation can be centered on a
//! specific body to show relative motion from that body's perspective.
//!
//! The implementation uses `ArrayMesh` and `SurfaceTool` to efficiently generate
//! line-based visualizations of each body's predicted path, with customizable
//! colors for each trajectory.

use super::controller::{GravityController, SimulatedBody};
use crate::{
    from_glam_vec3, physics::gravity::controller::__gdext_GravityController_Funcs, worker::Worker,
};
use glam::Vec3A;
use godot::{
    classes::{
        ArrayMesh, MeshInstance3D, StandardMaterial3D, SurfaceTool, base_material_3d::ShadingMode,
        mesh,
    },
    prelude::*,
};
use itertools::Itertools;
use std::mem;

/// Represents a single body's trajectory path with visual styling information.
///
/// A trajectory consists of a collection of 3D points that form the predicted path
/// of a celestial body over time, along with a color used for visualization.
pub struct Trajectory {
    /// Color used to render this trajectory
    color: Color,
    /// Sequential 3D positions forming the predicted path
    points: Vec<Vec3A>,
}

/// Contains all necessary information for simulating body trajectories.
///
/// This struct encapsulates all the data needed to perform an n-body gravity simulation
/// for trajectory prediction, including the initial state of bodies, simulation parameters,
/// and reference frame information.
pub struct SimulationInfo {
    /// Current state of all bodies for simulation
    bodies_sim: Vec<SimulatedBody>,

    /// Trajectory data structures to populate during simulation
    trajectories: Vec<Trajectory>,

    /// Optional reference body index and initial position for relative trajectories
    /// When present, (index, initial_position) is used to make trajectories relative to the body
    offset_info: Option<(usize, Vec3A)>,

    /// Time increment per simulation step in seconds
    delta: f32,

    /// Gravitational constant for force calculations
    grav_const: f32,

    /// Number of steps to simulate
    n_steps: usize,

    leapfrog: bool,
}

/// Manages a background thread for trajectory calculations.
///
/// This worker handles trajectory simulations asynchronously, allowing the main game thread
/// to continue running smoothly while calculations are performed in the background.
pub type TrajectoryWorker = Worker<Vec<Trajectory>, TrajectoryCommand>;

/// Commands that can be sent to the trajectory worker thread.
///
/// This enum defines the communication protocol between the main thread and
/// the trajectory calculation thread.
pub enum TrajectoryCommand {
    /// Request to calculate trajectories with the provided simulation information
    Calculate(SimulationInfo),
    /// Signal the worker thread to terminate
    Shutdown,
}

#[godot_api]
impl GravityController {
    /// Enables trajectory visualization and starts the worker thread.
    ///
    /// This function initializes a background thread for trajectory calculation if not already running.
    /// It sets up communication channels and queues an initial trajectory calculation.
    #[func]
    fn enable_trajectories(&mut self) {
        // If the worker is already running, do nothing
        if self.trajectory_worker.is_some() {
            return;
        }

        // Create a new worker thread for trajectory calculations
        let worker = TrajectoryWorker::new(|cmd_receiver, result_tx| {
            while let Ok(batch) = cmd_receiver.recv_batch() {
                match batch.find_or_latest(|c| matches!(c, TrajectoryCommand::Shutdown)) {
                    TrajectoryCommand::Shutdown => break,

                    TrajectoryCommand::Calculate(info) => {
                        let trajectories = Self::simulate_trajectories_inner(info);

                        if let Err(e) = result_tx.send(trajectories) {
                            godot_error!("Failed to send trajectory results: {}", e);
                        }
                    }
                }
            }
        });

        self.trajectory_worker = Some(worker);

        // Queue the first trajectory calculation
        self.queue_simulate_trajectories();
    }

    /// Disables trajectory visualization and shuts down the worker thread.
    ///
    /// This function gracefully terminates the background trajectory calculation thread
    /// and removes any existing trajectory visualizations from the scene.
    #[func]
    fn disable_trajectories(&mut self) {
        if let Some(worker) = self.trajectory_worker.take() {
            worker
                .send_command(TrajectoryCommand::Shutdown)
                .unwrap_or_else(|e| godot_error!("Failed to send shutdown command: {}", e));

            worker.join().unwrap_or_else(|_| {
                godot_error!("Failed to join trajectory worker thread on shutdown")
            });
        }

        self.clear_trajectories();
    }

    /// Queues a new trajectory calculation request to the worker thread.
    ///
    /// This function collects the current simulation parameters and body states,
    /// and sends them to the worker thread for asynchronous trajectory calculation.
    #[func]
    fn queue_simulate_trajectories(&mut self) {
        if let Some(worker) = &self.trajectory_worker {
            let info = self.get_simulation_info();
            let _ = worker.send_command(TrajectoryCommand::Calculate(info));
        }
    }

    /// Polls for and applies any newly calculated trajectories.
    ///
    /// This function should be called periodically from the main thread to check if
    /// the worker thread has completed any trajectory calculations. If new trajectories
    /// are available, they are used to update the visualization.
    #[func]
    fn poll_trajectory_results(&mut self) {
        if let Some(trajectories) = self
            .trajectory_worker
            .as_ref()
            .and_then(|worker| worker.try_recv_latest())
        {
            self.replace_trajectories(trajectories)
        }
    }

    /// Simulates and visualizes orbital trajectories for all registered celestial bodies.
    ///
    /// This function performs an n-body gravity simulation for the specified number of steps,
    /// generating trajectory data for each body in the system. The simulation:
    ///
    /// - Uses the configured gravity constant, number of steps, and time delta
    /// - Calculates the gravitational interactions between all bodies
    /// - Optionally centers the visualization on a specified body (if `sim_center_body` is set)
    /// - Stores position data at each step to create visual trajectory paths
    /// - Updates the scene with the newly calculated trajectories
    ///
    /// After simulation, the trajectories are visualized as colored paths showing the predicted
    /// orbital movement of each body.
    #[func]
    fn simulate_trajectories(&mut self) {
        let info = self.get_simulation_info();
        let trajectories = Self::simulate_trajectories_inner(info);
        self.replace_trajectories(trajectories);
    }

    /// Removes all trajectories currently displayed in the scene.
    ///
    /// This function clears the trajectory visualization without affecting other simulation settings.
    /// It does this by replacing the current trajectories with an empty vector.
    #[func]
    fn clear_trajectories(&mut self) {
        self.replace_trajectories(Vec::new());
    }

    /// Updates trajectory visualizations if auto_update_trajectories is enabled
    #[func]
    fn update_trajectories(&mut self) {
        if self.auto_update_trajectories {
            self.simulate_trajectories();
        }
    }
}

impl GravityController {
    /// Collects simulation parameters and prepares data for trajectory calculation.
    ///
    /// This function gathers the current state of all bodies, creates empty trajectory
    /// structures, and determines the reference frame for relative trajectories if needed.
    ///
    /// # Returns
    ///
    /// A `SimulationInfo` struct containing all data needed for trajectory simulation.
    fn get_simulation_info(&self) -> SimulationInfo {
        let n_steps = self.simulation_steps as usize;
        let delta = self.simulation_step_delta;
        let grav_const = self.grav_const;

        let (bodies_sim, trajectories): (Vec<_>, Vec<_>) = self
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

        SimulationInfo {
            bodies_sim,
            trajectories,
            offset_info,
            delta,
            grav_const,
            n_steps,
            leapfrog: self.leapfrog_integration,
        }
    }

    /// Performs the n-body gravity simulation to generate trajectory data.
    ///
    /// This function runs the actual physics simulation, updating positions and velocities
    /// for all bodies over multiple time steps and recording the resulting trajectories.
    ///
    /// # Parameters
    ///
    /// * `SimulationInfo` - Contains all simulation parameters and bodies' initial states
    ///
    /// # Returns
    ///
    /// A vector of `Trajectory` objects containing the simulated orbital paths
    fn simulate_trajectories_inner(
        SimulationInfo {
            mut bodies_sim,
            mut trajectories,
            offset_info,
            delta,
            grav_const,
            n_steps,
            leapfrog,
        }: SimulationInfo,
    ) -> Vec<Trajectory> {
        for _ in 1..n_steps {
            // Step
            if leapfrog {
                Self::step_time_leapfrog(grav_const, delta, &mut bodies_sim);
            } else {
                Self::step_time(grav_const, delta, &mut bodies_sim);
            }

            let offset = offset_info
                .map(|(idx, init)| bodies_sim[idx].pos - init)
                .unwrap_or(Vec3A::ZERO);

            // Store positions
            for (i, body) in bodies_sim.iter().enumerate() {
                trajectories[i].points.push(body.pos - offset);
            }
        }

        trajectories
    }

    /// Replaces existing trajectories with new ones, creating meshes for visualization.
    ///
    /// Each valid trajectory (with at least 2 points) is converted into a mesh instance
    /// and added as a child to the controller. Old trajectory meshes are properly freed.
    ///
    /// # Parameters
    ///
    /// * `new_trajectories` - Vector of trajectories to visualize
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

    /// Creates a mesh and material for visualizing a trajectory.
    ///
    /// This function converts a trajectory's points into a line strip mesh and creates
    /// a material with the trajectory's color for rendering.
    ///
    /// # Parameters
    ///
    /// * `trajectory` - The trajectory to convert into a mesh
    ///
    /// # Returns
    ///
    /// A tuple containing:
    /// * The generated mesh for the trajectory
    /// * A material with the trajectory's color
    fn build_trajectory_mesh(trajectory: &Trajectory) -> (Gd<ArrayMesh>, Gd<StandardMaterial3D>) {
        let mut surface_tool = SurfaceTool::new_gd();
        surface_tool.begin(mesh::PrimitiveType::LINE_STRIP);

        for &point in &trajectory.points {
            surface_tool.add_vertex(from_glam_vec3(point));
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
