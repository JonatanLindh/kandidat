extends CanvasLayer
@onready var pos_x_label: Label = %PosXLabel
@onready var pos_y_label: Label = %PosYLabel
@onready var pos_z_label: Label = %PosZLabel

@onready var speed_label: Label = %SpeedLabel
@onready var fps_label: Label = %FpsLabel
@onready var help_label: Label = %HelpLabel

var show_help := false
var controls_text := "-WASD for directional movement 
						\n-Mouse movement to look around 
						\n-Increase base speed with arrowkey up or scroll wheel up 
						\n-Decrease base speed with arrowkey down or scroll wheel down
						\n-Spacebar to jump
						\n-1 to roll counter clockwise
						\n-4 to roll clockwise
						\n-v to stop flying when close to a planet
						\n-h to close this text
						\n-f to turn on/off flashlight"

func _ready():
	var player = get_parent()
	if player:
		player.updated_status.connect(_on_player_updated_status)

func _process(_delta: float) -> void:
	fps_label.text = "FPS: %.0f" % Engine.get_frames_per_second()
	if Input.is_action_just_pressed("ui_filedialog_show_hidden"):
		show_help = not show_help
		help_label.text = controls_text if show_help else "Press h for help"
		help_label.scale = help_label.scale / 1.5 if show_help else help_label.scale * 1.5
		

func _on_player_updated_status(position, speed):
	pos_x_label.text = "%.2f /" % position.x
	pos_y_label.text = "%.2f /" % position.y
	pos_z_label.text = "%.2f" % position.z
	
	speed_label.text = "Speed: %.0f AU/s" % ceil(speed)
	
