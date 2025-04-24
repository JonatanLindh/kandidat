use crate::to_glam_vec3;

use super::BoundingBox;
use glam::Vec3A;
use godot::{
    classes::{Mesh, MeshInstance3D},
    prelude::*,
};

#[derive(GodotClass)]
#[class(tool, init, base = Node)]
struct MeshKeeper {
    // octree: LodOctree,
    #[init(node = "../OctreePlanetSpawner")]
    spawner: OnReady<Gd<Node>>,

    octree: LodOctree,

    base: Base<Node>,
}

#[godot_api]
impl INode for MeshKeeper {
    fn ready(&mut self) {
        let root_center = Vector3::ZERO;
        let root_half_width = 32.0;
        let root_bounds = BoundingBox {
            center: to_glam_vec3(root_center),
            half_width: root_half_width,
        };

        let root_mesh_instance = self
            .spawner
            .call(
                "SpawnChunk",
                &[
                    root_center.to_variant(),
                    root_half_width.to_variant(),
                    0.0.to_variant(),
                ],
            )
            .try_to::<Gd<MeshInstance3D>>();

        if root_mesh_instance.is_err() {
            godot_error!("Failed to generate root mesh instance");
            return;
        };

        self.octree = LodOctree::new(
            root_bounds,
            8,
            NodeData::Leaf {
                mesh_instance: root_mesh_instance.unwrap(),
            },
        );
    }
}

#[derive(Debug, Default)]
pub struct LodOctree {
    pub max_depth: u32,
    pub root_index: Option<usize>,
    pub nodes: Vec<LodNode>,
}

#[derive(Debug)]
pub struct LodNode {
    pub bounds: BoundingBox,
    pub depth: u32,
    pub data: NodeData,
}

#[derive(Debug)]
pub enum NodeData {
    Leaf {
        mesh_instance: Gd<MeshInstance3D>,
    },

    Internal {
        cached_mesh: Gd<Mesh>,
        children: [Option<usize>; 8],
    },
}

#[derive(GodotClass)]
#[class(tool, init)]
struct ChunkInfo {
    #[export]
    pub node_index: u32,

    #[export]
    pub center: Vector3,

    #[export]
    pub half_width: f32,

    #[export]
    pub depth: u32,
}

impl LodOctree {
    pub fn new(root_bounds: BoundingBox, max_depth: u32, data: NodeData) -> Self {
        let root_node = LodNode {
            bounds: root_bounds,
            depth: 0,
            data,
        };

        let nodes = vec![root_node];

        LodOctree {
            max_depth,
            root_index: Some(0),
            nodes,
        }
    }
}
