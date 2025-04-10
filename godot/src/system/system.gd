@tool
extends Node3D

@export var numberOfPlanets:int = 5;
@export var distanceBetweenPlanets = 50;
@export var baseDistanceFromSun = 100;
@export var generate: bool:
	set(val):
		generatePlanets(self.rand,self.numberOfPlanets);
		
@export var clear: bool:
	set(val):
		clearBodies();
		
@onready var SUN = ($"./GravityController/Star")

const G = 1.0;
var PLANET_SCENE:PackedScene = load("res://src/bodies/planet/planet.tscn");
var PLANET_MARCHING_CUBE_SCENE:PackedScene = load("res://src/bodies/planet/planet_marching_cube.tscn");
var MOON_SCENE:PackedScene = load("res://src/bodies/moon/moon.tscn"); # DOESNT WORK THE MOON SCENE IS ONLY A TOOL?!??!
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
			maxR = max(maxR,pnode.position.distance_to(Vector3.ZERO));
	return maxR;

func _ready() -> void:
	if Engine.is_editor_hint():
		return;
	#clearBodies();
	#generatePlanets(numberOfPlanets, rand)

func orbitRadiusFromSpeed(v:float):
	return SUN.mass*G/pow(v,2)

func orbitSpeedFromRadius(r:float, m:float):
	return sqrt(m*G/r)

const MOON_ORBIT_RATIO_PLANET_DISTANCE = 80.
const MIN_ORBIT_RADIUS = 2.
const MAX_ORBIT_RADIUS = 80.
const MIN_ORBIT_ANGLE = 0.
const MAX_ORBIT_ANGLE = 2. * PI
const MIN_PLANET_MASS = 10.
const MAX_PLANET_MASS = 30.
const MIN_PLANET_RADIUS = 0.5
const MAX_PLANET_RADIUS = 4.

func randomOrbitRadius(r):
	return r.randf_range(MIN_ORBIT_RADIUS, MAX_ORBIT_RADIUS)
	
func randomOrbitAngle(r):
	return r.randf_range(MIN_ORBIT_ANGLE, MAX_ORBIT_ANGLE)
	
func randomPlanetMass(r):
	return r.randf_range(MIN_PLANET_MASS, MAX_PLANET_MASS)

func randomPlanetRadius(r):
	return r.randf_range(MIN_PLANET_RADIUS, MAX_PLANET_RADIUS)


func generatePlanet(r,planetRadius = 0, planetMass = 0, orbitRadius = 0, orbitSpeed = 0):
	#Planet stuff
	if (planetRadius == 0):
		planetRadius = randomPlanetRadius(r);
	if (planetMass == 0):
		planetMass = randomPlanetMass(r);
	
	#Orbit stuff
	var orbitAngle = randomOrbitAngle(r);
	if (orbitRadius == 0):
		orbitRadius = randomOrbitRadius(r);
	if (orbitSpeed == 0):
		orbitSpeed = orbitSpeedFromRadius(orbitRadius, SUN.mass);
		
	var planetInstance = spawnPlanetMarchingCube(planetRadius, planetMass, orbitRadius, orbitSpeed, orbitAngle)
	return planetInstance

func generateMoon(r, planetInstance, orbitRadius):
	var moonRadius = r.randf_range(planetInstance.Radius/10, planetInstance.Radius/5)

	var orbitAngle = randomOrbitAngle(r);
	var orbitSpeed = orbitSpeedFromRadius(orbitRadius, planetInstance.mass);
	var moonMass = 0.01
	
	return spawnMoon(moonRadius, moonMass, orbitRadius, orbitSpeed, orbitAngle, planetInstance.position, planetInstance.velocity);

func generatePlanets(n:int, r):
	for i in n:
		var planetInstance = generatePlanet(r, 0,0,baseDistanceFromSun + i*distanceBetweenPlanets,0);
		var moons = 1
		for m in range(moons):
			generateMoon(r,planetInstance, (m+1) * distanceBetweenPlanets / MOON_ORBIT_RATIO_PLANET_DISTANCE)

func generateSystemFromSeed(s:int):
	print(s);
	clearBodies();
	var r = RandomNumberGenerator.new();
	r.seed = s;
	
	var n = r.randi_range(3,10);
	generatePlanets(n,r)


func spawnMoon(moonRadius, moonMass, orbitRadius, orbitSpeed, orbitAngle, primaryPosition = Vector3.ZERO, primaryVelocity= Vector3.ZERO):
	var randomID = rand.randi_range(100000, 999999);
	var bodyInstance = PLANET_SCENE.instantiate(); #Uses PLANET_SCENE for now since moon scene didn't quite work
	bodyInstance.mass = moonMass;
	bodyInstance.velocity = primaryVelocity + Vector3(cos(orbitAngle)*orbitSpeed,0,-sin(orbitAngle)*orbitSpeed)
	bodyInstance.position = primaryPosition + Vector3(sin(orbitAngle)*orbitRadius,0,cos(orbitAngle)*orbitRadius)
	bodyInstance.planet_data.radius = moonRadius
	bodyInstance.name = "Body" + str(randomID);
	bodyInstance.trajectory_color = Color.from_hsv(rand.randf_range(0,1),0.80,0.80)*3;
	$GravityController.add_child(bodyInstance);
	bodyInstance.owner = self
	bodies.append(randomID);
	return bodyInstance;


func spawnPlanetMarchingCube(planetRadius, planetMass, orbitRadius, orbitSpeed, orbitAngle):
	var randomID = rand.randi_range(100000, 999999);
	var bodyInstance = PLANET_MARCHING_CUBE_SCENE.instantiate();
	bodyInstance.mass = planetMass;
	bodyInstance.velocity = Vector3(cos(orbitAngle)*orbitSpeed,0,-sin(orbitAngle)*orbitSpeed)
	bodyInstance.position = Vector3(sin(orbitAngle)*orbitRadius,0,cos(orbitAngle)*orbitRadius)
	bodyInstance.Radius = planetRadius
	bodyInstance.name = "Body" + str(randomID);
	bodyInstance.trajectory_color = Color.from_hsv(rand.randf_range(0,1),0.80,0.80)*3;
	$GravityController.add_child(bodyInstance);
	bodyInstance.owner = self
	bodies.append(randomID);
	return bodyInstance;
