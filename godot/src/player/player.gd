extends CharacterBody3D
class_name Player

@onready var head: Node3D = $Head
@onready var camera_3d: Camera3D = $Head/Camera3D

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

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	PlayerVariables.player_position = camera_3d.global_transform.origin
	PlayerVariables.camera_direction = -camera_3d.global_transform.basis.z.normalized()

func _input(event):
	if event is InputEventMouseMotion and Input.mouse_mode != Input.MOUSE_MODE_VISIBLE:
		rotate_y(deg_to_rad(-event.relative.x * mouse_sensitivity))
		head.rotate_x(deg_to_rad(-event.relative.y * mouse_sensitivity))
		head.rotation.x = clamp(head.rotation.x,deg_to_rad(-89),deg_to_rad(89) )
		PlayerVariables.camera_direction = -camera_3d.global_transform.basis.z.normalized()
	
	if Input.is_action_just_pressed("ui_cancel"):
		toggle_mouse_lock()
		
func _physics_process(delta: float) -> void:
	if not in_gravity_field:
		in_space_state_movement(delta)
	else:
		in_gravity_field_movement(delta)

func apply_velocity(dir : Vector3, speed_multiplier):
	velocity.x = dir.x * current_speed * speed_multiplier
	velocity.z = dir.z * current_speed * speed_multiplier
	if flying and not floating_flag:
		velocity.y = dir.y * current_speed * speed_multiplier

func handle_rotation(direction : Vector3, delta : float):
	var x_axis_basis = direction.cross(get_gravity()).normalized()
	var y_axis_basis = get_gravity().normalized()
	var z_axis_basis = direction.normalized()
	var new_basis = Basis(x_axis_basis, y_axis_basis, z_axis_basis).orthonormalized()
	transform.basis = Basis(transform.basis.get_rotation_quaternion().slerp(
		new_basis, delta * 1
	))

func handle_flying() -> void:
	if Input.is_action_just_pressed("up"):
		floating_flag = true
		velocity.y += float_speed * current_speed * vertical_multiplier;
	elif Input.is_action_just_pressed("down"):
		floating_flag = true
		velocity.y += -float_speed * current_speed * vertical_multiplier;
	if Input.is_action_just_released("up"):
		floating_flag = false
		velocity.y = move_toward(velocity.y, 0, float_speed)
	elif Input.is_action_just_released("down"):
		floating_flag = false
		velocity.y = move_toward(velocity.y, 0, float_speed)

		
func emit_player_status_changed() -> void:
	# for updating UI
	var speed = velocity.length()
	if position != last_position or !is_equal_approx(speed, last_speed):
		updated_status.emit(position, speed)
		last_position = position
		last_speed = speed
	PlayerVariables.player_position = global_position
	PlayerVariables.camera_direction = -camera_3d.global_transform.basis.z.normalized()
	PlayerVariables.player_velocity = velocity

func toggle_mouse_lock():
	if Input.mouse_mode == Input.MOUSE_MODE_VISIBLE:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	else:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	
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
	velocity = Vector3.ZERO
	gravity_strength = 0
	flying = true
	# Reset rotation back to world UP smoothly

	# Reset head (camera) to match new upright position
	print("Im exit")

func in_space_state_movement(delta : float):
	if Input.is_action_pressed("speedup"):
		current_speed = current_speed + 1
	elif Input.is_action_pressed("speeddown"):
		current_speed = max(1, current_speed - 1)

	handle_flying()
		
	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")	
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		if flying:
			direction = (head.global_transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
		if Input.is_action_pressed("sprint"):
			apply_velocity(direction, sprint_factor)
			camera_3d.fov = base_fov * 1.1
		else:
			apply_velocity(direction, 1)
			camera_3d.fov = base_fov
	else:
		velocity.x = move_toward(velocity.x, 0, current_speed)
		velocity.z = move_toward(velocity.z, 0, current_speed)
		if not floating_flag and flying:
			velocity.y = move_toward(velocity.y, 0, current_speed)
	
	emit_player_status_changed()
	move_and_slide()

func in_gravity_field_movement(delta : float):
	if flying:
		current_speed = max(planet_velocity.length() + BASE_SPEED, current_speed)	
	else:
		current_speed = planet_velocity.length() + BASE_SPEED
	
	planet_velocity = PlayerVariables.planet_velocity
	gravity_vector = get_gravity()
	
	if Input.is_action_just_pressed("fly"):
		flying = not flying
		velocity = Vector3.ZERO if flying else gravity_vector * delta * current_speed
	
	if flying:
		handle_flying()
		if Input.is_action_pressed("speedup"):
			current_speed = current_speed + 1
		elif Input.is_action_pressed("speeddown"):
			current_speed = max(1, current_speed - 1)

	if is_falling():
		velocity += gravity_vector * delta
	
		
	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")	
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		if flying:
			direction = (head.global_transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
		if Input.is_action_pressed("sprint"):
			apply_velocity(direction, sprint_factor)
			camera_3d.fov = base_fov * 1.1
		else:
			apply_velocity(direction, 1)
			camera_3d.fov = base_fov
	else:
		if flying:
			velocity.x = move_toward(velocity.x, planet_velocity.x, current_speed)
			velocity.z = move_toward(velocity.z, planet_velocity.z, current_speed)
			if not floating_flag:
				velocity.y = move_toward(velocity.y, planet_velocity.y, current_speed)
		if is_on_floor():
			handle_rotation(-gravity_vector.normalized(), 0.1)
			flying = false
			print("NOT FLUYING")
			velocity.x = move_toward(velocity.x, planet_velocity.x + get_gravity().x * gravity_strength, current_speed)
			velocity.z = move_toward(velocity.z, planet_velocity.z + get_gravity().z * gravity_strength, current_speed)
			velocity.y = move_toward(velocity.y, planet_velocity.y + get_gravity().y * gravity_strength, current_speed)
			
	if not flying and is_on_floor():
		velocity += planet_velocity
	
	emit_player_status_changed()
	move_and_slide()

func is_falling() -> bool:
	return not flying and not floating_flag and in_gravity_field and not is_on_floor()
