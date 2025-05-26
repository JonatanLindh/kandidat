extends Node3D

@export var start_scene : PackedScene

@onready var system : Node3D = $System
@onready var seed_text_edit : TextEdit = %SeedTextEdit
@export var generate_planets : bool = true

@export_category("May cause crashes:")
@export var display_orbits : bool = false

func _ready() -> void:
	if generate_planets:
		if display_orbits:
			HudSignalBus.emit_signal("orbits_visibility", true)
		var menu_seed = randi()
		system.generateSystemFromSeed(123)
	

func _on_start_button_pressed() -> void:
	var seed
	if(seed_text_edit.text == ""):
		seed = randi()
	else:
		seed = seed_text_edit.text
	
	GameSettings.SEED = seed
	get_tree().change_scene_to_packed(start_scene)
	
