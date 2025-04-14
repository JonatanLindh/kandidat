@tool
extends GravityBody

@onready var star_mesh: MeshInstance3D = $StarMesh

var colors := [
	Color(1, 0.14, 0),   # Red (Red dwarf or red giant)
	Color(1, 0.5, 0),    # Orange (Orange dwarf)
	Color(1, 1, 1),      # White (White star)
	Color(0.5, 0.5, 1),  # Light blue (A-type star)
	Color(0.2, 0.2, 1),  # Blue (Hot B-type star)
	Color(0.1, 0.1, 1),  # Very hot blue (O-type star)
	Color(0.8, 0.8, 1),  # Pale blue-white (F-type star)
	Color(0.9, 0.8, 0),  # Yellow-orange (K-type star)
	Color(0.8, 0.6, 0.4) # Yellow-brownish (G-type star, slightly more red)
]

@export var seed : int = 10:
	set(new_seed):
		seed = new_seed
	get():
		return seed

@export var radius := 5.0:
	set(r):
		radius = r
		if (star_mesh == null):
			star_mesh = $StarMesh
		star_mesh.set_radius(r)
		
func _ready() -> void:
	if star_mesh != null:
		var rng = RandomNumberGenerator.new()
		rng.seed = seed
		var random_i = rng.randf_range(0, len(colors)-1)
		# TODO add random here after seed has been integrated
		var star_color_random = colors[1]
		print(star_color_random)
		star_mesh.set_color(star_color_random)
	else:
		print("Star mesh not found.")
