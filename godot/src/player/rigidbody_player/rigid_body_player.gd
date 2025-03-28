extends RigidBody3D
class_name RigidBodyPlayer

signal updated_status(position, speed)

const JUMP_VELOCITY = 4.5
const BASE_SPEED = 5.0
var current_speed = 5.0
var mouse_sensitivity = 0.1
var sprint_factor = 2
var float_speed = 5
var base_fov = 75
var flying := true
var last_position := Vector3.ZERO
var last_speed := 0.0
var floating_flag := false
var vertical_multiplier = 0.1
var gravity_vector := Vector3.ZERO
var in_gravity_field = false
var planet_velocity: Vector3 = Vector3.ZERO
var gravity_strength := 0


func _process(delta):
	_move()
	
func _integrate_forces(state):
	_walk_around_planet(state)
	
func _walk_around_planet(state):
	# allign the players y-axis (up and down) with the planet's gravity direciton:
	state.transform.basis.y = -get_gravity().normalized()
	
func _move():
	#handles all input and logic related to character movement
	#move
	if Input.is_action_pressed("forward"):
		apply_central_force(BASE_SPEED* global_transform.basis.z)
		
	if Input.is_action_pressed("backward"):
		apply_central_force(BASE_SPEED* -global_transform.basis.z)

	if Input.is_action_pressed("left"):
		apply_central_force(BASE_SPEED* global_transform.basis.x)

	if Input.is_action_pressed("right"):
		apply_central_force(BASE_SPEED* -global_transform.basis.x)
	
	#jump:
	if Input.is_action_just_pressed("ui_accept"):
		apply_impulse(Vector3.UP, JUMP_VELOCITY* global_transform.basis.y)
	
func on_gravity_field_entered(gravity : float, gravity_direction : Vector3, planet_velocity : Vector3):
	in_gravity_field = true
	gravity_strength = gravity
	gravity_vector = gravity_direction * gravity_strength
	self.planet_velocity = planet_velocity
	print("Im enter")

func on_gravity_field_exited():
	in_gravity_field = false
	gravity_vector = Vector3.ZERO
	planet_velocity = Vector3.ZERO
	gravity_strength = 0
	flying = true
	print("Im exit")
