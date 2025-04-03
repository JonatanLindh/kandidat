pub mod morton;
pub mod parallel;
pub mod visualize;

use core::array;
use either::Either;
use glam::Vec3A;
use godot::prelude::{godot_error, godot_warn};
use std::num::NonZeroUsize;
use visualize::VisualizeOctree;

use crate::physics::gravity::controller::SimulatedBody;

// --- Constants ---
const MAX_BODIES_PER_LEAF: usize = 1; // Standard for Barnes-Hut is 1 body per leaf
// Softening factor to prevent extreme forces at close range
const SOFTENING_SQUARED: f32 = 1e-4;
// Minimum node half-width to prevent infinite subdivision
const MIN_HALF_WIDTH: f32 = 1e-5;
// Maximum recursion depth during insertion
const MAX_INSERT_DEPTH: u32 = 64;

// --- Data Structures ---
pub trait Spacial {
    fn get_pos(&self) -> Vec3A;
}

pub trait Massive {
    fn get_mass(&self) -> f32;
}

#[derive(Clone, Debug, Default)]
pub struct GravityData {
    pub mass: f32,
    pub center_of_mass: Vec3A,
}

impl Spacial for GravityData {
    fn get_pos(&self) -> Vec3A {
        self.center_of_mass
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
    fn merge(ds: &[Self]) -> Self {
        let total_mass = ds.iter().map(|d| d.mass).sum();

        let total_mass_inv = if total_mass > 0.0 {
            1.0 / total_mass
        } else {
            0.0
        };

        let center_of_mass = ds
            .iter()
            .map(|d| d.center_of_mass * d.mass * total_mass_inv)
            .reduce(|a, b| a + b)
            .unwrap_or_default();

        GravityData {
            mass: total_mass,
            center_of_mass,
        }
    }
}

/// Represents the different types of nodes in the octree arena.
#[derive(Clone, Debug)]
pub enum NodeData {
    Internal(InternalData),
    Leaf(LeafData),
    /// Represents an unused slot in the arena.
    Unused,
}

/// Data specific to an internal node.
#[derive(Clone, Debug)]
pub struct InternalData {
    pub bounds: BoundingBox,
    /// Index 0 is the root node, which cannot be a child.
    pub children: [Option<NonZeroUsize>; 8],
}

/// Data specific to a leaf node.
#[derive(Clone, Debug)]
pub struct LeafData {
    pub bounds: BoundingBox,
    pub positionable_indices: Vec<usize>, // Keep as Vec for potential MAX_BODIES > 1
}

/// The main Octree structure using an arena allocator (Vec<NodeData>).
#[derive(Debug)]
pub struct Octree<'a, T: Spacial> {
    nodes: Vec<NodeData>,
    root_index: usize,
    bounds: BoundingBox,
    external_data: &'a [T],
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

type Partition<T> = Either<T, T>;

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
    pub fn get_octant_bounds_morton(&self, octant_index: usize) -> Self {
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

impl VisualizeOctree for Octree<'_, GravityData> {
    fn get_bounds_and_depths(&self) -> Vec<(BoundingBox, u32)> {
        let mut result = Vec::new();
        let mut stack: Vec<(usize, u32)> = Vec::new();
        stack.push((self.root_index, 0));

        while let Some((node_index, depth)) = stack.pop() {
            if let Some(node) = self.get_node(node_index) {
                let bounds = match node {
                    NodeData::Internal(internal_data) => internal_data.bounds,
                    NodeData::Leaf(leaf_data) => leaf_data.bounds,
                    NodeData::Unused => continue,
                };
                result.push((bounds, depth));

                if let NodeData::Internal(internal_data) = node {
                    for &child_index in internal_data.children.iter() {
                        if let Some(child_index) = child_index {
                            stack.push((child_index.into(), depth + 1));
                        }
                    }
                }
            }
        }

        result
    }
}

// --- Implementation ---

impl<'a, T: Spacial> Octree<'a, T> {
    /// Creates a new, empty Octree.
    pub fn new(external_data: &'a [T]) -> Self {
        Octree {
            nodes: Vec::with_capacity(external_data.len() * 2), // Guess capacity
            root_index: usize::MAX,
            bounds: BoundingBox {
                center: Vec3A::ZERO,
                half_width: 1.0,
            },
            external_data,
        }
    }

