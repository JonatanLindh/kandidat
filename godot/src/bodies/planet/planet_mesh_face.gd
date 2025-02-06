@tool
extends MeshInstance3D
class_name PlanetMeshFace

@export var normal: Vector3
@export var material: Material

func regenerate_mesh(planet_data: PlanetData):
	var resolution := planet_data.resolution
	
	var st = SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	st.set_color(Color.WHITE)
	
	var axisA := Vector3(normal.y, normal.z, normal.x)
	var axisB := normal.cross(axisA)
	var i = 0
	for y in resolution:
		for x in resolution:
			var percent := Vector2(x, y) / (resolution - 1)
			var point_on_unit_cube := normal + (percent.x - 0.5) * 2 * axisA + (percent.y - 0.5) * 2 * axisB
			var point_on_unit_sphere = point_on_unit_cube.normalized()
			var point_on_planet := planet_data.point_on_planet(point_on_unit_sphere)
			st.add_vertex(point_on_planet)
			
			var l = point_on_planet.length()
			planet_data.min_height = min(planet_data.min_height, l)
			planet_data.max_height = max(planet_data.max_height, l)
			
			if x != resolution - 1 and y != resolution - 1:
				st.add_index(i)
				st.add_index(i + resolution)
				st.add_index(i + resolution + 1)
				
				st.add_index(i)
				st.add_index(i + resolution + 1)
				st.add_index(i + 1)
			i += 1
	
	st.generate_normals()
	#st.set_material(material)
	var commited_mesh := st.commit()
	call_deferred("_update_mesh", commited_mesh, planet_data)

func _update_mesh(commited_mesh: Mesh, planet_data: PlanetData):
	self.mesh = commited_mesh
	
	material_override.set_shader_parameter("min_height", planet_data.min_height)
	material_override.set_shader_parameter("max_height", planet_data.max_height)
	material_override.set_shader_parameter("height_color", planet_data.planet_color)
