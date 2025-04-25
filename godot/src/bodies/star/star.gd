@tool
extends GravityBody

@onready var star_mesh: MeshInstance3D = $StarMesh

var colors := [
	Color(1, 0.14, 0), # Red (Red dwarf or red giant)
	Color(1, 0.5, 0), # Orange (Orange dwarf)
	Color(1, 1, 1), # White (White star)
	Color(0.5, 0.5, 1), # Light blue (A-type star)
	Color(0.2, 0.2, 1), # Blue (Hot B-type star)
	Color(0.1, 0.1, 1), # Very hot blue (O-type star)
	Color(0.8, 0.8, 1), # Pale blue-white (F-type star)
	Color(0.9, 0.8, 0), # Yellow-orange (K-type star)
	Color(0.8, 0.6, 0.4) # Yellow-brownish (G-type star, slightly more red)
]

@export var radius := 5.0:
	set(r):
		radius = r
		if (star_mesh == null):
			star_mesh = $StarMesh
		star_mesh.set_radius(r)