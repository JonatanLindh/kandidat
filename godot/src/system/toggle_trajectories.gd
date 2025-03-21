extends CheckButton

@onready var gravity_controller: GravityController = $"../GravityController"


func _button_pressed():
	print("Hello world!")


func _on_toggled(toggled_on: bool) -> void:
	gravity_controller.show_trajectories_ingame = toggled_on
