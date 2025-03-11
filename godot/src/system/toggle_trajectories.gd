extends CheckButton

@onready var gravity_controller: GravityController = $"../GravityController"

func _on_toggled(toggled_on: bool) -> void:
	gravity_controller.show_trajectories_ingame = toggled_on
