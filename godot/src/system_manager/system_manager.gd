extends Node3D

var currentSeed:int = 0;
@onready var system = $System;
@onready var galaxyMap = $GalaxyMap;

func _ready() -> void:
	var ss = get_node("GalaxyMap/UiSelectableStar")
	ss.connect("ExploreStar",_on_star_clicked)
	
	
func _on_star_clicked(newSeed):
	currentSeed = newSeed
	system.generateSystemFromSeed(currentSeed)
	goToSystem()
	# Handle the seed as needed

func getSystemRadius():
	return system.getSystemRadius();

func goToGalaxyMap():
	system.hide();
	galaxyMap.show();
	return;
	
func goToSystem():
	galaxyMap.hide();
	system.show();
	return;
	
