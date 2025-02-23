extends CanvasLayer
@onready var position_label: Label = $Control/PositionLabel
@onready var speed_label: Label = $Control/SpeedLabel

func _ready():
	var player = get_parent()
	if player:
		player.updated_status.connect(_on_player_updated_status)

func _on_player_updated_status(position, speed):
	position_label.text = "x: %.2f\ny: %.2f\nz: %.2f" % [position.x, position.y, position.z]
	speed_label.text = "current speed: " + str(ceil(speed))
	
