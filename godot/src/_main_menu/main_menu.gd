extends Node3D

@export var start_scene : PackedScene

@onready var system : Node3D = $System
@onready var seed_text_edit : TextEdit = %SeedTextEdit

func _ready() -> void:
	HudSignalBus.emit_signal("orbits_visibility", true)
	var menu_seed = randi()
	system.generateSystemFromSeed(menu_seed)
	

func _on_start_button_pressed() -> void:
	var seed
	if(seed_text_edit.text == ""):
		seed = randi()
	else:
		seed = seed_text_edit.text
	
	GameSettings.SEED = seed
	get_tree().change_scene_to_packed(start_scene)
	
