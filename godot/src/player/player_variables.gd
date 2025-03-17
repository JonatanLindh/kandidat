### SINGLETON CLASS FOR USESFUL PLAYER VARIABLES SUCH AS POSITION, HEAD ROTATION, ETC.
extends Node

@export var player_position: Vector3:
	set(new_pos):
		player_position = new_pos
	get():
		return player_position
		
@export var camera_direction: Vector3:
	set(new_direction):
		camera_direction = new_direction
	get():
		return camera_direction
