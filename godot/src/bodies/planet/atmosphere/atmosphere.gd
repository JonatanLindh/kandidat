extends MeshInstance3D
@onready var planet: GravityBody = $".."

var radius: float:
	set(new_radius):
		radius = new_radius
		scale = Vector3(radius*3,radius*3,radius*3)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	mesh.material.set_shader_parameter("planet_position", position)
	mesh.material.set_shader_parameter("planet_radius", radius)
	scale = Vector3(radius*3,radius*3,radius*3)
	


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	mesh.material.set_shader_parameter("planet_position", position)
	mesh.material.set_shader_parameter("planet_radius", radius)
