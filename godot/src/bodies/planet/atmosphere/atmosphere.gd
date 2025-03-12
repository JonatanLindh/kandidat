@tool
extends MeshInstance3D

var radius: float:
	set(new_radius):
		radius = new_radius
		_atmosphere_changed()
		
var sun_dir: Vector3:
	set(new_dir):
		sun_dir = new_dir
		_atmosphere_changed()
		
#TODO will be changed to depend on rays
var atmosphere_color := Vector3(
		randf_range(-10, 10), 
		randf_range(-10, 10), 
		randf_range(-10, 10)
	)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Duplicate shader to allow uniuqe parameters
	if mesh.material:
		mesh.material = mesh.material.duplicate()
	_update_shader_params()
	mesh.size = Vector3(radius*3,radius*3,radius*3)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	_update_shader_params()

func _atmosphere_changed():
	_update_shader_params()
	#mesh.size = Vector3(radius*3,radius*3,radius*3)
	scale = Vector3(radius * 4, radius * 4, radius * 4)

func _update_shader_params():
	mesh.material.set_shader_parameter("sun_direction", sun_dir)
	mesh.material.set_shader_parameter("planet_position", position)
	mesh.material.set_shader_parameter("planet_radius", radius)
	#TODO will be changed to depend on rays
