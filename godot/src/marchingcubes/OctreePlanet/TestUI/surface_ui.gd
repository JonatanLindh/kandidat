extends CanvasLayer
@onready var ui_grass_density = $Control/MarginContainer/VBoxContainer/HScrollBar
@onready var ui_surface_amount = $Control/MarginContainer/VBoxContainer/SpinBox

signal apply_button_pressed(grass_density, surface_amount)


func _on_button_pressed() -> void:
	var grass_density = ui_grass_density.value
	var surface_amount = int(round(ui_surface_amount.value))
	apply_button_pressed.emit(grass_density, surface_amount)
	pass # Replace with function body.
