use super::controller::{
    __gdext_GravityController_Funcs as GravityController_Funcs, GravityController, SimulatedBody,
};
use crate::from_glam_vec3;
use glam::Vec3A;
use godot::{builtin::math::ApproxEq, classes::notify::Node3DNotification, prelude::*};
use proc::editor;

/// A gravity-affected node in a physics simulation.
///
/// `GravityBody` represents an object with mass and velocity that interacts with a [`GravityController`].
/// It automatically connects to a parent controller when added to the scene tree and notifies
/// the controller when its properties change to update trajectory calculations.
///
///
/// # Notes
/// This node automatically detects its parent [`GravityController`] and registers with it.
/// Position changes are tracked to update simulations and trajectory visualizations.
#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
pub struct GravityBody {
    /// The mass of the body
    #[export]
    #[var(get, set = set_mass)]
    #[init(val = 1.0)]
    pub mass: f32,

    /// The current velocity vector of the body
    #[export]
    #[var(get, set = set_velocity)]
    pub velocity: Vector3,

    pub acc: Option<Vec3A>,

    /// The color used to render this body's trajectory
    #[export]
    pub trajectory_color: Color,

    /// The ancestor controller, if any
    controller: Option<Gd<GravityController>>,

    /// Track the last position to detect changes
    last_position: Vector3,

    base: Base<Node3D>,
}

#[godot_api]
impl INode3D for GravityBody {
    fn on_notification(&mut self, notification: Node3DNotification) {
        use Node3DNotification::*;

        // Setup
        if READY == notification {
            self.setup_editor();
        }

        // Locate controller
        if let READY | PATH_RENAMED | PARENTED = notification {
            self.locate_controller();
        }

        // Update trajectories
        if let READY | EXIT_TREE | TRANSFORM_CHANGED = notification {
            match notification {
                // Init last_position
                READY => self.last_position = self.base().get_position(),

                // Update last_position
                TRANSFORM_CHANGED => self.update_last_position(),

                _ => {}
            }

            self.emit_update_trajectories();
        }
    }
}

#[godot_api]
impl GravityBody {
    #[func]
    pub fn set_velocity(&mut self, value: Vector3) {
        self.velocity = value;
        self.emit_update_trajectories();
    }

    #[func]
    pub fn set_mass(&mut self, value: f32) {
        self.mass = value;
        self.emit_update_trajectories();
    }
}

impl GravityBody {
    #[editor(only)]
    fn setup_editor(&mut self) {
        // Notify on transforms in editor (for simulation updates)
        self.base_mut().set_notify_transform(true);
    }

    #[inline]
    fn update_last_position(&mut self) {
        let current_pos = self.base().get_position();

        if current_pos.approx_eq(&self.last_position) {
            return;
        }

        self.last_position = current_pos;
    }

    pub fn update_from_sim(&mut self, sim: &SimulatedBody) {
        self.mass = sim.mass;
        self.velocity = from_glam_vec3(sim.vel);
        self.base_mut().set_position(from_glam_vec3(sim.pos));
    }

    /// Traverses up the node tree to find the parent `GravityController`.
    fn locate_controller(&mut self) {
        let mut current = self.to_gd().upcast::<Node>();

        while let Some(parent) = current.get_parent() {
            match parent.try_cast::<GravityController>() {
                Ok(controller) => {
                    self.controller = Some(controller);
                    break;
                }
                Err(node) => current = node,
            }
        }
    }

    #[editor(only)]
    fn emit_update_trajectories(&mut self) {
        if !self.base().is_inside_tree() {
            return;
        }

        if let Some(controller) = self.controller.as_mut() {
            controller.call_deferred(GravityController_Funcs::update_trajectories, &[]);
        }
    }
}
