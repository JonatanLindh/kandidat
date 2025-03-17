extends MeshInstance3D

@export var radius: float:
	set(new_radius):
		radius = new_radius
		_update_shader_params()
		
@export var sun_direction: Vector3:
	set(new_dir):
		sun_direction = new_dir
		_update_shader_params()
		
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
	scale = Vector3(radius * 4, radius * 4, radius * 4)

func _update_shader_params():
	var cam_pos = PlayerVariables.player_position
	var cam_dir = PlayerVariables.camera_direction

	if mesh and mesh.material:
		mesh.material.set_shader_parameter("sun_direction", sun_direction)
		mesh.material.set_shader_parameter("planet_position", position)
		mesh.material.set_shader_parameter("planet_radius", radius)
		mesh.material.set_shader_parameter("camera_position", cam_pos)
		mesh.material.set_shader_parameter("camera_direction", cam_dir)
