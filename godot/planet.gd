@tool
extends Node3D

@export var planet_data: PlanetData:
	set(val):
		planet_data = val
		generate()
		if planet_data != null && !planet_data.is_connected("changed", generate):
			planet_data.connect("changed", generate)

func _ready() -> void:
	generate()

func generate() -> void:
	planet_data.min_height = 99999.0
	planet_data.max_height = 0.0
	for child in get_children():
		var face := child as PlanetMeshFace
		face.regenerate_mesh(planet_data)
