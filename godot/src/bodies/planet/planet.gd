@tool
extends GravityBody

var atmosphere
#var planet_gravity_field: PlanetGravityField
var planet_gravity_field: PlanetGravityField;


@export var planet_data: PlanetData:
	set(val):
		planet_data = val
		generate()
		if planet_data != null && !planet_data.is_connected("changed", generate):
			planet_data.connect("changed", generate)

func _ready() -> void:
	planet_data = planet_data.duplicate()
	generate()
	
	#Create gravity field
	planet_gravity_field = PlanetGravityField.new()
	planet_gravity_field.radius = planet_data.radius
	
func _process(delta: float) -> void:
	set_atmosphere_sun_dir()
	
func generate_atmosphere() -> void:
	atmosphere = get_node_or_null("Atmosphere")
	if atmosphere == null:
		return
		
	var radius = planet_data.radius
	atmosphere.radius = radius
	# call method when node is ready
	call_deferred("set_atmosphere_sun_dir")

func set_atmosphere_sun_dir() -> void:
	var sun_dir = (planet_data.sun_position - global_position).normalized()
	atmosphere.sun_direction = sun_dir


func generate() -> void:
	planet_data.min_height = 99999.0
	planet_data.max_height = 0.0
	for child in get_children():
		var face := child as PlanetMeshFace
		if face:
			face.regenerate_mesh(planet_data)
	generate_atmosphere()
