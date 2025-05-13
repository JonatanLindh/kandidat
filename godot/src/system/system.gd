@tool
extends Node3D

@export var MOON_ORBIT_RATIO_PLANET_DISTANCE: float = 30.0
@export var MIN_NUMBER_OF_PLANETS: int = 3
@export var MAX_NUMBER_OF_PLANETS: int = 8
@export var MIN_ORBIT_ANGLE: float = 0.0
@export var MAX_ORBIT_ANGLE: float = 2.0 * PI
@export var MIN_PLANET_MASS: float = 5000.0
@export var MAX_PLANET_MASS: float = 10000.0
@export var MIN_PLANET_RADIUS: float = 10.0
@export var MAX_PLANET_RADIUS: float = 15.0
@export var DISTANCE_BETWEEN_PLANETS: float = 300.0;
@export var BASE_DISTANCE_FROM_SUN: float = 300.0;

@export var generate: bool:
	set(val):
		generateSystemFromSeed(0);
		
@export var clear: bool:
	set(val):
		clearBodies();
		
@onready var SUN = ($"./GravityController/Star")
@onready var GC = ($"./GravityController")

@onready var G = GC.grav_const;
var PLANET_SCENE: PackedScene = load("res://src/bodies/planet/planet.tscn");
var PLANET_MARCHING_CUBE_SCENE: PackedScene = load("res://src/bodies/planet/planet_marching_cube.tscn");
var MOON_SCENE: PackedScene = load("res://src/bodies/moon/moon.tscn"); # DOESNT WORK THE MOON SCENE IS ONLY A TOOL?!??!
var rand = RandomNumberGenerator.new();
var bodies = [];


func clearBodies():
	for p in bodies:
		var nodeName = "./GravityController/Body" + str(p);
		var pnode = get_node(nodeName)
		if (is_instance_valid(pnode)):
			pnode.queue_free()
	bodies.clear();

func getSystemRadius():
	var maxR = 0;
	for p in bodies:
		var nodeName = "./GravityController/Body" + str(p);
		var pnode = get_node(nodeName)
		if (is_instance_valid(pnode)):
			maxR = max(maxR, pnode.position.distance_to(Vector3.ZERO));
	return maxR;
	
func getSystemRadiusFromData(system_data):
	var maxR = 0;
	for p in system_data["planets"]:
		maxR = max(maxR, p.orbit_radius);
	return maxR;

func _ready() -> void:
	if Engine.is_editor_hint():
		return ;

func orbitRadiusFromSpeed(v: float):
	return SUN.mass * G / pow(v, 2)

func orbitSpeedFromRadius(r: float, m: float):
	return sqrt(m * G / r)

func randomOrbitAngle(r):
	return r.randf_range(MIN_ORBIT_ANGLE, MAX_ORBIT_ANGLE)
	
func randomPlanetMass(r):
	return r.randf_range(MIN_PLANET_MASS, MAX_PLANET_MASS)

func randomPlanetRadius(r):
	return r.randf_range(MIN_PLANET_RADIUS, MAX_PLANET_RADIUS)

# Creates a unique seed for every planet in a system based on the seed for that solar system
func generatePlanetDataSeed(systemSeed: int, position: Vector3):
	var seedGen = SeedGenerator.new();
	return seedGen.GenerateSeed(systemSeed, position);

func spawnMoon(moonRadius, moonMass, orbitRadius, orbitSpeed, orbitAngle, primaryPosition = Vector3.ZERO, primaryVelocity = Vector3.ZERO):
	var randomID = rand.randi_range(100000, 999999);
	var bodyInstance = PLANET_SCENE.instantiate(); # Uses PLANET_SCENE for now since moon scene didn't quite work
	bodyInstance.mass = moonMass;
	bodyInstance.position = primaryPosition + Vector3(sin(orbitAngle) * orbitRadius, 0, cos(orbitAngle) * orbitRadius)
	var bodySpeedAroundSun = orbitSpeedFromRadius((bodyInstance.position - SUN.position).length(), SUN.mass)
	var bodyAngleAroundSun = atan2(bodyInstance.position.x - SUN.position.x, bodyInstance.position.z - SUN.position.z)
	var bodyVelocityAroundSun = Vector3(cos(bodyAngleAroundSun) * bodySpeedAroundSun, 0, -sin(bodyAngleAroundSun) * bodySpeedAroundSun)
	bodyInstance.velocity = bodyVelocityAroundSun + Vector3(cos(orbitAngle) * orbitSpeed, 0, -sin(orbitAngle) * orbitSpeed)
	bodyInstance.planet_data.radius = moonRadius
	bodyInstance.name = "Body" + str(randomID);
	bodyInstance.trajectory_color = Color.from_hsv(rand.randf_range(0, 1), 0.80, 0.80) * 3;
	$GravityController.add_child(bodyInstance);
	bodyInstance.owner = self
	bodies.append(randomID);
	return bodyInstance;

func spawnPlanetMarchingCube(planetRadius, planetMass, orbitRadius, orbitSpeed, orbitAngle, r):
	var randomID = rand.randi_range(100000, 999999);
	var planetInstance = PLANET_MARCHING_CUBE_SCENE.instantiate();
	
	planetInstance.mass = planetMass;
	planetInstance.velocity = Vector3(cos(orbitAngle) * orbitSpeed, 0, -sin(orbitAngle) * orbitSpeed)
	planetInstance.position = Vector3(sin(orbitAngle) * orbitRadius, 0, cos(orbitAngle) * orbitRadius)
	planetInstance.set("Radius", planetRadius)
	planetInstance.SunPosition = SUN.global_position;
	planetInstance.name = "Body" + str(randomID);
	planetInstance.trajectory_color = Color.from_hsv(rand.randf_range(0, 1), 0.80, 0.80) * 3;
	
	# Create a new seed for each planet to be used when generating marching cubes planet
	planetInstance._seed = generatePlanetDataSeed(r.seed, planetInstance.position);
	
	$GravityController.add_child(planetInstance);
	
	planetInstance.owner = self
	bodies.append(randomID);
	return planetInstance;


