use glam::Vec3A;
use godot::builtin::math::FloatExt;
use rayon::iter::{IntoParallelRefIterator, ParallelIterator};

use super::{NBodyGravityCalculator, Particle};

pub struct DirectSummation;

impl<'a, T> NBodyGravityCalculator<'a, T> for DirectSummation
where
    T: Particle + Sync,
{
    fn calculate_accelerations<const PARALLEL: bool>(g: f32, particles: &'a [T]) -> Vec<Vec3A> {
        if particles.is_empty() {
            return Vec::new();
        }

        // --- Direct Summation ---
        if PARALLEL {
            particles
                .par_iter()
                .map(|body| calc_acc(g, body, particles))
                .collect()
        } else {
            particles
                .iter()
                .map(|body| calc_acc(g, body, particles))
                .collect()
        }
    }
}

fn calc_acc<T: Particle>(g: f32, body: &T, bodies: &[T]) -> Vec3A {
    bodies
        .iter()
        .map(|other| (other.get_pos() - body.get_pos(), other.get_mass()))
        .filter(|(diff, _)| !diff.length_squared().is_zero_approx())
        .map(|(diff, other_mass)| {
            let r2 = diff.length_squared();
            let dir = diff.normalize();

            g * dir * (other_mass) / r2
        })
        .sum()
}
