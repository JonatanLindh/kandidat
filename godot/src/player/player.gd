extends CharacterBody3D

@onready var head: Node3D = $Head
@onready var camera_3d: Camera3D = $Head/Camera3D

signal updated_status(position, speed)

const JUMP_VELOCITY = 4.5
var current_speed = 5.0
var mouse_sensitivity = 0.1
var sprint_factor = 2
var float_speed = 5
var base_fov = 75
var flying := true
var last_position := Vector3.ZERO
var last_speed := 0.0

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	
func _input(event):
	if event is InputEventMouseMotion and Input.mouse_mode != Input.MOUSE_MODE_VISIBLE:
		rotate_y(deg_to_rad(-event.relative.x * mouse_sensitivity))
		head.rotate_x(deg_to_rad(-event.relative.y * mouse_sensitivity))
		head.rotation.x = clamp(head.rotation.x,deg_to_rad(-89),deg_to_rad(89) )
	
	if Input.is_action_just_pressed("speedup"):
		current_speed = current_speed + 1
	elif Input.is_action_just_pressed("speeddown"):
		current_speed = max(1, current_speed - 1)
	
	if Input.is_action_just_pressed("ui_cancel") and Input.mouse_mode != Input.MOUSE_MODE_VISIBLE:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	elif Input.is_action_just_pressed("ui_cancel") and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
		
func _physics_process(delta: float) -> void:
		
	if Input.is_action_just_pressed("fly"):
		flying = not flying
		velocity.y = 0 if flying else get_gravity().y * delta * current_speed
	
	if not flying and not is_on_floor():
		velocity += get_gravity() * delta
	
	if flying:
		handle_flying()
		
	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		if Input.is_action_pressed("sprint"):
			apply_velocity_xz(direction, sprint_factor)
			camera_3d.fov = base_fov * 1.1
		else:
			apply_velocity_xz(direction, 1)
			camera_3d.fov = base_fov
	else:
		velocity.x = move_toward(velocity.x, 0, current_speed)
		velocity.z = move_toward(velocity.z, 0, current_speed)
	emit_player_status_changed()
	move_and_slide()

func apply_velocity_xz(dir : Vector3, speed_multiplier):
	velocity.x = dir.x * current_speed * speed_multiplier
	velocity.z = dir.z * current_speed * speed_multiplier

func handle_flying() -> void:
	if Input.is_action_just_pressed("up"):
		velocity.y += float_speed
	elif Input.is_action_just_pressed("down"):
		velocity.y += -float_speed
	if Input.is_action_just_released("up"):
		velocity.y = move_toward(velocity.y, 0, current_speed)
	elif Input.is_action_just_released("down"):
		velocity.y = move_toward(velocity.y, 0, current_speed)
		
func emit_player_status_changed() -> void:
	var speed = velocity.length()
	if position != last_position or abs(speed-last_speed) > 0:
		updated_status.emit(position, speed)
		last_position = position
		last_speed = speed
