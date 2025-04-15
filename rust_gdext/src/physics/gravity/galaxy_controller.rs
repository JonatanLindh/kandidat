use super::{Massive, NBodyGravityCalculator, Spacial};
use crate::{from_glam_vec3, octree::morton_based::MortonBasedOctree, to_glam_vec3};
use glam::Vec3A;
use godot::prelude::*;

#[derive(GodotClass)]
#[class(init, base = Node)]
pub struct GalaxyController {
    base: Base<Node>,

    /// The gravitational constant used in force calculations
    #[export]
    #[init(val = 1.0)]
    pub grav_const: f32,

    /// Time increment (in seconds) between each simulation step
    #[export]
    #[init(val = 0.3)]
    pub simulation_step_delta: f32,

    stars: Option<Vec<StarData>>,
    bridge: Option<Gd<Node>>,
    bridge_initialized: bool,
}

#[derive(Debug, Clone)]
struct StarData {
    position: Vec3A,
    velocity: Vec3A,
    mass: f32,
}

impl Spacial for StarData {
    #[inline(always)]
    fn get_pos(&self) -> Vec3A {
        self.position
    }
}

impl Massive for StarData {
    #[inline(always)]
    fn get_mass(&self) -> f32 {
        self.mass
    }
}

#[godot_api]
impl INode for GalaxyController {
    fn ready(&mut self) {
        self.bridge = self.base().get_node_or_null("../GalaxyPhysicsBridge");

        if self.bridge.is_none() {
            godot_error!("GalaxyPhysicsBridge node not found in the scene tree.");
            self.base_mut().queue_free();
        }
    }

    fn physics_process(&mut self, delta: f64) {
        let stars = match &mut self.stars {
            Some(s) => s,

            // Will be attempted again next frame
            None => {
                self.get_stars();
                return;
            }
        };

        let accs = MortonBasedOctree::calculate_accelerations::<true>(self.grav_const, stars);
        for (star, acc) in stars.iter_mut().zip(accs.iter()) {
            star.velocity += acc * delta as f32;
            star.position += star.velocity * delta as f32;
        }

        let vels = stars
            .iter()
            .map(|star| from_glam_vec3(star.velocity))
            .collect::<PackedVector3Array>();

        self.get_bridge()
            .call("apply_velocities", &[vels.to_variant()]);
    }
}

impl GalaxyController {
    #[inline]
    fn get_bridge(&self) -> Gd<Node> {
        self.bridge
            .as_ref()
            .expect("Expected bridge to exist")
            .clone()
    }

    fn get_stars(&mut self) {
        let bridge = self.get_bridge();

        if !self.bridge_initialized {
            self.bridge_initialized = bridge.get("init").booleanize();
            return;
        }

        match bridge
            .get("stars")
            .try_to::<Dictionary>()
            .ok()
            .and_then(|dict| {
                let positions = dict.get("position")?.try_to::<PackedVector3Array>().ok()?;
                let velocities = dict.get("velocity")?.try_to::<PackedVector3Array>().ok()?;
                let masses = dict.get("mass")?.try_to::<PackedFloat32Array>().ok()?;

                let stars = positions
                    .to_vec()
                    .into_iter()
                    .zip(velocities.to_vec())
                    .zip(masses.to_vec())
                    .map(|((pos, vel), mass)| StarData {
                        position: to_glam_vec3(pos),
                        velocity: to_glam_vec3(vel),
                        mass,
                    })
                    .collect::<Vec<_>>();

                Some(stars)
            }) {
            Some(stars) => {
                self.stars = Some(stars);
            }
            None => {
                godot_error!("Failed to retrieve stars from the bridge.");
                self.base_mut().queue_free();
            }
        }
    }
}
