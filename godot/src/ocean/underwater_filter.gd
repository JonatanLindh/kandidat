@tool
extends MeshInstance3D
class_name underwater_filter

@onready var _camera: Camera3D = $Head/Camera3D
var _filter_quad: QuadMesh = mesh as QuadMesh;

func _ready() -> void:
	#_camera = get_node("root/Main/Player/Head/Camera3D");
	setup_filter();

func setup_filter() -> void:
	#var camera_fov: float = _camera.fov;
	#var near_plane_distance: float = _camera.near;
	#var far_plane_distance: float = _camera.far;
	#var aspect_ratio: float = _camera.get_viewport().Size.x 
	var screen_size: Vector2 = get_viewport().get_visible_rect().size;
	_filter_quad.size = screen_size;

func _process(delta: float) -> void:
	if(_inside_ocean()):
		1+1
		
func _inside_ocean():
	1+1
