extends Node3D

var currentSeed:int = 0;
@onready var system = $System;
@onready var galaxyMap = $GalaxyMap;
@onready var ss = $GalaxyMap/UiSelectableStar;
@onready var skybox = $/root/Main/Skybox;
@onready var player = $/root/Main/Player;

var lastGalaxyMapPosition

func _ready() -> void:
	ss.connect("ExploreStar",_on_star_clicked)
	skybox.environment = load("res://src/galaxy/skybox/_resources/space_no_stars.tres")
	
func _on_star_clicked(newSeed):
	currentSeed = newSeed
	system.generateSystemFromSeed(currentSeed)
	lastGalaxyMapPosition = player.position
	goToSystem()
	# Handle the seed as needed

func getSystemRadius():
	return system.getSystemRadius();

func goToGalaxyMap():
	currentSeed = 0;
	system.clearPlanets();
	system.hide();
	galaxyMap.show();
	ss.show();
	return;
	
func goToSystem():
	galaxyMap.hide();
	system.show();
	ss.hide();
	player.position = Vector3(0,getSystemRadius()/2, 0)
	skybox.environment = load("res://src/galaxy/skybox/_resources/skybox.tres")
	
	return;
	
func getLastGalaxyMapPosition():
	return lastGalaxyMapPosition
