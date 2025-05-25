extends RichTextLabel


func _on_h_scroll_bar_value_changed(value: float) -> void:
	text = "Grass Density: " + str(value)
	pass # Replace with function body.