    /// Gets a reference to a node using its index.
    #[inline]
    fn get_node(&self, index: usize) -> Option<&NodeData> {
        self.nodes.get(index)
    }

    /// Gets a mutable reference to a node using its index.
    #[inline]
    fn get_node_mut(&mut self, index: usize) -> Option<&mut NodeData> {
        self.nodes.get_mut(index)
    }

    /// Adds a node to the arena and returns its index.
    #[inline]
    fn add_node(&mut self, node_data: NodeData) -> usize {
        let index = self.nodes.len();
        self.nodes.push(node_data);
        index
    }

    /// Builds the octree from a list of simulated bodies.
    pub fn build(&mut self) {
        self.nodes.clear();
        if self.external_data.is_empty() {
            self.root_index = usize::MAX; // No root if no bodies
            return;
        }

        // 1. Compute bounds
        self.bounds = BoundingBox::containing(self.external_data);

        // 2. Initialize root node (as empty leaf initially, easier to insert into)
        let root_node_data = LeafData {
            bounds: self.bounds,
            positionable_indices: Vec::new(), // Start empty
        };
        self.root_index = self.add_node(NodeData::Leaf(root_node_data));

        // 3. Insert all bodies
        for (body_index, body) in self.external_data.iter().enumerate() {
            // Pass the bodies slice reference needed for potential subdivisions
            self.insert(self.root_index, body_index, body, self.external_data, 0);
        }
    }

