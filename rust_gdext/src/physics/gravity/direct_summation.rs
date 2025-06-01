use super::{GRAVITATIONAL_SOFTENING_SQUARED, NBodyGravityCalculator, PosMass, merge_radius};
use glam::Vec3A;
use godot::builtin::math::FloatExt;
use rayon::iter::{IntoParallelRefIterator, ParallelIterator}; // Added import

pub struct DirectSummation<'a, T> {
    particles: &'a [T],
}

impl<'a, T> DirectSummation<'a, T> {
    pub fn new(particles: &'a [T]) -> Self {
        Self { particles }
    }
}

impl<'a, T> NBodyGravityCalculator<T> for DirectSummation<'a, T>
where
    T: PosMass + Sync,
{
    fn calc_accs<const PARALLEL: bool>(&self, g: f32) -> Vec<Vec3A> {
        let particles = self.particles;

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

    fn detect_collisions(&self, merge_scaler: f32) -> Vec<(usize, usize)> {
        let particles = self.particles;

        let mut collisions = Vec::new();
        let len = particles.len();
        for i in 0..len {
            for j in (i + 1)..len {
                let a = &particles[i];
                let b = &particles[j];
                if a.get_pos().distance(b.get_pos())
                    < merge_radius(merge_scaler, a.get_mass(), b.get_mass())
                {
                    collisions.push((i, j));
                }
            }
        }

        collisions
    }
}

fn calc_acc<T: PosMass>(g: f32, body: &T, bodies: &[T]) -> Vec3A {
    bodies
        .iter()
        .map(|other| (other.get_pos() - body.get_pos(), other.get_mass()))
        .filter(|(diff, _)| !diff.length_squared().is_zero_approx())
        .map(|(diff, other_mass)| {
            let r2 = diff.length_squared();
            let r2_softened = r2 + GRAVITATIONAL_SOFTENING_SQUARED; // Apply softening using common constant

            let inv_r_softened_cubed = r2_softened.powf(-1.5); // 1 / (r_softened^3/2)

            diff * inv_r_softened_cubed * g * other_mass
        })
        .sum()
}
