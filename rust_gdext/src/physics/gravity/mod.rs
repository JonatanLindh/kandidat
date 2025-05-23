//! Implementation of gravitational physics and orbital mechanics.
//!
//! This module provides components for simulating gravitational interactions between bodies
//! in 3D space, including force calculations, trajectory predictions, and visualization.

pub mod barnes_hut;
pub mod body;
pub mod controller;
pub mod direct_summation;
pub mod galaxy_controller;
pub mod trajectories;

use glam::Vec3A;

/// Common softening factor for gravitational calculations to prevent singularities.
/// This value is squared to avoid square roots in distance comparisons.
const GRAVITATIONAL_SOFTENING: f32 = 1e-2;
pub const GRAVITATIONAL_SOFTENING_SQUARED: f32 = GRAVITATIONAL_SOFTENING * GRAVITATIONAL_SOFTENING;

pub fn merge_radius(scaler: f32, m1: f32, m2: f32) -> f32 {
    scaler * (m1.log10() + m2.log10()).max(0.0) / 2.0
}

pub trait HasPosition {
    fn get_pos(&self) -> Vec3A;
    fn set_pos(&mut self, pos: Vec3A);
}

pub trait HasVelocity {
    fn get_vel(&self) -> Vec3A;
    fn set_vel(&mut self, vel: Vec3A);
}

pub trait HasMass {
    fn get_mass(&self) -> f32;
    fn set_mass(&mut self, mass: f32);
}

pub trait PosMass: HasPosition + HasMass {
    #[inline]
    fn weighted_pos(&self) -> Vec3A {
        if self.get_mass() > 0.0 {
            self.get_pos() * self.get_mass()
        } else {
            Vec3A::ZERO
        }
    }
}

impl<T: HasPosition + HasMass> PosMass for T {}

pub trait VelMass: HasVelocity + HasMass {
    #[inline]
    fn non_elastic_collision(&mut self, other: &Self) {
        let m1 = self.get_mass();
        let m2 = other.get_mass();
        let v1 = self.get_vel();
        let v2 = other.get_vel();

        // Non-elastic collision: momentum conservation
        let new_v1 = (m1 * v1 + m2 * v2) / (m1 + m2);
        self.set_vel(new_v1);

        // Update the mass of the current object
        self.set_mass(m1 + m2);
    }
}

impl<T: HasVelocity + HasMass> VelMass for T {}

pub trait NBodyGravityCalculator<T>
where
    T: PosMass,
{
    fn calc_accs<const PARALLEL: bool>(&self, g: f32) -> Vec<Vec3A>;
    fn detect_collisions(&self, merge_scaler: f32) -> Vec<(usize, usize)>;
}
