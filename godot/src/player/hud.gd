extends CanvasLayer
@onready var position_label: Label = $Control/PositionLabel
@onready var speed_label: Label = $Control/SpeedLabel
@onready var fps_label: Label = $Control/FpsLabel
@onready var help_label: Label = $Control/HelpLabel

var show_help := false
var controls_text := "-WASD for directional movement 
						\n-Mouse movement to look around 
						\n-Increase base speed with arrowkey up or scroll wheel up 
						\n-Decrease base speed with arrowkey down or scroll wheel down
						\n-Spacebar to jump
						\n-1 to roll counter clockwise
						\n-4 to roll clockwise
						\n-v to stop flying when close to a planet
						\n-h to close this text"

func _ready():
	var player = get_parent()
	if player:
		player.updated_status.connect(_on_player_updated_status)

func _process(_delta: float) -> void:
	fps_label.text = "FPS: " + str(Engine.get_frames_per_second())
	if Input.is_action_just_pressed("ui_filedialog_show_hidden"):
		show_help = not show_help
		help_label.text = controls_text if show_help else "Press h for help"
		help_label.scale = help_label.scale / 1.5 if show_help else help_label.scale * 1.5
		

func _on_player_updated_status(position, speed):
	position_label.text = "x: %.2f\ny: %.2f\nz: %.2f" % [position.x, position.y, position.z]
	speed_label.text = "current speed: " + str(ceil(speed))
	
