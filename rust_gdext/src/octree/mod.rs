pub mod lod_octree;
pub mod morton_based;
pub mod old_versions;
pub mod visualize;

use crate::physics::gravity::{Massive, Particle, Spacial, controller::SimulatedBody};
use core::array;
use either::Either;
use glam::Vec3A;
use old_versions::partition_based::Partition;

// --- Constants ---
const MAX_BODIES_PER_LEAF: usize = 1; // Standard for Barnes-Hut is 1 body per leaf
// Softening factor to prevent extreme forces at close range
const SOFTENING_SQUARED: f32 = 1e-4;
// Minimum node half-width to prevent infinite subdivision
const MIN_HALF_WIDTH: f32 = 1e-5;

#[derive(Clone, Debug, Default)]
pub struct GravityData {
    pub mass: f32,
    pub center_of_mass: Vec3A,
}

impl Spacial for GravityData {
    #[inline(always)]
    fn get_pos(&self) -> Vec3A {
        self.center_of_mass
    }
}

impl Massive for GravityData {
    #[inline(always)]
    fn get_mass(&self) -> f32 {
        self.mass
    }
}

impl From<&SimulatedBody> for GravityData {
    fn from(body: &SimulatedBody) -> Self {
        GravityData {
            mass: body.get_mass(),
            center_of_mass: body.get_pos(),
        }
    }
}

impl GravityData {
    #[inline]
    pub fn merge_reduce_fn<T: Particle>(self, other: &T) -> Self {
        let total_mass = self.mass + other.get_mass();
        Self {
            center_of_mass: (self.weighted_pos() + other.weighted_pos()) / total_mass,
            mass: total_mass,
        }
    }

    #[inline]
    pub fn merge<'a, T: Particle + 'a>(ds: impl IntoIterator<Item = &'a T>) -> Self {
        ds.into_iter()
            .fold(Self::default(), |acc, v| acc.merge_reduce_fn(v))
    }
}

#[derive(Clone, Copy, Debug)]
pub struct BoundingBox {
    pub center: Vec3A,
    pub half_width: f32, // Represents max half-width along any axis
}

impl Default for BoundingBox {
    fn default() -> Self {
        Self {
            center: Vec3A::ZERO,
            half_width: 1.0,
        }
    }
}

impl BoundingBox {
    pub fn containing_gravity_data(bodies: &[GravityData]) -> Self {
        let mut ps = bodies.iter().map(|b| b.center_of_mass);

        let p0 = match ps.next() {
            Some(pos) => pos,
            None => return Self::default(),
        };

        let (min_p, max_p) = bodies
            .iter()
            .skip(1)
            .map(|b| b.center_of_mass)
            .fold((p0, p0), |(min, max), pos| (min.min(pos), max.max(pos)));

        let center = (min_p + max_p) * 0.5;
        // Calculate the max extent from the center along any axis
        let extent = (max_p - min_p) * 0.5;
        let half_width = extent.max_element().max(MIN_HALF_WIDTH);

        BoundingBox { center, half_width }
    }

    // Method to compute the bounding box of multiple bodies
    pub fn containing<T: Spacial>(bodies: &[T]) -> Self {
        let mut ps = bodies.iter().map(|b| b.get_pos());

        let p0 = match ps.next() {
            Some(pos) => pos,
            None => return Self::default(),
        };

        let (min_p, max_p) = bodies
            .iter()
            .skip(1)
            .map(|b| b.get_pos())
            .fold((p0, p0), |(min, max), pos| (min.min(pos), max.max(pos)));

        let center = (min_p + max_p) * 0.5;
        // Calculate the max extent from the center along any axis
        let extent = (max_p - min_p) * 0.5;
        let half_width = extent.max_element().max(MIN_HALF_WIDTH);

        BoundingBox { center, half_width }
    }

    /// Method to determine which octant a point belongs to within this box
    #[inline]
    pub fn get_octant_index(&self, point: Vec3A) -> usize {
        let offset = point - self.center;

        ((offset.x > 0.0) as usize) << 2
            | ((offset.y > 0.0) as usize) << 1
            | ((offset.z > 0.0) as usize)
    }

    /// Method to determine which octant a point belongs to within this box.
    ///
    /// Uses nested Eithers to work with [`rayon::iter::ParallelIterator::partition_map`]
    #[inline]
    pub fn octant_either_partition(
        &self,
        point: GravityData,
    ) -> Partition<Partition<Partition<GravityData>>> {
        use Either::{Left as L, Right as R};
        // Use sign bit check for efficiency if possible, otherwise comparison
        let offset = point.center_of_mass - self.center;

        match ((offset.x > 0.0), (offset.y > 0.0), (offset.z > 0.0)) {
            (false, false, false) => L(L(L(point))),
            (false, false, true) => L(L(R(point))),
            (false, true, false) => L(R(L(point))),
            (false, true, true) => L(R(R(point))),
            (true, false, false) => R(L(L(point))),
            (true, false, true) => R(L(R(point))),
            (true, true, false) => R(R(L(point))),
            (true, true, true) => R(R(R(point))),
        }
    }

    // Method to calculate the center and half-width of a specific octant
    #[inline]
    pub fn get_octant_bounds(&self, octant_index: usize) -> Self {
        let child_half_width = self.half_width * 0.5;

        let offset_x = if (octant_index & 4) != 0 {
            child_half_width
        } else {
            -child_half_width
        };
        let offset_y = if (octant_index & 2) != 0 {
            child_half_width
        } else {
            -child_half_width
        };
        let offset_z = if (octant_index & 1) != 0 {
            child_half_width
        } else {
            -child_half_width
        };

        let child_center = self.center + Vec3A::new(offset_x, offset_y, offset_z);
        BoundingBox {
            center: child_center,
            half_width: child_half_width,
        }
    }

    #[inline]
    pub fn get_octant_bounds_morton(&self, octant_index: u8) -> Self {
        let child_half_width = self.half_width * 0.5;

        // Correct bit checks for ZYX mapping:
        // Bit 0 (value 1) -> X axis
        let offset_x = if (octant_index & 1) != 0 {
            child_half_width
        } else {
            -child_half_width
        };
        // Bit 1 (value 2) -> Y axis
        let offset_y = if (octant_index & 2) != 0 {
            child_half_width
        } else {
            -child_half_width
        };
        // Bit 2 (value 4) -> Z axis
        let offset_z = if (octant_index & 4) != 0 {
            child_half_width
        } else {
            -child_half_width
        };

        let child_center = self.center + Vec3A::new(offset_x, offset_y, offset_z);
        BoundingBox {
            center: child_center,
            half_width: child_half_width,
        }
    }

    pub fn subdivide(&self) -> [Self; 8] {
        array::from_fn(|i| self.get_octant_bounds(i))
    }
}
