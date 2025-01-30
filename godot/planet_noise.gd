@tool
extends Resource

class_name PlanetNoise

@export var noise_map: FastNoiseLite:
	set(val):
		noise_map = val
		emit_changed()
		if noise_map != null && !noise_map.is_connected("changed", emit_changed):
			noise_map.connect("changed", emit_changed)


@export var amplitude := 1.0:
	set(val):
		amplitude = val
		emit_changed()

@export var min_height := 0.0:
	set(val):
		min_height = val
		emit_changed()

@export var use_layer0_as_mask := false:
	set(val):
		use_layer0_as_mask = val
		emit_changed()
