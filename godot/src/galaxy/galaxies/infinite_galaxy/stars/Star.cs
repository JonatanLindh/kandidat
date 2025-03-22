using Godot;
using System;

public partial class Star
{
	public String name { get; private set; }
	public Transform3D transform { get; private set; }
	public uint seed { get; private set; }

	public Star(Transform3D transform, uint seed, string name = "Star")
	{
		this.name = name;
		this.transform = transform;
		this.seed = seed;
	}
}
