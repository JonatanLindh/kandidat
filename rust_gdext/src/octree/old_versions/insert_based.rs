use crate::octree::{
    BoundingBox, GravityData, MAX_BODIES_PER_LEAF, MIN_HALF_WIDTH, Massive, SOFTENING_SQUARED,
    Spacial, visualize::VisualizeOctree,
};
use glam::Vec3A;
use godot::prelude::{godot_error, godot_warn};
use std::num::NonZeroUsize;

// Maximum recursion depth during insertion
const MAX_INSERT_DEPTH: u32 = 64;

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
pub struct InsertBasedOctree<'a, T: Spacial> {
    nodes: Vec<NodeData>,
    root_index: usize,
    bounds: BoundingBox,
    external_data: &'a [T],
}

impl VisualizeOctree for InsertBasedOctree<'_, GravityData> {
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

impl<'a, T: Spacial> InsertBasedOctree<'a, T> {
    /// Creates a new, empty Octree.
    pub fn new(external_data: &'a [T]) -> Self {
        InsertBasedOctree {
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

impl<'a, T: Spacial + Massive> InsertBasedOctree<'a, T> {
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
