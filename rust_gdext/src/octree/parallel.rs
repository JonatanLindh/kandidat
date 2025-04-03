use super::{BoundingBox, MAX_BODIES_PER_LEAF, visualize::VisualizeOctree};
use crate::octree::GravityData;
use itertools::Itertools;
use rayon::prelude::*;
use std::array;

#[derive(Debug)]
pub struct Node {
    /// The bounding box of the node.
    ///
    /// This is a 3D box defined by its center and half-width.
    /// The half-width is the maximum distance from the center to any corner of the box.
    pub bounds: BoundingBox,

    /// The number of total children in the subtree rooted at this node.
    pub n_subtree_nodes: usize,

    /// The data associated with this node.
    ///
    /// This can be either internal or leaf data.
    pub data: GravityData,
}

// The Octree structure holding the final arena
#[derive(Debug, Default)]
pub struct ParallelOctree {
    pub nodes: Vec<Node>, // Final arena
    pub bounds: BoundingBox,
}

impl VisualizeOctree for ParallelOctree {
    fn get_bounds_and_depths(&mut self) -> Vec<(BoundingBox, u32)> {
        let root_half_width = self.bounds.half_width;
        self.nodes
            .iter()
            .sorted_by(|a, b| {
                a.bounds
                    .half_width
                    .partial_cmp(&b.bounds.half_width)
                    .unwrap()
            })
            .map(|node| {
                // Calculate the depth based on the half-width of the bounding box
                // Depth zero is the root node and the depth increases as the half-width decreases.
                let depth = (root_half_width / node.bounds.half_width).log(2.0).round() as u32;
                (node.bounds, depth)
            })
            .collect()
    }
}

impl ParallelOctree {
    pub const ROOT_IDX: usize = 0;
    pub const HALF_WIDTH_MERGE_THRESHOLD: f32 = 1e-2;

    const PARALLEL_PARTITION_MIN: usize = 100_000;
    const PARALLEL_SUBTREE_BUILD_MIN: usize = 200;

    pub fn construct_branch(bounds: &BoundingBox, bodies: Vec<GravityData>) -> (Node, Vec<Node>) {
        debug_assert!(!bodies.is_empty(), "construct_branch called with no bodies");
        let n_bodies = bodies.len();

        // 1. Partition Bodies
        let octants = if n_bodies > Self::PARALLEL_PARTITION_MIN {
            // Use parallel partitioning if the number of bodies is large enough
            Self::partition_bodies_parallel(bounds, bodies)
        } else {
            // Use sequential partitioning for smaller sets
            Self::partition_bodies_sequential(bounds, bodies)
        };

        // 2. Build subtree
        let (data, subtree_arena) = if n_bodies > Self::PARALLEL_SUBTREE_BUILD_MIN {
            // Use parallel subtree building if the number of bodies is large enough
            Self::branch_subtree_parallel(bounds, octants)
        } else {
            // Use sequential subtree building for smaller sets
            Self::branch_subtree_sequential(bounds, octants)
        };

        // 3. Create the branching node
        (
            Node {
                bounds: *bounds,
                n_subtree_nodes: subtree_arena.len(),
                data: GravityData::merge(&data),
            },
            subtree_arena,
        )
    }

    /// Builds the octree in parallel using the hybrid strategy.
    pub fn build(bodies: Vec<GravityData>) -> Self {
        if bodies.is_empty() {
            return ParallelOctree::default();
        }

        // 1. Compute Root Bounds
        let root_bounds = BoundingBox::containing_gravity_data(&bodies);

        // 2. Construct the nodes
        let (root_node, subtree_arena) = Self::construct_branch(&root_bounds, bodies);

        // 3. Create the octree
        ParallelOctree {
            nodes: core::iter::once(root_node).chain(subtree_arena).collect(),
            bounds: root_bounds,
        }
    }

    pub fn partition_bodies_parallel(
        bounds: &BoundingBox,
        bodies: Vec<GravityData>,
    ) -> [Vec<GravityData>; 8] {
        // Use fold + reduce for efficient parallel aggregation.
        // Each thread accumulates into a local `[Vec<GravityData>; 8]`.
        bodies
            .into_par_iter()
            .fold(
                // Initial value factory for each thread's accumulator
                || array::from_fn(|_| Vec::new()), // Create [Vec::new(), Vec::new(), ...]
                // Fold operation: Add index to the correct octant's vec in the thread-local accumulator
                |mut octants, body| {
                    let octant_index = bounds.get_octant_index(body.center_of_mass);
                    octants[octant_index].push(body);
                    octants
                },
            )
            // Reduce operation: Combine thread-local accumulators
            .reduce(
                // Initial value for the final reduction (empty)
                || array::from_fn(|_| Vec::new()),
                // Combine two accumulators
                |mut a, mut b| {
                    for i in 0..8 {
                        a[i].append(&mut b[i]); // Append vectors
                    }
                    a
                },
            )
    }

    pub fn partition_bodies_parallel2(
        bounds: &BoundingBox,
        bodies: Vec<GravityData>,
    ) -> [Vec<GravityData>; 8] {
        let mut octants = array::from_fn(|_| Vec::new());
        bodies
            .into_par_iter()
            .map(|body| {
                let octant_index = bounds.get_octant_index(body.center_of_mass);
                (octant_index, body)
            })
            .collect_vec_list()
            .into_iter()
            .flatten()
            .for_each(|(octant_index, body)| {
                octants[octant_index].push(body);
            });

        octants
    }

