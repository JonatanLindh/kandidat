using Godot;
using System;

public partial class Star
{
	public String name { get; protected set; }
	public Transform3D transform { get; set; }
	public uint seed { get; protected set; }

	public Star(Transform3D transform, uint seed, string name = "Star")
	{
		this.transform = transform;
		this.seed = seed;
		this.name = name;
	}
}
