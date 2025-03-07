@tool
extends GravityBody

@onready var star_mesh: MeshInstance3D = $StarMesh

@export var radius := 5.0:
	set(r):
		radius = r
		if (star_mesh == null):
			star_mesh = $StarMesh
		star_mesh.set_radius(r)
