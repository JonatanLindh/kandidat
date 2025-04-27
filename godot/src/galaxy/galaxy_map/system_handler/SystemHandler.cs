using Godot;
using System;
using System.Collections.Generic;

public partial class SystemHandler : Node
{
	[Export] PackedScene systemScene;

	List<Node3D> activeSystems = new List<Node3D>();

	[Export] public float closeStarGenerateRadius { get; private set; } = 100;

	// Must be greater than closeStarGenerateRadius. Will otherwise be clamped to closeStarGenerateRadius.
	[Export] public float closeStarCullRadius { get; private set; } = 1000;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;

	public override void _Ready()
	{
		if(closeStarCullRadius < closeStarGenerateRadius)
		{
			GD.PrintErr($"SystemHandler: closeStarCullRadius must be greater than closeStarGenerateRadius. Setting closeStarCullRadius to {closeStarGenerateRadius} (closeStarGenerateRadius) instead.");
			closeStarCullRadius = closeStarGenerateRadius;
		}
	}

	public Node3D GenerateSystem(Star star)
	{
		if (systemScene == null)
		{
			GD.PrintErr("SystemHandler: No system node set.");
			return null;
		}

		// Instantiate the system
		Node3D newSystem = systemScene.Instantiate<Node3D>();
		newSystem.Position = star.transform.Origin;
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
			if (system.Position.DistanceTo(basePos) > closeStarCullRadius)
			{
				system.QueueFree();
				activeSystems.Remove(system);
				if (debugPrint) GD.Print($"SystemHandler: Culled system at {system.GlobalPosition}");
			}
		}
	}

	public bool SystemExists(Vector3 position)
	{
		foreach (Node3D system in activeSystems)
		{
			if (system.Position == position)
			{
				return true;
			}
		}

		return false;
	}

	public Node3D GetClosestSystem(Vector3 position)
	{
		Node3D closestSystem = null;
		float closestDistance = float.MaxValue;

		foreach (Node3D system in activeSystems)
		{
			float distance = system.Position.DistanceTo(position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestSystem = system;
			}
		}

		return closestSystem;
	}

	public List<Node3D> GetActiveSystems()
	{
		return activeSystems;
	}
}
