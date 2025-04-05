//! Implementation of gravitational physics and orbital mechanics.
//!
//! This module provides components for simulating gravitational interactions between bodies
//! in 3D space, including force calculations, trajectory predictions, and visualization.

pub mod barnes_hut;
pub mod body;
pub mod controller;
pub mod trajectories;

use glam::Vec3A;

pub trait Spacial {
    fn get_pos(&self) -> Vec3A;
}

pub trait Massive {
    fn get_mass(&self) -> f32;
}

pub trait Particle: Spacial + Massive {
    #[inline]
    fn weighted_pos(&self) -> Vec3A {
        if self.get_mass() > 0.0 {
            self.get_pos() * self.get_mass()
        } else {
            Vec3A::ZERO
        }
    }
}

impl<T: Spacial + Massive> Particle for T {}

pub trait NBodyGravityCalculator<T: Particle> {
    fn step_time(&self, particles: &[T]) -> Vec<Vec3A>;
}
