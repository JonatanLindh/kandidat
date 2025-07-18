using Godot;
using System;
using System.Collections.Generic;

public partial class SystemHandler : Node
{
	[Export] PackedScene systemScene;

	List<Node3D> activeSystems = new List<Node3D>();

	Node hudSignalBus;
	[Export] PackedScene customPhysicsBody;

	// Must be greater than closeStarLateGenerateRadius. Will otherwise be clamped to closeStarLateGenerateRadius.
	[Export] public float closeStarEarlyGenerateRadius { get; private set; } = 200;

	[Export] public float closeStarLateGenerateRadius { get; private set; } = 150;

	// Must be greater than closeStarGenerateRadius. Will otherwise be clamped to closeStarGenerateRadius.
	[Export] public float closeStarCullRadius { get; private set; } = 300;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;

	public override void _Ready()
	{
		if(closeStarCullRadius < closeStarEarlyGenerateRadius)
		{
			GD.PrintErr($"SystemHandler: closeStarCullRadius must be greater than closeStarEarlyGenerateRadius. Setting closeStarCullRadius to {closeStarEarlyGenerateRadius} (closeStarEarlyGenerateRadius) instead.");
		}

		if (closeStarEarlyGenerateRadius < closeStarLateGenerateRadius)
		{
			GD.PrintErr($"SystemHandler: closeStarEarlyGenerateRadius must be greater than closeStarLateGenerateRadius. Setting closeStarEarlyGenerateRadius to {closeStarLateGenerateRadius} (closeStarLateGenerateRadius) instead.");
			closeStarEarlyGenerateRadius = closeStarLateGenerateRadius;
		}

		hudSignalBus = GetNode<Node>("/root/HudSignalBus");
		hudSignalBus.Connect("add_physics_body", new Callable(this, nameof(OnAddPhysicsBody)));
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
		newSystem.Scale = Vector3.One * Mathf.Epsilon;
		AddChild(newSystem);

		activeSystems.Add(newSystem);

		newSystem.Call("generateSystemFromSeed", star.seed);
		if(debugPrint) GD.Print($"SystemHandler: Generated system at {star.transform.Origin}");
		return newSystem;
	}

	public void CullFarSystems(Vector3 basePos)
	{
		for(int i = activeSystems.Count - 1; i >= 0; i--)
		{
			Node3D system = activeSystems[i];
			if (system.Position.DistanceTo(basePos) > closeStarCullRadius)
			{
				system.QueueFree();
				activeSystems.RemoveAt(i);
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

	private void OnAddPhysicsBody(float mass, Vector3 velocity, Vector3 position)
	{
		if(activeSystems.Count == 0)
		{
			GD.PrintErr("SystemHandler: No systems to add physics body to.");
			return;
		}

		Node3D gravityController = null;
		foreach (Node3D child in activeSystems[0].GetChildren())
		{
			if (child.Name == "GravityController")
			{
				gravityController = child;
			}
		}

		Node3D newBody = customPhysicsBody.Instantiate<Node3D>();

		newBody.Call("set_test_body_mass", mass);
		newBody.Call("set_test_body_velocity", velocity);
		gravityController.AddChild(newBody);
		newBody.GlobalPosition = position;
	}
}
