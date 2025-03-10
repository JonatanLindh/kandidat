extends Node3D

@onready var player = $Player;
@onready var systemManager = $SystemManager;


func _process(delta: float):
	var distance_from_origin = player.global_position.distance_to(Vector3.ZERO)
	const distanceToOuterSpace = 200;
	if distance_from_origin > systemManager.getSystemRadius() + distanceToOuterSpace:
		systemManager.goToGalaxyMap();
		player.position = Vector3.ZERO;
