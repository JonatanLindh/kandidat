extends Node3D

var currentSeed:int = 0;
@onready var system = $System;

func _ready() -> void:
	var ss = get_node("GalaxyMap/UiSelectableStar")
	ss.connect("ExploreStar",_on_star_clicked)
	
	
func _on_star_clicked(newSeed):
	currentSeed = newSeed
	system.generateSystemFromSeed(currentSeed)
	# Handle the seed as needed
