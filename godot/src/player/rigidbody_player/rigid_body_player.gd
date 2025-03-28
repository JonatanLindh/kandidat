extends RigidBody3D
class_name RigidBodyPlayer

var move_force = 30
var jump_force = 20


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
	if Input.is_action_pressed("movement_forward"):
		apply_central_force(move_force* global_transform.basis.z)
		
	if Input.is_action_pressed("movement_backward"):
		apply_central_force(move_force* -global_transform.basis.z)

	if Input.is_action_pressed("movement_left"):
		apply_central_force(move_force* global_transform.basis.x)

	if Input.is_action_pressed("movement_right"):
		apply_central_force(move_force* -global_transform.basis.x)
	
	#jump:
	if Input.is_action_just_pressed("movement_jump"):
		apply_impulse(Vector3.UP, jump_force* global_transform.basis.y)
