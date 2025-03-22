extends Node
class_name OdGenerator
## Code attributed to Haritz Nur Fauzan
## https://medium.com/@mharitsnf/generating-texture-for-accelerating-atmospheric-scattering-in-godot-engine-4-1dca2d84452c

@export var filename : String = "od_tex.png" :
	set(value):
		filename = "od_tex" + value + ".png"
		print("Filename set to: ", filename)
	get():
		return filename

@export_category("Optical Depth Data")
@export var optical_depth_sample_size : int = 50 :
	set(value):
		optical_depth_sample_size = value
		print("Optical depth sample size set to: ", optical_depth_sample_size)
@export var texture_width : int = 10000
@export var texture_height : int = 10000

@export_category("Density Falloffs")
@export var density_falloff_strength : float = 1.0 :
	set(value):
		density_falloff_strength = value
		print("Density falloff strength set to: ", density_falloff_strength)

@export var r_density_falloff : float = 20.0 :
	set(value):
		r_density_falloff = value
		print("Rayleigh density falloff set to: ", r_density_falloff)

@export var m_density_falloff : float = 20.0 :
	set(value):
		m_density_falloff = value
		print("Mie density falloff set to: ", m_density_falloff)

@export_category("Object Locations")
@export var planet_center : Vector3 = Vector3.ZERO :
	set(value):
		planet_center = value
		print("Planet center set to: ", planet_center)

@export var sun_center : Vector3 = Vector3.ZERO :
	set(value):
		sun_center = value
		print("Sun center set to: ", sun_center)

@export_category("Radiuses")
@export var atmosphere_radius : float = 300.0 :
	set(value):
		atmosphere_radius = value
		print("Atmosphere radius set to: ", atmosphere_radius)

@export var planet_radius : float = 100.0 :
	set(value):
		planet_radius = value
		print("Planet radius set to: ", planet_radius)

func _calculate_optical_depth() -> Image:
	# create image
	var img := Image.create(texture_width, texture_height, false, Image.FORMAT_RGBF)

	# initialize step size for increasing sample point height
	var y_pos : float = planet_radius
	var y_step : float = (atmosphere_radius - planet_radius) / float(texture_width - 1.)

	for x in range(texture_width):
		print("current X: ", x)

		for y in range(texture_height):
			# calculate dot product based on texture height
			# and find the rotation angle in radians
			var dot_product = 2. * (1. - float(y) / float(texture_height - 1.)) - 1. # converts 0-1 range to 1-(-1)
			var angle = snappedf(acos(dot_product), .0001)

			# find sample point and direction based on the rotation angle
			var epsilon = Vector3(0.002, 0.002, 0.002)
			var sample_point : Vector3 = Vector3(0., y_pos, 0.)
			var dir : Vector3 = sample_point.rotated(Vector3.FORWARD, angle).normalized()

			# find the length from the current point to the end of atmosphere
			var ray_length : float = _ray_sphere_intersect(sample_point, dir - epsilon).y 

			# calculate step size, and initialize optical depth
			var od_step : float = ray_length / float(optical_depth_sample_size - 1.)
			var od : Vector2 = Vector2.ZERO;

			# calculate optical depth loop
			# dont forget to multiply with step size in the end (in shader program)
			for i in range(optical_depth_sample_size):
				od += _calculate_local_density(sample_point)
				sample_point += dir * od_step

			# write the result
			img.set_pixel(x, y, Color(od.x, od.y, 0.))

	# update height
	y_pos += y_step

	return img

func _ray_sphere_intersect(start : Vector3, dir : Vector3) -> Vector2:
	var offset : Vector3 = start - planet_center;
	var a : float = dir.dot(dir);
	var b : float = 2.0 * offset.dot(dir);
	var c : float = offset.dot(offset) - (atmosphere_radius * atmosphere_radius);
	var d : float = (b*b) - 4.0*a*c;

	# if there is no intersection, return some invalid number
	if (d < 0.): return Vector2(1e5,-1e5);

	# point near is 0 if point is on camera.
	var s : float = sqrt(d);
	var start_intersect = max(0., (-b - s) / (2. * a));
	var end_intersect = (-b + s) / (2. * a);

	# dist to the atmosphere and dist through the atmosphere
	return Vector2(start_intersect, end_intersect - start_intersect);


func _calculate_local_density(point : Vector3) -> Vector2:
	var height = (planet_center - point).length() - planet_radius;
	var height01 = height / (atmosphere_radius - planet_radius);
	return Vector2(
	exp(-height01 * r_density_falloff) * density_falloff_strength,
	exp(-height01 * m_density_falloff) * density_falloff_strength
	);

func generate_od_img():
	if filename.is_empty(): return

	print("Saving optical depth texture as file: %s" % filename)

	var od_img := _calculate_optical_depth()
	od_img.save_png("res://src/bodies/planet/atmosphere/textures/%s.png" % filename)

	var img_bytes = od_img.get_data()
	var img_dat := {
		"width": od_img.get_width(),
		"height": od_img.get_height(),
		"format": od_img.get_format(),
		"blen": len(img_bytes)
	}

	var img_out = FileAccess.open("res://src/bodies/planet/atmosphere/textures/%s.i" % filename, FileAccess.WRITE)
	var dat_out = FileAccess.open("res://src/bodies/planet/atmosphere/textures/%s.idat" % filename, FileAccess.WRITE)

	img_out.store_buffer(img_bytes)
	dat_out.store_line(JSON.stringify(img_dat))

	img_out.close()
	dat_out.close()

	print("File is saved!")
	get_tree().quit()
