@tool
extends MeshInstance3D
@onready var star: GravityBody = $".."

func _ready() -> void:
	var radius = star.radius
	var radius_scaled = radius * 3
	mesh.size = Vector3(radius_scaled, radius_scaled, radius_scaled)
	mesh.material.set_shader_parameter("star_radius", radius)
	mesh.material.set_shader_parameter("color", star.star_color)
