using Godot;
using System;
using System.Collections.Generic;

public partial class SystemHandler : Node
{
	[Export] PackedScene system;

	List<Node3D> activeSystems = new List<Node3D>();
	
	[Export] public float closeStarRadius { get; private set; } = 10;
	[Export] public float closeStarCullRadius { get; private set; } = 100;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;

	public override void _Ready()
	{

	}

	public Node3D GenerateSystem(Star star)
	{
		if (system == null)
		{
			GD.PrintErr("SystemHandler: No system node set.");
			return null;
		}

		Node3D newSystem = system.Instantiate<Node3D>();
		newSystem.GlobalPosition = star.transform.Origin;
		newSystem.Scale = new Vector3(1, 1, 1) * 0.05f;
		AddChild(newSystem);

		activeSystems.Add(newSystem);

		newSystem.Call("generateSystemFromSeed", star.seed);
		if(debugPrint) GD.Print($"SystemHandler: Generated system at {star.transform.Origin}");
		return newSystem;
	}

	public void CullFarSystems(Vector3 basePos)
	{
		foreach (Node3D system in activeSystems)
		{
			if (system.GlobalPosition.DistanceTo(basePos) > closeStarCullRadius)
			{
				system.QueueFree();
				activeSystems.Remove(system);
				if (debugPrint) GD.Print($"SystemHandler: Culling system at {system.GlobalPosition}");
			}
		}
	}
}
