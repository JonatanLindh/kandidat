@tool
extends GravityBody

func _init() -> void:
	mass = 5000
	trajectory_color = Color.WHITE

func set_test_body_mass(new_mass):
	mass = new_mass
	
func set_test_body_velocity(vel):
	velocity = vel
