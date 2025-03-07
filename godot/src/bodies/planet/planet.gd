@tool
extends GravityBody

@export var planet_data: PlanetData:
	set(val):
		planet_data = val
		generate()
		if planet_data != null && !planet_data.is_connected("changed", generate):
			planet_data.connect("changed", generate)

func _ready() -> void:
	planet_data = planet_data.duplicate()
	generate()

func generate() -> void:
	planet_data.min_height = 99999.0
	planet_data.max_height = 0.0
	for child in get_children():
		var face := child as PlanetMeshFace
		if face:
			face.regenerate_mesh(planet_data)
