class_name galaxy_physics_bridge extends Node

@onready var true_galaxy : TrueGalaxy = $".."
var stars : Dictionary = {}
var init : bool = false

func _ready():
	call_deferred("init_stars")
	
func init_stars():
	stars = true_galaxy.GetStars()
	init = true;

## Get stars of type Godot Dictionary.[br]
## Should only be necessary to call this once.[br]
##
## [codeblock]
## Godot.Collections.Dictionary dict {
##     {"transform", Godot.Collections.Array<Transform3D>},
##     {"velocity", Godot.Collections.Array<Vector3>},
##     {"mass", Godot.Collections.Array<float>}
## }
## [/codeblock]
##
## Returns an empty dictionary [code] {} [/code] if the stars aren't initalized yet.
##
func get_stars() -> Dictionary:
	if(!init):
		printerr("Physics bridge: Stars not initialized yet.")
		return {}
	return stars

## Get the current frame deltatime
func get_delta() -> float:
	return get_process_delta_time()

## Updates star transforms and velocity
##
## [codeblock]
## Godot.Collections.Dictionary dict {
##     {"transform", Transform3D},
##     {"velocity", Vector3},
##     {"mass", float} # Nothing is done with mass.
## }
## [/codeblock]
##
func update_stars(new_stars : Dictionary):
	if(!init):
		printerr("Physics bridge: Stars not initialized yet.")
		return
	true_galaxy.UpdateStars(new_stars)
