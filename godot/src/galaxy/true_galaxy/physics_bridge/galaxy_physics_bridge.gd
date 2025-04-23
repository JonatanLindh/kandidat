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
##     {"position", PackedVector3Array},
##     {"velocity", PackedVector3Array},
##     {"mass", PackedFloat32Array}
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

## Updates star velocities and applies them to the transforms
func apply_velocities(new_velocites : PackedVector3Array):
	if(!init):
		printerr("Physics bridge: Stars not initialized yet.")
		return
	true_galaxy.ApplyVelocities(new_velocites)
