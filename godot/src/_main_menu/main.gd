extends Node3D

@onready var player = $Player;
@onready var systemManager = $SystemManager;
@onready var skybox = $Skybox;

func _process(delta: float):
	if systemManager.currentSeed != 0 and playerIsInOuterSpace():
		systemManager.goToGalaxyMap();
		skybox.environment= load("res://src/galaxy/skybox/_resources/space_no_stars.tres");
		player.position = systemManager.getLastGalaxyMapPosition()
	

func playerIsInOuterSpace():
	var distance_from_origin = player.global_position.distance_to(Vector3.ZERO)
	const distanceToOuterSpace = 200;
	if distance_from_origin > systemManager.getSystemRadius() + distanceToOuterSpace:
		return true;
	
	return false;
