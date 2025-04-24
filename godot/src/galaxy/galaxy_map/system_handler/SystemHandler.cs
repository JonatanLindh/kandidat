using Godot;
using System;

public partial class SystemHandler : Node
{
	[Export] PackedScene system;

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

		newSystem.Call("generateSystemFromSeed", star.seed);
		if(debugPrint) GD.Print($"SystemHandler: Generated system at {star.transform.Origin}");
		return newSystem;
	}
}
