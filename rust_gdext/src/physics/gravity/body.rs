use super::controller::{GravityController, SimulatedBody};
use godot::{builtin::math::ApproxEq, classes::notify::Node3DNotification, prelude::*};
use proc::editor;

#[derive(GodotClass)]
#[class(tool, init, base = Node3D)]
pub struct GravityBody {
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

                self.emit_update_trajectories();
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

    pub fn update_from_sim(&mut self, sim: &SimulatedBody) {
        self.mass = sim.mass;
        self.initial_velocity = sim.vel;
        self.base_mut().set_position(sim.pos);
    }

    /// Traverses up the node tree to find the parent `GravityController`.
    ///
    /// # Panics
    ///
    /// Panics if no `GravityController` is found in the parent hierarchy.
    fn locate_controller(&self) -> Gd<GravityController> {
        let mut current = self.to_gd().upcast::<Node>();

        while let Some(parent) = current.get_parent() {
            match parent.try_cast::<GravityController>() {
                Ok(controller) => return controller,
                Err(node) => current = node,
            }
        }

        godot_error!("No GravityController found in parent hierarchy");
        panic!("GravityController must be a parent of this GravityBody");
    }

    #[editor(only)]
    fn emit_update_trajectories(&mut self) {
        if !self.base().is_inside_tree() {
            return;
        }

        self.locate_controller().call_deferred(
            "emit_signal",
            &[GravityController::UPDATE_TRAJECTORY_SIGNAL.to_variant()],
        );
    }
}
