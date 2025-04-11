using Godot;
using System;

public partial class TrueStar : Star
{
	public float mass { get; set; }
	public Vector3 direction { get; set; }

	public TrueStar(Transform3D transform, uint seed, float mass, Vector3 direction, string name = "Star")
		: base(transform, seed, name)
	{
		this.mass = mass;
		this.direction = direction;
	}
}