    /// Inserts a body into the octree, starting from the given node index.
    /// `depth` is used to prevent infinite recursion.
    fn insert(
        &mut self,
        node_index: usize,
        body_sim_index: usize,
        positionable_to_insert: &T,
        all_positionables: &[T], // Needed for subdivisions
        depth: u32,
    ) {
        if depth > MAX_INSERT_DEPTH {
            eprintln!(
                "Max insertion depth ({}) reached, skipping body index {}.",
                MAX_INSERT_DEPTH, body_sim_index
            );
            godot_warn!(
                "Max insertion depth ({}) reached, skipping body index {}.",
                MAX_INSERT_DEPTH,
                body_sim_index
            );
            return;
        }

        // Get current node's center and half_width safely
        let (_current_center, current_half_width) = match self.get_node(node_index) {
            Some(NodeData::Internal(d)) => (d.bounds.center, d.bounds.half_width),
            Some(NodeData::Leaf(d)) => (d.bounds.center, d.bounds.half_width),
            _ => {
                eprintln!("Invalid node index {} during insert.", node_index);
                godot_error!("Invalid node index {} during insert.", node_index);
                return;
            }
        };

        // Prevent subdivision below a certain size
        if current_half_width < MIN_HALF_WIDTH {
            // If it's already a leaf, try adding the body (if space permits)
            if let Some(NodeData::Leaf(leaf_data)) = self.get_node_mut(node_index) {
                if !leaf_data.positionable_indices.contains(&body_sim_index) {
                    // Avoid duplicates
                    leaf_data.positionable_indices.push(body_sim_index);
                }
            } else {
                // It's an internal node that's too small, treat as a leaf for insertion
                godot_warn!(
                    "Node {} too small to subdivide, merging into effective leaf.",
                    node_index
                );
                // This case needs careful handling - maybe convert back to leaf?
                // For now, just skip inserting here, forcing it into a sibling/parent later?
            }
            return;
        }

        // --- Main Insertion Logic ---
        let node_data = self.nodes[node_index].clone(); // Clone to allow modifications below
        let body_pos = positionable_to_insert.get_pos();

        match node_data {
            NodeData::Internal(mut internal_data) => {
                // Determine which octant the body belongs to
                let octant_index = internal_data.bounds.get_octant_index(body_pos);

                let child_node_index = internal_data.children[octant_index];

                if let Some(child_node_index) = child_node_index {
                    // Descend into the existing child node
                    self.insert(
                        child_node_index.into(),
                        body_sim_index,
                        positionable_to_insert,
                        all_positionables,
                        depth + 1,
                    );
                } else {
                    // Create a new leaf node for this body
                    let child_bounds = internal_data.bounds.get_octant_bounds(octant_index);

                    let new_leaf_data = LeafData {
                        bounds: child_bounds,
                        positionable_indices: vec![body_sim_index],
                    };
                    let new_node_index = self.add_node(NodeData::Leaf(new_leaf_data));

                    // Link the new leaf node as a child
                    internal_data.children[octant_index] = Some(
                        NonZeroUsize::new(new_node_index).expect("Child's index cannot be zero"),
                    );
                    // Update the current node *in the arena*
                    self.nodes[node_index] = NodeData::Internal(internal_data);
                }
            }
            NodeData::Leaf(leaf_data) => {
                // --- Subdivide Leaf or Add Body ---
                if leaf_data.positionable_indices.is_empty()
                    || leaf_data.positionable_indices.len() < MAX_BODIES_PER_LEAF
                {
                    // Leaf is empty or has space, just add the body index
                    // Need mutable access to the node in the arena
                    if let Some(NodeData::Leaf(leaf_mut)) = self.get_node_mut(node_index) {
                        if !leaf_mut.positionable_indices.contains(&body_sim_index) {
                            leaf_mut.positionable_indices.push(body_sim_index);
                        }
                    } // Insertion finished
                } else {
                    // --- Subdivide ---
                    let existing_body_indices = leaf_data.positionable_indices.clone(); // Bodies already here

                    // Create new internal node data
                    let new_internal_data = InternalData {
                        bounds: leaf_data.bounds,
                        children: [None; 8], // No children initially
                    };

                    // Replace the current leaf node with the new internal node
                    self.nodes[node_index] = NodeData::Internal(new_internal_data);

                    // Re-insert the existing body/bodies from the former leaf into the NEW internal node
                    for &existing_body_index in &existing_body_indices {
                        // Get the body data from the main simulation list
                        let existing_body = &all_positionables[existing_body_index];
                        self.insert(
                            node_index,
                            existing_body_index,
                            existing_body,
                            all_positionables,
                            depth + 1,
                        );
                    }

                    // Now, insert the *new* body into the (now internal) node
                    self.insert(
                        node_index,
                        body_sim_index,
                        positionable_to_insert,
                        all_positionables,
                        depth + 1,
                    );
                }
            }

            NodeData::Unused => {
                godot_error!("Attempted to insert into an unused node slot!");
            }
        }
    }
}

impl<'a, T: Spacial + Massive> Octree<'a, T> {
    /// Computes total mass and center of mass for the subtree rooted at node_index.
    /// This is now called on-demand when needed (e.g., by calculate_force).
    /// Returns (total_mass, center_of_mass).
    fn compute_aggregate_properties(&self, node_index: usize) -> (f32, Vec3A) {
        match self.get_node(node_index) {
            Some(NodeData::Internal(internal_data)) => {
                let mut total_mass = 0.0;
                let mut weighted_pos_sum = Vec3A::ZERO;

                for &child_index_option in &internal_data.children {
                    if let Some(child_index) = child_index_option {
                        let (child_mass, child_com) =
                            self.compute_aggregate_properties(child_index.into());
                        total_mass += child_mass;
                        weighted_pos_sum += child_com * child_mass;
                    }
                }

                let center_of_mass = if total_mass > 1e-9 {
                    weighted_pos_sum / total_mass
                } else {
                    internal_data.bounds.center // Default if massless
                };
                (total_mass, center_of_mass)
            }
            Some(NodeData::Leaf(leaf_data)) => {
                let mut total_mass = 0.0;
                let mut weighted_pos_sum = Vec3A::ZERO;

                for &item_index in &leaf_data.positionable_indices {
                    let item = &self.external_data[item_index];
                    let mass = item.get_mass();
                    total_mass += mass;
                    weighted_pos_sum += item.get_pos() * mass;
                }

                let center_of_mass = if total_mass > 1e-9 {
                    weighted_pos_sum / total_mass
                } else {
                    leaf_data.bounds.center // Default if massless
                };
                (total_mass, center_of_mass)
            }
            _ => (0.0, Vec3A::ZERO), // Unused or invalid index
        }
    }

