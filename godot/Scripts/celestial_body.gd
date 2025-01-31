extends Node3D
class_name CelestialBody

@export var mass: float = 1
@export var affectingGravity: bool = false
@export var affectedByGravity: bool = false
@export var velocity: Vector3 = Vector3.ZERO
@export var acceleration: Vector3 = Vector3.ZERO

func setAcceleration(acc: Vector3):
	acceleration = acc

func updateVelocity(dt: float):
	velocity += acceleration * dt

func updatePosition(dt: float):
	position += velocity * dt
