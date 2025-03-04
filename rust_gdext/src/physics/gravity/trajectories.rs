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
use crate::physics::gravity::controller::__gdext_GravityController_Funcs;
use godot::{
    classes::{
        ArrayMesh, MeshInstance3D, StandardMaterial3D, SurfaceTool, base_material_3d::ShadingMode,
        mesh,
    },
    prelude::*,
};
use itertools::Itertools;
use std::mem;

/// Simple struct to hold information about a body's trajectory
struct Trajectory {
    color: Color,
    points: Vec<Vector3>,
}

#[godot_api]
impl GravityController {
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
            Self::step_time(grav_const, delta, &mut bodies_sim, &mut accelerations);

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

    /// Removes all trajectories currently displayed in the scene.
    ///
    /// This function clears the trajectory visualization without affecting other simulation settings.
    /// It does this by replacing the current trajectories with an empty vector.
    #[func]
    fn clear_trajectories(&mut self) {
        self.replace_trajectories(Vec::new());
    }

    /// Function connected to the signal `UPDATE_TRAJECTORIES`
    ///
    /// Updates trajectory visualizations if auto_update_trajectories is enabled
    #[func]
    fn _on_update_trajectories(&mut self) {
        if self.auto_update_trajectories {
            self.simulate_trajectories();
        }
    }

    /// Signal emitted when trajectories should be recalculated.
    ///
    /// This signal is used to notify the controller that trajectory predictions
    /// need to be updated, typically after changes to bodies or simulation parameters.
    #[signal]
    fn UPDATE_TRAJECTORIES();
    pub const UPDATE_TRAJECTORY_SIGNAL: &str = "UPDATE_TRAJECTORIES";
}

impl GravityController {
    /// Replaces existing trajectories with new ones, creating meshes for visualization.
    ///
    /// Each valid trajectory (with at least 2 points) is converted into a mesh instance
    /// and added as a child to the controller. Old trajectory meshes are properly freed.
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

    /// Builds a mesh representation of a trajectory for visualization.
    ///
    /// Creates a line strip mesh from trajectory points and a material with the
    /// trajectory's color. The material is set to unshaded rendering mode.
    ///
    ///  ### Returns
    /// A tuple containing:
    /// * The generated `ArrayMesh`
    /// * A `StandardMaterial3D` with appropriate color settings
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