    /// Calculates the gravitational force exerted on a target body using Barnes-Hut.
    pub fn calculate_force(&self, target_item_index: usize, theta: f32, grav_const: f32) -> Vec3A {
        if self.root_index == usize::MAX || self.external_data.is_empty() {
            return Vec3A::ZERO;
        }

        let target_item = &self.external_data[target_item_index];
        let target_pos = target_item.get_pos();
        let target_mass = target_item.get_mass();
        let mut total_force = Vec3A::ZERO;
        let theta_sq = theta * theta;

        let mut stack: Vec<usize> = Vec::with_capacity(64);
        stack.push(self.root_index);

        while let Some(node_index) = stack.pop() {
            match self.get_node(node_index) {
                Some(NodeData::Internal(internal_data)) => {
                    // Compute aggregate properties *on demand* for this internal node
                    let (node_total_mass, node_center_of_mass) =
                        self.compute_aggregate_properties(node_index);

                    if node_total_mass < 1e-9 {
                        continue;
                    } // Skip massless nodes

                    let diff = node_center_of_mass - target_pos;
                    let dist_sq = diff.length_squared();
                    let s_sq = (internal_data.bounds.half_width * 2.0).powi(2);

                    if s_sq < theta_sq * dist_sq || dist_sq < SOFTENING_SQUARED {
                        // Treat as point mass
                        let dist_sq_soft = dist_sq + SOFTENING_SQUARED;
                        let inv_dist = dist_sq_soft.sqrt().recip();
                        let force_dir = diff * inv_dist;
                        let force_magnitude =
                            (grav_const * target_mass * node_total_mass) * inv_dist * inv_dist;
                        total_force += force_dir * force_magnitude;
                    } else {
                        // Recurse
                        for &child_index_option in &internal_data.children {
                            if let Some(child_index) = child_index_option {
                                stack.push(child_index.into());
                            }
                        }
                    }
                }
                Some(NodeData::Leaf(leaf_data)) => {
                    // Direct calculation for leaf items
                    for &item_index in &leaf_data.positionable_indices {
                        if item_index == target_item_index {
                            continue;
                        }

                        let other_item = &self.external_data[item_index];
                        let diff = other_item.get_pos() - target_pos;
                        let dist_sq = diff.length_squared();
                        let dist_sq_soft = dist_sq + SOFTENING_SQUARED;

                        if dist_sq_soft > 1e-9 {
                            let inv_dist = dist_sq_soft.sqrt().recip();
                            let force_dir = diff * inv_dist;
                            let force_magnitude =
                                (grav_const * target_mass * other_item.get_mass())
                                    * inv_dist
                                    * inv_dist;
                            total_force += force_dir * force_magnitude;
                        }
                    }
                }
                _ => {} // Ignore Unused or invalid index
            }
        }
        total_force
    }
}

#[cfg(test)]
mod tests {
    use godot::obj::InstanceId;

    use crate::physics::gravity::controller::SimulatedBody;

    use super::*;

    fn create_bench_bodies(n: u32) -> Vec<SimulatedBody> {
        let mut bodies = Vec::with_capacity(n as usize);

        for i in 1..=n {
            // Position bodies in a spherical pattern
            let phi = (i as f32) * 0.1;
            let theta = (i as f32) * 0.2;
            let radius = 1000.0 + (i as f32) * 10.0;

            bodies.push(SimulatedBody {
                body_instance_id: InstanceId::from_i64(i.into()),
                mass: 1000.0 + (i as f32),
                pos: Vec3A::new(
                    radius * phi.sin() * theta.cos(),
                    radius * phi.sin() * theta.sin(),
                    radius * phi.cos(),
                ),
                vel: Vec3A::ZERO,
            });
        }

        bodies
    }

    #[test]
    fn build() {
        let sim_bodies = create_bench_bodies(100000);
        let mut octree = Octree::new(&sim_bodies);
        octree.build();

        dbg!(octree);
    }
}
