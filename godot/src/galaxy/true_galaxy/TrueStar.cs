using Godot;
using System;

public partial class TrueStar : Star
{
	public float mass { get; private set; }
	public Vector3 velocity { get; set; }

	public TrueStar(Transform3D transform, uint seed, float mass, Vector3 velocity, string name = "Star")
		: base(transform, seed, name)
	{
		this.mass = mass;
		this.velocity = velocity;
	}
}
