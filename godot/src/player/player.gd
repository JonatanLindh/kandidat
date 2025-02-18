extends CharacterBody3D

@onready var head: Node3D = $Head

var current_speed = 5.0
const JUMP_VELOCITY = 4.5
var mouse_sensitivity = 0.1
var sprint_factor = 2

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	
func _input(event):
	if event is InputEventMouseMotion and not is_on_floor():
		rotate_y(deg_to_rad(-event.relative.x * mouse_sensitivity))
		head.rotate_x(deg_to_rad(-event.relative.y * mouse_sensitivity))
	elif Input.is_action_pressed("speedup"):
		current_speed = current_speed + 1
	elif Input.is_action_pressed("speeddown"):
		current_speed = current_speed - 1
		
func _physics_process(delta: float) -> void:
	var direction := Vector3.ZERO
	
	if Input.is_action_pressed("up"):
		velocity.y += Vector3(0,1,0).y * current_speed
	elif Input.is_action_pressed("down"):
		velocity.y += Vector3(0,-1,0).y * current_speed
	else:
		velocity.y = move_toward(velocity.y, 0, current_speed)
	
	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		if Input.is_action_pressed("sprint"):
			apply_velocity(direction, sprint_factor)
		else:
			apply_velocity(direction, 1)
	else:
		velocity.x = move_toward(velocity.x, 0, current_speed)
		velocity.z = move_toward(velocity.z, 0, current_speed)
	

	move_and_slide()

func apply_velocity(dir : Vector3, speed_multiplier):
	velocity.x = dir.x * current_speed * speed_multiplier
	velocity.z = dir.z * current_speed * speed_multiplier
