use glam::Vec3A;
use godot::{
    classes::{
        ImmediateMesh, MeshInstance3D, StandardMaterial3D,
        base_material_3d::{Flags, ShadingMode},
        mesh::PrimitiveType,
    },
    prelude::*,
};

use crate::{
    octree::{BoundingBox, NodeData, Octree},
    physics::gravity::controller::SimulatedBody,
}; // Import necessary items from your octree.rs

#[derive(GodotClass)]
#[class(tool, base = Node3D)]
pub struct OctreeVisualizer {
    mesh_instance: Option<Gd<MeshInstance3D>>,
    material: Option<Gd<StandardMaterial3D>>,
    base: Base<Node3D>,
}

#[godot_api]
impl INode3D for OctreeVisualizer {
    fn init(base: Base<Node3D>) -> Self {
        godot_print!("init");
        Self {
            base,
            mesh_instance: None,
            material: None,
        }
    }

    fn ready(&mut self) {
        // Create a mesh instance to hold the immediate mesh
        let mut mesh_inst = MeshInstance3D::new_alloc();

        // Create a simple material for the lines
        let mut mat = StandardMaterial3D::new_gd();
        mat.set_shading_mode(ShadingMode::UNSHADED);
        // Enable vertex colors instead of using a single albedo color
        mat.set_flag(Flags::ALBEDO_FROM_VERTEX_COLOR, true);
        mat.set_flag(Flags::DISABLE_FOG, true); // Keep lines visible

        // Assign material to mesh instance
        mesh_inst.set_material_override(&mat);

        self.mesh_instance = Some(mesh_inst);
        self.material = Some(mat);
    }
}

#[godot_api]
impl OctreeVisualizer {
    /// Call this method to update the visualization based on the current octree state.
    /// It rebuilds the ImmediateMesh with the bounds of all nodes.
    pub fn update_visualization(&mut self, sim_bodies: &[SimulatedBody]) {
        let mut octree = Octree::new(sim_bodies);
        octree.build();

        let nodes = octree.nodes;

        let Some(mesh_inst) = self.mesh_instance.as_mut() else {
            godot_error!("MeshInstance not ready.");
            return;
        };
        let Some(material) = self.material.as_ref() else {
            godot_error!("Material not ready.");
            return;
        };

        // Create or clear the ImmediateMesh
        let mut imm_mesh = ImmediateMesh::new_gd();
        imm_mesh.surface_begin(PrimitiveType::LINES);

        // Track the maximum depth to create a color gradient
        let mut level = 0;

        let mut next_level = vec![nodes[0].clone()];
        let mut nodes0 = Vec::new();

        while !next_level.is_empty() {
            let current_level = std::mem::take(&mut next_level);
            for node in current_level.iter() {
                match node {
                    NodeData::Internal(d) => {
                        d.children.iter().flatten().for_each(|child| {
                            next_level.push(nodes[child.get()].clone());
                        });
                    }
                    NodeData::Leaf(_) => {}
                    NodeData::Unused => continue,
                };
                nodes0.push((level, node.clone()));
            }
            level += 1;
        }

        // Draw each node with its appropriate color
        for (depth, node_data) in nodes0.iter() {
            let bounds = match node_data {
                NodeData::Internal(d) => &d.bounds,
                NodeData::Leaf(d) => &d.bounds,
                NodeData::Unused => continue, // Skip unused nodes
            };

            // Get the depth of this node (implement this based on your Octree structure)

            // Create a color based on depth
            let color = get_color_for_depth(*depth, level);

            Self::draw_bounding_box(&mut imm_mesh, bounds, color);
        }

        imm_mesh.surface_end();

        // Assign the completed mesh to the instance
        mesh_inst.set_mesh(&imm_mesh);
        let mesh_inst = mesh_inst.clone();
        self.base_mut().add_child(&mesh_inst);
    }
}

// Create a color gradient based on depth
fn get_color_for_depth(depth: u32, max_depth: u32) -> Color {
    if max_depth == 0 {
        return Color::from_rgb(0.0, 1.0, 0.0); // Default green for single level
    }

    // Create a gradient from red (shallow) to blue (deep)
    let t = depth as f32 / max_depth as f32;

    // RGB interpolation for a nice rainbow effect
    let r = 1.0 - t;
    let g = if t < 0.5 { t * 2.0 } else { (1.0 - t) * 2.0 };
    let b = t;

    Color::from_rgb(r, g, b)
}

impl OctreeVisualizer {
    /// Helper function to draw a single bounding box wireframe.
    fn draw_bounding_box(imm_mesh: &mut Gd<ImmediateMesh>, bounds: &BoundingBox, color: Color) {
        let center = bounds.center;
        let half_width = bounds.half_width;

        // Calculate the 8 corners of the cube
        let corners = [
            center + Vec3A::new(-half_width, -half_width, -half_width), // 0: ---
            center + Vec3A::new(half_width, -half_width, -half_width),  // 1: +--
            center + Vec3A::new(half_width, half_width, -half_width),   // 2: ++-
            center + Vec3A::new(-half_width, half_width, -half_width),  // 3: -+-
            center + Vec3A::new(-half_width, -half_width, half_width),  // 4: --+
            center + Vec3A::new(half_width, -half_width, half_width),   // 5: +-+
            center + Vec3A::new(half_width, half_width, half_width),    // 6: +++
            center + Vec3A::new(-half_width, half_width, half_width),   // 7: -++
        ];

        // Convert glam::Vec3A to godot::Vector3 for drawing
        let corners_godot: Vec<Vector3> = corners
            .iter()
            .map(|&v| Vector3::new(v.x, v.y, v.z))
            .collect();

        // Draw the 12 edges
        let edges = [
            (0, 1),
            (1, 2),
            (2, 3),
            (3, 0), // Bottom face
            (4, 5),
            (5, 6),
            (6, 7),
            (7, 4), // Top face
            (0, 4),
            (1, 5),
            (2, 6),
            (3, 7), // Connecting edges
        ];

        for (i, j) in edges {
            imm_mesh.surface_set_color(color);
            imm_mesh.surface_add_vertex(corners_godot[i]);

            imm_mesh.surface_set_color(color);
            imm_mesh.surface_add_vertex(corners_godot[j]);
        }
    }
}
