@tool
extends MeshInstance3D

func set_radius(r: float):
    var new_mesh = self.mesh.duplicate()
    new_mesh.radius = r
    new_mesh.height = r * 2
    self.mesh = new_mesh