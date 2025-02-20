extends CanvasLayer

@onready var speed_label: Label = $Control/SpeedLabel
@onready var position_label: Label = $Control/PositionLabel
@onready var player: CharacterBody3D = $".."


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	position_label.text = "x:" + str(player.position.x) + "\n" + "y:" + str(player.position.y) + "\n" + "z:" + str(player.position.z)
	speed_label.text = "current speed: " + str(ceil(sqrt(player.velocity.x**2 + player.velocity.y**2 + player.velocity.z**2)))
