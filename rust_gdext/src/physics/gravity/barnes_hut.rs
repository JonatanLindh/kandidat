use super::{NBodyGravityCalculator, Particle};
use crate::octree::{GravityData, morton_based::MortonBasedOctree};
use glam::Vec3A;
use rayon::iter::{IntoParallelIterator, ParallelIterator};
use std::{assert_matches::debug_assert_matches, marker::Sync};

// --- Constants for Barnes-Hut ---

/// Barnes-Hut opening angle parameter
const THETA: f32 = 0.7;
const THETA_SQ: f32 = THETA * THETA;

/// Softening factor to prevent infinite forces at zero distance
const EPSILON: f32 = 0.01;
const EPSILON_SQ: f32 = EPSILON * EPSILON;

impl<'a, T> NBodyGravityCalculator<'a, T> for MortonBasedOctree<'a, T>
where
    T: Particle + Sync,
{
    /// Calculates the accelerations of particles using the Barnes-Hut algorithm.
    ///
    /// Note
    ///
    /// The PARELLEL constant determines whether the force calculations are done in parallel.
    /// The octree determines its own parallelism based on the number of particles.
    fn calculate_accelerations<const PARALLEL: bool>(g: f32, particles: &'a [T]) -> Vec<Vec3A> {
        if particles.is_empty() {
            return Vec::new();
        }

        // --- Barnes-Hut Algorithm ---
        // 1. Build the octree from the particles
        let octree = MortonBasedOctree::new(particles);

        // 2. For each particle, calculate the acceleration using the octree
        if PARALLEL {
            (0..particles.len())
                .into_par_iter()
                .map(|i| octree.calculate_accel_on_particle(g, i))
                .collect()
        } else {
            (0..particles.len())
                .map(|i| octree.calculate_accel_on_particle(g, i))
                .collect()
        }
    }
}

impl<'a, T: Particle + Sync> MortonBasedOctree<'a, T> {
    /// Calculates the total acceleration on a single target particle using Barnes-Hut.
    #[inline]
    fn calculate_accel_on_particle(&self, g: f32, target_particle_index: usize) -> Vec3A {
        debug_assert_matches!(
            self.root_index,
            Some(idx) if idx < self.nodes.len(),
            "This function should only be called on a valid octree with a root node."
        );

        let target_particle = &self.data_ref[target_particle_index];
        let target_pos = target_particle.get_pos();

        // Start the recursive calculation from the root node
        self.calculate_accel_recursive(
            g,
            self.root_index.unwrap(),
            target_particle_index,
            target_pos,
        )
    }

    /// Recursive helper function for Barnes-Hut traversal.
    fn calculate_accel_recursive(
        &self,
        g: f32,
        current_node_index: usize,
        target_particle_index: usize, // Index of the particle we're calculating for
        target_pos: Vec3A,            // Position of that particle
    ) -> Vec3A {
        // Get the node we're currently considering
        let node = &self.nodes[current_node_index];
        let GravityData {
            mass: node_mass,
            center_of_mass: node_com,
        } = node.data;

        // Get the node's bounding box
        // Vector from target particle to the node's center of mass
        let delta_pos = node_com - target_pos;
        let dist_sq = delta_pos.length_squared();

        // --- Check MAC (Multipole Acceptance Criterion) ---
        // Size 's' of the node (e.g., width of its bounding box)
        let node_width = node.bounds.half_width * 2.0;
        let s_sq = node_width * node_width;

        // If distance is zero (or very small), skip interaction (might be self or coincident)
        if dist_sq < EPSILON_SQ * 0.1 {
            // Special case: If it's a leaf node containing *only* the target particle,
            // we definitely skip. Otherwise, if it's an internal node or a leaf
            // with other particles, the recursive calls/direct interactions below
            // will handle skipping the self-interaction pair.
            // This simple check mainly avoids division by zero for coincident CoM.
            if node.body_range.len() == 1
                && node.body_range.start < self.sorted_indices.len()
                && self.sorted_indices[node.body_range.start].item == target_particle_index
            {
                return Vec3A::ZERO;
            }
            // Otherwise, proceed, self-interaction check below will handle it.
        }

        // MAC: s^2 / d^2 < theta^2  or s^2 < theta^2 * d^2
        if s_sq < THETA_SQ * dist_sq {
            // --- Node is far enough: Use approximation ---
            // Calculate acceleration contribution from this node's CoM
            // acc = G * M_node * delta_pos / (|delta_pos|^2 + eps^2)^(3/2)
            let dist = (dist_sq + EPSILON_SQ).sqrt(); // Add softening
            let dist_cubed = dist * dist * dist;
            if dist_cubed < 1e-18 {
                return Vec3A::ZERO;
            } // Avoid division by near-zero cubed distance

            let acc_contribution = (g * node_mass / dist_cubed) * delta_pos;
            return acc_contribution;
        }

        // --- Node is too close: Recurse or calculate directly ---
        if let Some(children) = node.children {
            // --- Internal Node: Recurse on children ---
            let total_acc = children
                .iter()
                .flatten()
                .map(|child_index_nz| child_index_nz.get())
                .inspect(|&child_index| {
                    // Debug assertion to ensure child index is within bounds
                    // Will not be included in release builds
                    debug_assert!(child_index < self.nodes.len(), "Child index out of bounds");
                })
                .map(|child_index| {
                    self.calculate_accel_recursive(
                        g,
                        child_index,
                        target_particle_index,
                        target_pos,
                    )
                })
                .sum();

            return total_acc;
        }

        // --- Leaf Node: Direct Calculation ---

        node.body_range
            .clone()
            .map(|i| {
                // Debug assertion to ensure index is within bounds
                // Will not be included in release builds
                debug_assert!(i < self.sorted_indices.len(), "Index out of bounds");

                self.sorted_indices[i].item
            }) // Get the actual particle index
            .filter(|&index| index != target_particle_index) // Filter out the target particle
            .map(|i| {
                // Debug assertion to ensure actual particle index is within bounds
                // Will not be included in release builds
                debug_assert!(i < self.data_ref.len(), "Index out of bounds");

                // Get the particle data from the original reference slice
                let particle = &self.data_ref[i];
                let particle_pos = particle.get_pos();
                let particle_mass = particle.get_mass();

                // Calculate direct interaction
                let direct_delta_pos = particle_pos - target_pos;
                let direct_dist_sq = direct_delta_pos.length_squared();

                // Skip if coincident
                if direct_dist_sq < EPSILON_SQ * 0.1 {
                    return Vec3A::ZERO;
                }

                let direct_dist = (direct_dist_sq + EPSILON_SQ).sqrt(); // Softening
                let direct_dist_cubed = direct_dist * direct_dist * direct_dist;
                if direct_dist_cubed < 1e-18 {
                    return Vec3A::ZERO; // Avoid division by zero
                }

                (g * particle_mass / direct_dist_cubed) * direct_delta_pos
            })
            .sum()
    }
}
