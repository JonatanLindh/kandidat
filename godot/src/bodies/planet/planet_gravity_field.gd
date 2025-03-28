extends Area3D
class_name PlanetGravityField
## This class is used to generate a gravitational field around planets using Godot's built in physics engine. 

## The planet the gravity field is assigned to
@onready var planet: GravityBody = $".."

## The collision shape
@onready var collision_shape_3d: CollisionShape3D = $CollisionShape3D

@export var radius : float
@export var base_gravity: float = 9.82

var _player_inside_field := false

func _physics_process(delta: float) -> void:
	if not planet:
		return

	gravity_point_center = planet.global_position
	var player_position = PlayerVariables.player_position
	gravity_direction = (gravity_point_center - player_position).normalized()
	
	# 🛰️ Adjust for planet's velocity
	var relative_velocity = planet.velocity - get_player_velocity()
	if _player_inside_field:
		PlayerVariables.planet_velocity = planet.velocity
		PlayerVariables.planet_position = planet.global_transform.origin
	update_gravity(relative_velocity)

	
func _ready() -> void:
	#Debug and test
	gravity_space_override = Area3D.SPACE_OVERRIDE_REPLACE
	gravity_point = true
	gravity = 9.82
	connect("body_entered", _on_body_entered)
	connect("body_exited", _on_body_exited)

func update_gravity(relative_velocity: Vector3) -> void:
		if planet:
			var velocity_magnitude = planet.velocity.length()
			var relative_velocity_magnitude = relative_velocity.length()
			gravity = base_gravity + velocity_magnitude * 0.1 + relative_velocity_magnitude * 0.05

func get_player_velocity() -> Vector3:
		if Engine.has_singleton("PlayerVariables") and PlayerVariables.has("player_velocity"):
			return PlayerVariables.velocity
		return Vector3.ZERO
		
func _on_body_entered(body):
	if body is Player:
		body.on_gravity_field_entered(gravity, gravity_direction, planet.velocity)
		_player_inside_field = true

func _on_body_exited(body):
	if body is Player:
		body.on_gravity_field_exited()
		_player_inside_field = false

## Factory Method for creating new PlanetGravityFields 
func create_new_gravity_field(radius : float , gravity_strength : float ) -> PlanetGravityField:
	var gravity_field := PlanetGravityField.new()
	var gravity_field_shape := CollisionShape3D.new()
	gravity_field_shape.shape = SphereShape3D.new()
	gravity_field_shape.shape.radius = radius
	gravity_field.add_child(gravity_field_shape)
	return gravity_field
