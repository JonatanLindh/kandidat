@tool
extends Node3D

@export var planets = [];
@export var numberOfPlanets:int = 0
@export var generate: bool:
	set(val):
		print("dawg");
		generatePlanets(self.numberOfPlanets);
		
@export var clear: bool:
	set(val):
		for p in planets:
			var nodeName = "./Planet" + str(p);
			var pnode = get_node(nodeName)
			if (is_instance_valid(pnode)):
				pnode.queue_free()
		planets.clear();
		

const G = 1.0;
const SUN_MASS = 1000.0;
var PLANET_SCENE:PackedScene = load("res://src/bodies/planet/planet.tscn");
var rand = RandomNumberGenerator.new();



func orbitRadiusFromSpeed(v:float):
	return SUN_MASS*G/pow(v,2)

func orbitSpeedFromRadius(r:float):
	return sqrt(SUN_MASS*G/r)

func randomOrbitRadius():
	return rand.randf_range(50,80);
	
func randomOrbitAngle():
	return rand.randf_range(0, 2*PI);
	
func randomPlanetMass():
	return rand.randf_range(1,2);

func randomPlanetRadius():
	return rand.randf_range(1,5);


func generatePlanet(planetRadius = 0, planetMass = 0, orbitRadius = 0, orbitSpeed= 0):
	#Planet stuff
	if (planetRadius == 0):
		planetRadius = randomPlanetRadius();
	if (planetMass == 0):
		planetMass = randomPlanetMass();
	
	#Orbit stuff
	var orbitAngle = randomOrbitAngle();
	if (orbitRadius == 0):
		orbitRadius = randomOrbitRadius();
	if (orbitSpeed == 0):
		orbitSpeed = orbitSpeedFromRadius(orbitRadius);
	var randomID = rand.randi_range(100000, 999999);
	var planetInstance = PLANET_SCENE.instantiate();
	planetInstance.mass = planetMass;
	planetInstance.velocity = Vector3(cos(orbitAngle)*orbitSpeed,0,sin(orbitAngle)*orbitSpeed)
	planetInstance.position = Vector3(sin(orbitAngle)*orbitRadius,0,cos(orbitAngle)*orbitRadius)
	planetInstance.planet_data.radius = planetRadius
	planetInstance.name = "Planet" + str(randomID);
	planetInstance.trajectory_color = Color(rand.randi_range(0,255),rand.randi_range(0,255),rand.randi_range(0,255))
	add_child(planetInstance);
	planetInstance.owner = self
	planets.append(randomID);
	

func generatePlanets(n:int = 0):
	var baseDistance = 30;
	const step = 200;
	for i in n:
		generatePlanet(0,0,baseDistance + i*step);
