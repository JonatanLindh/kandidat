@tool
extends MeshInstance3D
class_name underwater_filter

@onready var _camera: Camera3D;
var _filter_quad: QuadMesh = mesh as QuadMesh;

func _ready() -> void:
	_setup_filter();

func _setup_filter() -> void:
	_camera = get_parent();
	var screen_size: Vector2 = get_viewport().get_visible_rect().size;
	_filter_quad.size = screen_size;

func toggle_underwater_filter(toggle: bool):
	visible = toggle;
