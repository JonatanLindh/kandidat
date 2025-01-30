extends Node3D

@export var gravityStrength = 1

func getCelestialBodies() -> Array[CelestialBody]:
	var bodies: Array[CelestialBody] = []
	
	for b in get_children():
		if b is CelestialBody:
			bodies.append(b)
	return bodies
	
func _physics_process(delta: float) -> void:
	var bodies = getCelestialBodies();
	
	for affectee in bodies:
		if not affectee.affectedByGravity:
			continue;
		var acc = Vector3.ZERO
		
		for affecter in bodies:
			if not affecter.affectingGravity:
				continue;
			if affecter == affectee:
				continue;
			acc += accelerationDueToGravity(affecter,affectee)
			
		affectee.setAcceleration(acc)
	
	for b in bodies:
		b.updateVelocity(delta)
		b.updatePosition(delta)
	
	

func accelerationDueToGravity(affecter :CelestialBody,affectee: CelestialBody) -> Vector3:
	var r2 = (affecter.position - affectee.position ).length_squared()
	var dir = (affecter.position - affectee.position).normalized()
	return dir * gravityStrength * affecter.mass /r2
	