func randomSunType(s: int):
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
	
	var types := [
		"M", # Red (Red dwarf or red giant)
		"K", # Orange (Orange dwarf)
		"A", # White (White star)
		"A", # Light blue (A-type star)
		"B", # Blue (Hot B-type star)
		"O", # Very hot blue (O-type star)
		"F", # Pale blue-white (F-type star)
		"K", # Yellow-orange (K-type star)
		"G" # Yellow-brownish (G-type star, slightly more red)
	]
	
	var index = s % colors.size()
	
	var sun_data = {
		"type": types[index],
		"color": colors[index]
	}
	
	return sun_data
	
func generateSystemDataFromSeed(s: int):
	var r = RandomNumberGenerator.new()
	r.seed = s
	var sun_data = randomSunType(s);

	# Step 1: Generate system variables
	var system_data = {
		"planets": [],
		"moons": [],
		"sun": {"type": sun_data["type"], "color": sun_data["color"]}
	}
	
	generatePlanetsData(r, system_data)
	
	return system_data;

func generateSystemFromSeed(s: int):
	print(s)
	clearBodies()
	var system_data = generateSystemDataFromSeed(s);

	# Step 2: Instantiate objects from system variables
	instantiateSystem(system_data)


func generatePlanetsData(r, system_data):
	var n = r.randi_range(MIN_NUMBER_OF_PLANETS, MAX_NUMBER_OF_PLANETS)
	var orbit_radius = SUN.radius + BASE_DISTANCE_FROM_SUN
	for i in range(n):
		var orbit_increase = log(i + 1) * DISTANCE_BETWEEN_PLANETS
		orbit_radius += orbit_increase;
		var planet_data = generatePlanetData(r, orbit_radius)
		system_data["planets"].append(planet_data)

		var moons_count = max(2, i) - 2
		for m in range(moons_count):
			var moon_orbit_radius = planet_data.radius + (m + 1) * orbit_increase / MOON_ORBIT_RATIO_PLANET_DISTANCE
			var moon_data = generateMoonData(r, planet_data, moon_orbit_radius)
			system_data["moons"].append(moon_data)


func generatePlanetData(r, orbit_radius):
	var planet_radius = randomPlanetRadius(r)
	var planet_mass = randomPlanetMass(r)
	var orbit_angle = randomOrbitAngle(r)
	var orbit_speed = orbitSpeedFromRadius(orbit_radius, SUN.mass)
	var pos = Vector3(sin(orbit_angle) * orbit_radius, 0, cos(orbit_angle) * orbit_radius);
	var vel = Vector3(cos(orbit_angle) * orbit_speed, 0, -sin(orbit_angle) * orbit_speed);
	return {
		"radius": planet_radius,
		"mass": planet_mass,
		"orbit_radius": orbit_radius,
		"orbit_angle": orbit_angle,
		"orbit_speed": orbit_speed,
		"position": pos,
		"velocity": vel
	}


func generateMoonData(r, planet_data, orbit_radius):
	var moon_radius = r.randf_range(planet_data.radius / 10, planet_data.radius / 5)
	var orbit_angle = randomOrbitAngle(r)
	var orbit_speed = orbitSpeedFromRadius(orbit_radius, planet_data.mass)
	var moon_mass = planet_data.mass * 0.0001

	return {
		"radius": moon_radius,
		"mass": moon_mass,
		"orbit_radius": orbit_radius,
		"orbit_angle": orbit_angle,
		"orbit_speed": orbit_speed,
		"primary_position": planet_data.position,
		"primary_velocity": planet_data.velocity
	}


func instantiateSystem(system_data):
	#Set star things
	SUN.star_mesh.set_color(system_data.sun.color);
	
	
	var system_radius = getSystemRadiusFromData(system_data);

	# Instantiate planets
	for planet_data in system_data["planets"]:
		var planetInstance = spawnPlanetMarchingCube(
			planet_data.radius,
			planet_data.mass,
			planet_data.orbit_radius,
			planet_data.orbit_speed,
			planet_data.orbit_angle,
			rand
		)
		var warmth = calculate_planet_warmth(planet_data.orbit_radius, system_radius)
		planetInstance.set("Warmth", warmth);

	# Instantiate moons
	for moon_data in system_data["moons"]:
		spawnMoon(
			moon_data.radius,
			moon_data.mass,
			moon_data.orbit_radius,
			moon_data.orbit_speed,
			moon_data.orbit_angle,
			moon_data.primary_position,
			moon_data.primary_velocity
		)
func calculate_planet_warmth(distance_to_sun: float, system_radius: float) -> float:
	var min_distance = BASE_DISTANCE_FROM_SUN
	if system_radius == 0:
		return 1.0
	
	# calculate warmth using invere squared law, convert to logarithmic scale to get more even distribution for aesthetics.
	var warmth = log(1.0 / pow(distance_to_sun, 2))
	var max_warmth = log(1.0 / pow(min_distance, 2))
	var min_warmth = log(1.0 / pow(system_radius, 2))
	
	# normalize warmth
	var normalized = (warmth - min_warmth) / (max_warmth - min_warmth)
	return clamp(normalized, 0.0, 1.0)
