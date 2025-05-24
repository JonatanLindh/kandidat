extends Node3D

@onready var system : Node3D = $System

@onready var seed_text_edit : TextEdit = $CanvasLayer/PanelContainer/VBoxContainer/SeedTextEdit
@onready var start_button : Button = $CanvasLayer/PanelContainer/VBoxContainer/StartButton

@export var start_scene : PackedScene

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	HudSignalBus.emit_signal("orbits_visibility", true)
	var menu_seed = randi()
	system.generateSystemFromSeed(menu_seed)
	

func _on_start_button_pressed() -> void:
	GameSettings.SEED = seed_text_edit.text
	get_tree().change_scene_to_packed(start_scene)
