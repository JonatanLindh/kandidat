@tool
extends GravityBody

@onready var star_mesh: MeshInstance3D = $StarMesh

@export var radius := 5.0:
	set(r):
		radius = r
		if (star_mesh == null):
			star_mesh = $StarMesh
		star_mesh.set_radius(r)

@export var star_color : Color = Color.from_rgba8(255,0,0):
	get():
		return star_color
	set(c):
		star_color = c

@export var star_corona_color : Color = star_color.lightened(0.5).lerp(Color(1,1,1), 0.4):
	get():
		return star_color
