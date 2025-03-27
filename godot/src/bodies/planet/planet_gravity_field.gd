extends Area3D
class_name PlanetGravityField
## This class is used to generate a gravitational field around planets using Godot's built in physics engine. 

## The planet the gravity field is assigned to
@onready var planet: GravityBody = $".."

## The collision shape
@onready var collision_shape_3d: CollisionShape3D = $CollisionShape3D

@export var radius : float

func _physics_process(delta: float) -> void:
	gravity_point_center = planet.global_position
	# TODO get player position from player variables
	var player_position = PlayerVariables.player_position
	gravity_direction = (player_position - gravity_point_center).normalized()
	
func _ready() -> void:
	#Debug and test
	gravity_space_override = Area3D.SPACE_OVERRIDE_REPLACE
	gravity_point = true
	gravity = 9.82
	
## Factory Method for creating new PlanetGravityFields 
func create_new_gravity_field(radius : float , gravity_strength : float ) -> PlanetGravityField:
	var gravity_field := PlanetGravityField.new()
	var gravity_field_shape := CollisionShape3D.new()
	gravity_field_shape.shape = SphereShape3D.new()
	gravity_field_shape.shape.radius = radius
	gravity_field.add_child(gravity_field_shape)
	return gravity_field
	
