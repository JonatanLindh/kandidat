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

@export var player_velocity: Vector3:
	set(new_v):
		player_velocity = new_v
	get():
		return player_velocity
		
@export var planet_velocity: Vector3:
	set(new_v):
		planet_velocity = new_v
	get():
		return planet_velocity

@export var planet_position: Vector3:
	set(new):
		planet_position = new
	get():
		return planet_position

@export var gravity_field_radius: float:
	set(new):
		gravity_field_radius = new
	get():
		return gravity_field_radius
