@tool
extends MeshInstance3D
class_name ocean

@export_group("Size parameters")
@export var radius: int = 32:
	set(new_radius):
		radius = new_radius;
		_update_size();

@export_group("Appearance parameters")
@export_color_no_alpha var deep_color: Color = Color.hex(0x005b82):
	set(new_color):
		deep_color = new_color;
		_update_deep_color();
@export_color_no_alpha var shallow_color: Color = Color.hex(0x008ec8):
	set(new_color):
		shallow_color = new_color;
		_update_shallow_color();
@export_color_no_alpha var foam_color: Color = Color.hex(0xffffff):
	set(new_color):
		foam_color = new_color;
		_update_foam_color();

var sphere: SphereMesh = mesh as SphereMesh;
var _time: float = 0.0;
var ocean_material: Material = get_surface_override_material(0);

func _ready() -> void:
	set_radius(radius);

func _process(delta: float) -> void:
	ocean_material.set_shader_parameter("delta_time", delta);
	ocean_material.set_shader_parameter("time", _time);
	_time += delta;

func _update_colors():
	_update_deep_color();
	_update_shallow_color();
	_update_foam_color();
	
func _update_deep_color():
	ocean_material.set_shader_parameter("deep_color", deep_color);
	
func _update_shallow_color():
	ocean_material.set_shader_parameter("shallow_color", shallow_color);
	
func _update_foam_color():
	ocean_material.set_shader_parameter("foam_color", foam_color);
	
func _update_size():
	sphere.radius = radius;
	sphere.height = 2 * radius;

func set_colors(deep: Color, shallow: Color, foam: Color):
	deep_color = deep;
	shallow_color = shallow;
	foam_color = foam;

func set_radius(new_radius: int):
	radius = new_radius;

## Returns an array of the water-colors on the form:
## [deep_color, shallow_color, foam_color]
func get_colors() -> PackedColorArray:
	var colors: PackedColorArray = [deep_color, shallow_color, foam_color];
	return colors;

func get_radius() -> int:
	return radius;
