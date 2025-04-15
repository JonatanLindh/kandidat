using Godot;
using System;

public partial class PhysicsStar : Star
{
	public float mass { get; private set; }
	public Vector3 velocity { get; set; }

	public PhysicsStar(Transform3D transform, uint seed, float mass, Vector3 velocity, string name = "Star")
		: base(transform, seed, name)
	{
		this.mass = mass;
		this.velocity = velocity;
	}
}
