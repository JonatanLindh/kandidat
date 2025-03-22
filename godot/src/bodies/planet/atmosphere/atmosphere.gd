extends MeshInstance3D

@export var radius: float:
	set(new_radius):
		radius = new_radius
		_update_shader_params()
		
@export var sun_direction: Vector3:
	set(new_dir):
		sun_direction = new_dir
		_update_shader_params()

@export var od_tex_filename : String:
	set(value):
		od_tex_filename = value
		var od_tex := _get_od_tex()
		if mesh and mesh.material: mesh.material.set_shader_parameter("optical_depth_texture", od_tex)
		
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
	
	# Really slow OD baking
	#var od_generator = OdGenerator.new()
	#od_generator.atmosphere_radius = radius * 1.5
	#od_generator.planet_radius = radius
	#od_generator.filename = str(get_instance_id())
	#od_generator.generate_od_img()
	#od_tex_filename = od_generator.filename
	
	mesh.size = Vector3(radius*3,radius*3,radius*3)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	_update_shader_params()

func _atmosphere_changed():
	_update_shader_params()
	scale = Vector3(radius, radius, radius)

func _update_shader_params():
	var cam_pos = PlayerVariables.player_position
	var cam_dir = PlayerVariables.camera_direction

	if mesh and mesh.material:
		mesh.material.set_shader_parameter("sun_direction", sun_direction.normalized())
		mesh.material.set_shader_parameter("planet_position", position)
		mesh.material.set_shader_parameter("planet_radius", radius)
		mesh.material.set_shader_parameter("camera_position", cam_pos)
		mesh.material.set_shader_parameter("camera_direction", cam_dir)

func _get_od_tex() -> ImageTexture:
	if od_tex_filename.is_empty(): return

	var img_in = FileAccess.open("res://src/bodies/planet/atmosphere/textures/%s.i" % od_tex_filename, FileAccess.READ)
	var dat_in = FileAccess.open("res://src/bodies/planet/atmosphere/textures/%s.idat" % od_tex_filename, FileAccess.READ)

	var dat = JSON.parse_string(dat_in.get_line())
	var inbytes = img_in.get_buffer(dat.blen)

	var od_img := Image.create_from_data(dat.width, dat.height, false, dat.format, inbytes)
	var od_tex := ImageTexture.create_from_image(od_img)

	return od_tex

func _update_shader_od():
	var od_tex := _get_od_tex() 
	mesh.material.set_shader_parameter("optical_depth_texture", od_tex)
