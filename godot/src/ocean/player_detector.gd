@tool
extends Node3D
class_name player_detector

@onready var _camera: Camera3D;
@onready var _underwater_filter: underwater_filter;
@onready var _ocean;

@export var useUnderwaterFilter: bool = false;

var _ocean_radius: int;
var _ocean_center: Vector3;

var _is_underwater: bool;
var _enter: bool;
var _exit: bool;

var _last_position: Vector3;

func _ready() -> void:
	if(useUnderwaterFilter and not Engine.is_editor_hint()):
		_camera = get_tree().current_scene.find_child("Camera3D", true);
		_underwater_filter = get_tree().current_scene.find_child("UnderwaterFilter", true);
		_ocean = get_parent();
		_ocean_radius = _ocean.radius
		_ocean_center = Vector3.ZERO;
		_last_position = (_camera.global_position);

func get_ocean_center() -> Vector3:
	return _ocean.transform.origin + Vector3(_ocean_radius, _ocean_radius, _ocean_radius);
	
func _inside_ocean(pos: Vector3) -> bool:
	#print(str("camera:	 ", pos, "	 ocean_center:	 "), to_global(_ocean_center), "	 distance:	", pos.distance_to(to_global(_ocean_center)), "	radius:	", _ocean_radius);
	return pos.distance_to(to_global(_ocean_center)) < _ocean_radius;
	
func _entered_ocean(pos: Vector3) -> bool:
	return !_inside_ocean(_last_position) and _inside_ocean(pos);

func _exited_ocean(pos: Vector3) -> bool:
	return _inside_ocean(_last_position) and !_inside_ocean(pos);
	
func _toggle_underwater_filter(toggle: bool):
	_underwater_filter.toggle_underwater_filter(toggle);
	
func _enter_water():
	_is_underwater = true;
	_toggle_underwater_filter(true)
	_ocean.mesh.flip_faces = true;

func _exit_water():
	_is_underwater = false;
	_toggle_underwater_filter(false);
	_ocean.mesh.flip_faces = false;

func _process(delta: float) -> void:
	if(_camera != null and _ocean != null and useUnderwaterFilter):
		var current_position: Vector3 = _camera.global_position;
		if(_entered_ocean(current_position)):
			_enter_water();
			print("inside");
		elif(_exited_ocean(current_position)):
			_exit_water();
			print("outside");
		_last_position = current_position;
	