    pub fn partition_bodies_parallel3(
        bounds: &BoundingBox,
        bodies: Vec<GravityData>,
    ) -> [Vec<GravityData>; 8] {
        let (((a, b), (c, d)), ((e, f), (g, h))) = bodies
            .into_par_iter()
            .partition_map(|data| bounds.octant_either_partition(data));

        [a, b, c, d, e, f, g, h]
    }

    pub fn partition_bodies_sequential(
        bounds: &BoundingBox,
        bodies: Vec<GravityData>,
    ) -> [Vec<GravityData>; 8] {
        // Sequential partitioning
        let mut octants = array::from_fn(|_| Vec::new());

        for body in bodies {
            let octant_index = bounds.get_octant_index(body.center_of_mass);
            octants[octant_index].push(body);
        }

        octants
    }

    pub fn branch_subtree_parallel(
        bounds: &BoundingBox,
        octants: [Vec<GravityData>; 8],
    ) -> (Vec<GravityData>, Vec<Node>) {
        let (data, subtree_arena) = octants
            .into_par_iter()
            .enumerate()
            .filter(|(_, octant)| !octant.is_empty()) // Filter out empty octants
            .map(|(octant_index, octant_members)| {
                let octant_bounds = bounds.get_octant_bounds(octant_index);

                // 3a.
                // If there is only one body in this octant, we can create a leaf node.
                if octant_members.len() == 1 {
                    let data = octant_members.into_iter().exactly_one().unwrap();
                    return (
                        Node {
                            bounds: octant_bounds,
                            n_subtree_nodes: 0,
                            data,
                        },
                        None,
                    );
                }

                // 3b.
                // If the octant is small enough, we can create a leaf node.
                if octant_bounds.half_width <= Self::HALF_WIDTH_MERGE_THRESHOLD {
                    let aggregate_data = GravityData::merge(&octant_members);
                    return (
                        Node {
                            bounds: octant_bounds,
                            n_subtree_nodes: 0,
                            data: aggregate_data,
                        },
                        None,
                    );
                }

                // 3c.
                // If the number of bodies exceeds the max allowed, we need to create a branch node.
                let (node, children_arena) = Self::construct_branch(&octant_bounds, octant_members);

                (node, Some(children_arena))
            })
            .fold(
                || (Vec::new(), Vec::new()),
                |(mut data_acc, mut arena_acc), (node, children_arena)| {
                    data_acc.push(node.data.clone());
                    arena_acc.push(node);

                    if let Some(arena) = children_arena {
                        arena_acc.extend(arena);
                    }

                    (data_acc, arena_acc)
                },
            )
            .reduce(
                || (Vec::new(), Vec::new()),
                |(mut acc_data, mut acc_arena), (mut data, mut arena)| {
                    acc_arena.append(&mut arena);
                    acc_data.append(&mut data);

                    (acc_data, acc_arena)
                },
            );
        (data, subtree_arena)
    }

    pub fn branch_subtree_sequential(
        bounds: &BoundingBox,
        octants: [Vec<GravityData>; 8],
    ) -> (Vec<GravityData>, Vec<Node>) {
        let (data, subtree_arena) = octants
            .into_iter()
            .enumerate()
            .filter(|(_, octant)| !octant.is_empty()) // Filter out empty octants
            .map(|(octant_index, octant_members)| {
                let octant_bounds = bounds.get_octant_bounds(octant_index);

                // 3a.
                // If there is only one body in this octant, we can create a leaf node.
                if octant_members.len() == 1 {
                    let data = octant_members.into_iter().exactly_one().unwrap();
                    return (
                        Node {
                            bounds: octant_bounds,
                            n_subtree_nodes: 0,
                            data,
                        },
                        None,
                    );
                }

                // 3b.
                // If the number of bodies in this octant is less than or equal to the max allowed
                // and the octant is small enough, we can create a leaf node.
                if octant_members.len() <= MAX_BODIES_PER_LEAF
                    && octant_bounds.half_width <= Self::HALF_WIDTH_MERGE_THRESHOLD
                {
                    let aggregate_data = GravityData::merge(&octant_members);
                    return (
                        Node {
                            bounds: octant_bounds,
                            n_subtree_nodes: 0,
                            data: aggregate_data,
                        },
                        None,
                    );
                }

                // 3c.
                // If the number of bodies exceeds the max allowed, we need to create a branch node.

                let (node, children_arena) = Self::construct_branch(&octant_bounds, octant_members);

                (node, Some(children_arena))
            })
            .fold(
                (Vec::new(), Vec::new()),
                |(mut data_acc, mut arena_acc), (node, children_arena)| {
                    data_acc.push(node.data.clone());
                    arena_acc.push(node);

                    if let Some(arena) = children_arena {
                        arena_acc.extend(arena);
                    }

                    (data_acc, arena_acc)
                },
            );

        (data, subtree_arena)
    }
}

#[test]
fn feature() {
    let bodies = (1..10)
        .map(|_| GravityData {
            mass: rand::random_range(1.0..100.0),
            center_of_mass: glam::Vec3A::new(
                rand::random_range(-100.0..100.0),
                rand::random_range(-100.0..100.0),
                rand::random_range(-100.0..100.0),
            ),
        })
        .collect::<Vec<_>>();

    dbg!(&bodies);

    let octree = ParallelOctree::build(bodies);
    dbg!(
        octree
            .nodes
            .iter()
            .filter(|body| body.n_subtree_nodes == 0)
            .map(|b| b.data.clone())
            .collect_vec()
    );
}
