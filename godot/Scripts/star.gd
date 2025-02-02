@tool
extends Node3D

@onready var star_mesh: MeshInstance3D = $StarMesh

@export var radius := 5.0:
	set(r):
		radius = r
		star_mesh.set_radius(r)
