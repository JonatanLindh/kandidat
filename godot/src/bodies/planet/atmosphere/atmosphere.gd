extends MeshInstance3D

@export var radius: float:
	set(new_radius):
		radius = new_radius
		
@export var sun_direction: Vector3:
	set(new_dir):
		sun_direction = new_dir
		_update_runtime_shader_params()

# CHEATING SINCE WE DON'T HAVE MIE SCATTERING
const EARTH_LIKE := Vector3(700, 530, 440)
const PURPLE_ISH := Vector3(540, 700, 380)
const MARS_LIKE_ORANGE := Vector3(620, 800, 1000)
const GREEN := Vector3(1000, 530, 800)

var wave_lenghts_array := [EARTH_LIKE, PURPLE_ISH, MARS_LIKE_ORANGE, GREEN]
	
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Duplicate shader to allow uniuqe parameters
	if mesh.material:
		mesh.material = mesh.material.duplicate()
	
	_update_runtime_shader_params()
	var random_wave_length = wave_lenghts_array.pick_random()
	mesh.material.set_shader_parameter("wavelengths", random_wave_length)
	mesh.material.set_shader_parameter("planet_radius", radius)
	mesh.size = Vector3(radius*6,radius*6,radius*6)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	_update_runtime_shader_params()

func _atmosphere_changed():
	_update_runtime_shader_params()

func _update_runtime_shader_params():
	if mesh and mesh.material:
		mesh.material.set_shader_parameter("sun_direction", sun_direction.normalized())
		mesh.material.set_shader_parameter("planet_position", position)
		mesh.material.set_shader_parameter("planet_radius", radius)
		
