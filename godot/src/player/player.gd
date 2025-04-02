extends CharacterBody3D
class_name Player

@onready var head: Node3D = $Head
@onready var camera_3d: Camera3D = $Head/Camera3D
@onready var ray_cast_3d: RayCast3D = $RayCast3D

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
var on_sufarce_movement := false

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	PlayerVariables.player_position = camera_3d.global_transform.origin
	PlayerVariables.camera_direction = -camera_3d.global_transform.basis.z.normalized()

func _input(event):
	if event is InputEventMouseMotion and Input.mouse_mode != Input.MOUSE_MODE_VISIBLE:
		var yaw = deg_to_rad(-event.relative.x * mouse_sensitivity)
		var pitch = deg_to_rad(-event.relative.y * mouse_sensitivity)
		rotate(global_basis.y, yaw)
		head.rotate_x(pitch)
		head.rotation.x = clamp(head.rotation.x, deg_to_rad(-89), deg_to_rad(89))

	if event is InputEventMouseButton:
		if event.is_pressed():
			if event.button_index == MOUSE_BUTTON_WHEEL_UP:
				current_speed = max(1, current_speed + 1)
			elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
				current_speed = max(1, current_speed - 1)
	
	if Input.is_action_just_pressed("ui_cancel"):
		toggle_mouse_lock()
		
func _physics_process(delta: float) -> void:
	if not in_gravity_field:
		in_space_state_movement(delta)
	else:
		if flying or not on_sufarce_movement:
			in_gravity_field_movement(delta)
		elif on_sufarce_movement:
			on_planet_movement(delta)
			
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

func in_space_state_movement(delta : float):
	if Input.is_action_pressed("speedup"):
		current_speed = current_speed + 1
	elif Input.is_action_pressed("speeddown"):
		current_speed = max(1, current_speed - 1)

	handle_flying()
		
	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")	
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	apply_flying_movement(Vector3.ZERO, delta)
	emit_player_status_changed()
	move_and_slide()

func in_gravity_field_movement(delta : float):
	current_speed = max(planet_velocity.length() + BASE_SPEED, current_speed)	
	
	planet_velocity = PlayerVariables.planet_velocity
	gravity_vector = get_gravity()
	
	if Input.is_action_just_pressed("fly"):
		flying = not flying
	
	if flying:
		handle_flying()
		if Input.is_action_pressed("speedup"):
			current_speed = current_speed + 1
		elif Input.is_action_pressed("speeddown"):
			current_speed = max(1, current_speed - 1)
		apply_flying_movement(planet_velocity, delta)
	else:
		if is_falling():
			up_direction = -gravity_vector.normalized()
			velocity += gravity_vector * delta
			align_with_vector(gravity_vector, 0.1)
		else:
			velocity = planet_velocity
			on_sufarce_movement = true
	
	
	emit_player_status_changed()
	move_and_slide()

func apply_flying_movement(base_velocity : Vector3, delta : float):
	if Input.is_action_pressed("roll_left"):
		rotate_object_local(Vector3(0, 0, 1), 2 * delta)
	elif Input.is_action_pressed("roll_right"):
		rotate_object_local(Vector3(0, 0, -1), 2 * delta)
	
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
		velocity.x = move_toward(velocity.x, base_velocity.x, current_speed)
		velocity.z = move_toward(velocity.z, base_velocity.z, current_speed)
		if not floating_flag:
			velocity.y = move_toward(velocity.y, base_velocity.y, current_speed)

func on_planet_movement(delta : float):
	current_speed = 3
	
	planet_velocity = PlayerVariables.planet_velocity
	gravity_vector = get_gravity()
	up_direction = -gravity_vector.normalized()
	
	if Input.is_action_just_pressed("fly"):
		on_sufarce_movement = false
		flying = not flying

	if is_falling():
		velocity += (gravity_vector + planet_velocity.normalized()) * delta

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	var forward = global_transform.basis.z
	var right = global_transform.basis.x

	var direction = (head.global_transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	# ensures movement is parallel to the ground
	direction = (direction - up_direction * direction.dot(up_direction)).normalized()

	if direction and not is_falling():
		if Input.is_action_pressed("sprint"):
			velocity.x = direction.x * current_speed * 2 + planet_velocity.x
			velocity.z = direction.z * current_speed * 2 + planet_velocity.z
			velocity.y = direction.y * current_speed * 2 + planet_velocity.y
			camera_3d.fov = base_fov * 1.1
		else:
			velocity.x = direction.x * current_speed + planet_velocity.x
			velocity.z = direction.z * current_speed + planet_velocity.z
			velocity.y = direction.y * current_speed + planet_velocity.y
			camera_3d.fov = base_fov
	else:
		if not is_falling():
			velocity.y = move_toward(velocity.y, planet_velocity.y, current_speed)
			velocity.x = move_toward(velocity.x, planet_velocity.x, current_speed)
			velocity.z = move_toward(velocity.z, planet_velocity.z, current_speed)
	
	print(not is_falling())
	if Input.is_action_just_pressed("ui_accept") and not is_falling():
		velocity += -gravity_vector.normalized() * JUMP_VELOCITY
		
	align_with_vector(gravity_vector, 1)
	emit_player_status_changed()
	move_and_slide()

func is_falling() -> bool:
	return not flying and not floating_flag and in_gravity_field and not is_on_floor() and not ray_cast_3d.is_colliding()

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
	align_with_vector(Vector3.DOWN,0.1)
	print("Im exit")

func apply_velocity(dir : Vector3, speed_multiplier):
	velocity.x = dir.x * current_speed * speed_multiplier
	velocity.z = dir.z * current_speed * speed_multiplier
	if flying and not floating_flag:
		velocity.y = dir.y * current_speed * speed_multiplier

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

func align_with_vector(alignment_vector: Vector3, rotation_speed : float):
	up_direction = -alignment_vector.normalized()
	var current_basis = global_transform.basis

	var forward_direction = (current_basis.z - up_direction * current_basis.z.dot(up_direction)).normalized()
	var right_direction = up_direction.cross(forward_direction).normalized()

	var target_basis = Basis(right_direction, up_direction, forward_direction).orthonormalized()
	global_transform.basis = global_transform.basis.slerp(target_basis, rotation_speed)
	
