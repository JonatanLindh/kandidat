@tool
extends Node3D

@export var numberOfPlanets:int = 5;
@export var distanceBetweenPlanets = 50;
@export var baseDistanceFromSun = 100;
@export var generate: bool:
	set(val):
		print("dawg");
		generatePlanets(self.rand,self.numberOfPlanets);
		
@export var clear: bool:
	set(val):
		clearPlanets();
		
@onready var SUN = ($"./GravityController/Star")

const G = 1.0;
var PLANET_SCENE:PackedScene = load("res://src/bodies/planet/planet.tscn");
var rand = RandomNumberGenerator.new();
var planets = [];


func clearPlanets():
	for p in planets:
		var nodeName = "./GravityController/Planet" + str(p);
		var pnode = get_node(nodeName)
		if (is_instance_valid(pnode)):
			pnode.queue_free()
	planets.clear();

func getSystemRadius():
	var maxR = 0;
	for p in planets:
		var nodeName = "./GravityController/Planet" + str(p);
		var pnode = get_node(nodeName)
		if (is_instance_valid(pnode)):
			maxR = max(maxR,pnode.position.distance_to(Vector3.ZERO));
	return maxR;

func _ready() -> void:
	if Engine.is_editor_hint():
		return;
	#clearPlanets();
	#generatePlanets(numberOfPlanets, rand)

func orbitRadiusFromSpeed(v:float):
	return SUN.mass*G/pow(v,2)

func orbitSpeedFromRadius(r:float):
	return sqrt(SUN.mass*G/r)

func randomOrbitRadius(r):
	return r.randf_range(2,80);
	
func randomOrbitAngle(r):
	return r.randf_range(0, 2*PI);
	
func randomPlanetMass(r):
	return r.randf_range(1,2);

func randomPlanetRadius(r):
	return r.randf_range(15,15);


func generatePlanet(r,planetRadius = 0, planetMass = 0, orbitRadius = 0, orbitSpeed= 0):
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
		orbitSpeed = orbitSpeedFromRadius(orbitRadius);
	var randomID = rand.randi_range(100000, 999999);
	var planetInstance = PLANET_SCENE.instantiate();
	planetInstance.mass = planetMass;
	planetInstance.velocity = Vector3(cos(orbitAngle)*orbitSpeed,0,-sin(orbitAngle)*orbitSpeed)
	planetInstance.position = Vector3(sin(orbitAngle)*orbitRadius,0,cos(orbitAngle)*orbitRadius)
	planetInstance.planet_data.radius = planetRadius
	# NEEDED FOR ATMOSPHERE, IF MULTIPLE SUN SOLAR SYSTEM, MAYBE USE ONE OF THE SUNS AS THE MAIN LIGHT SOURCE?
	planetInstance.planet_data.sun_position = SUN.global_position
	planetInstance.name = "Planet" + str(randomID);
	planetInstance.trajectory_color = Color.from_hsv(rand.randf_range(0,1),0.80,0.80)*3;
	$GravityController.add_child(planetInstance);
	planetInstance.owner = self
	planets.append(randomID);
	
	
func generatePlanets(n:int, r):
	for i in n:
		generatePlanet(r, 0,0,baseDistanceFromSun + i*distanceBetweenPlanets);

func generateSystemFromSeed(s:int):
	print(s);
	clearPlanets();
	var r = RandomNumberGenerator.new();
	r.seed = s;
	
	var n = r.randi_range(3,10);
	generatePlanets(n,r)
