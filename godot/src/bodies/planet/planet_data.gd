@tool
extends Resource

class_name PlanetData

@export var radius := 1.0:
	set(val):
		radius = val
		emit_changed()
		
@export var resolution := 20:
	set(val):
		resolution = val
		emit_changed()

@export var planet_noise: Array[PlanetNoise]:
	set(val):
		planet_noise = val
		emit_changed()
		for n in planet_noise:
			if n != null && !n.is_connected("changed", emit_changed):
				n.connect("changed", emit_changed)

@export var planet_color: GradientTexture1D:
	set(val):
		planet_color = val
		emit_changed()
		if planet_color != null && !planet_color.is_connected("changed", emit_changed):
				planet_color.connect("changed", emit_changed)

var min_height := 99999.0
var max_height := 0.0

func point_on_planet(point_on_sphere: Vector3) -> Vector3:
	var elevation := 0.
	var base_elevation := 0.
	if planet_noise.size() > 0 && planet_noise[0] != null && planet_noise[0].noise_map != null:
		base_elevation = planet_noise[0].noise_map.get_noise_3dv(point_on_sphere * 100.0)
		base_elevation = (base_elevation + 1) / 2.0 * planet_noise[0].amplitude
		base_elevation = max(0.0, base_elevation - planet_noise[0].min_height)
	for n in planet_noise:
		if n == null || n.noise_map == null:
			continue
		
		var mask := 1.0
		if n.use_layer0_as_mask:
			mask = base_elevation
		var level_elevation = n.noise_map.get_noise_3dv(point_on_sphere * 100.0)
		level_elevation = (level_elevation + 1) / 2.0 * n.amplitude
		level_elevation = max(0.0, level_elevation - n.min_height)
		elevation += level_elevation * mask
	return point_on_sphere * radius * (elevation + 1)
