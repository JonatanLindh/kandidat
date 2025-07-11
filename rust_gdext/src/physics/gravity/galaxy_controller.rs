use super::{HasMass, HasPosition, HasVelocity, NBodyGravityCalculator};
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

    #[init(node = "../GalaxyPhysicsBridge")]
    bridge: OnReady<Gd<Node>>,

    bridge_initialized: bool,
}

#[derive(Debug, Clone)]
struct StarData {
    position: Vec3A,
    velocity: Vec3A,
    mass: f32,
}

impl HasPosition for StarData {
    #[inline(always)]
    fn get_pos(&self) -> Vec3A {
        self.position
    }

    #[inline(always)]
    fn set_pos(&mut self, pos: Vec3A) {
        self.position = pos;
    }
}

impl HasVelocity for StarData {
    #[inline(always)]
    fn get_vel(&self) -> Vec3A {
        self.velocity
    }

    #[inline(always)]
    fn set_vel(&mut self, vel: Vec3A) {
        self.velocity = vel;
    }
}

impl HasMass for StarData {
    #[inline(always)]
    fn get_mass(&self) -> f32 {
        self.mass
    }

    #[inline(always)]
    fn set_mass(&mut self, mass: f32) {
        self.mass = mass;
    }
}

#[godot_api]
impl INode for GalaxyController {
    fn physics_process(&mut self, delta: f64) {
        let stars = match &mut self.stars {
            Some(s) => s,

            // Will be attempted again next frame
            None => {
                self.get_stars();
                return;
            }
        };

        let accs = MortonBasedOctree::new(stars).calc_accs::<true>(self.grav_const);
        for (star, acc) in stars.iter_mut().zip(accs.iter()) {
            star.velocity += acc * delta as f32;
            star.position += star.velocity * delta as f32;
        }

        let vels = stars
            .iter()
            .map(|star| from_glam_vec3(star.velocity))
            .collect::<PackedVector3Array>();

        self.bridge.call("apply_velocities", &[vels.to_variant()]);
    }
}

impl GalaxyController {
    fn get_stars(&mut self) {
        if !self.bridge_initialized {
            self.bridge_initialized = self.bridge.get("init").booleanize();
            return;
        }

        match self
            .bridge
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
