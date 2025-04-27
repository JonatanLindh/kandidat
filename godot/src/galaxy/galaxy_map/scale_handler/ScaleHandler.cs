using Godot;
using System;
using System.Collections.Generic;

public partial class ScaleHandler : Node
{
	SystemHandler systemHandler;
	InfiniteGalaxy galaxy;
	Node3D player;

	List<Node3D> activeSystems = new List<Node3D>();
	[Export] float baseSystemScale = 0.2f;

	public override void _Process(double delta)
	{
		activeSystems = systemHandler.GetActiveSystems();
		if (activeSystems.Count > 0)
		{
			UpdateSystemScale();
			UpdateGalaxyScale();
		}
	}

	private void UpdateSystemScale()
	{
		foreach (Node3D system in activeSystems)
		{
			float distance = player.Position.DistanceTo(system.Position);
			float scale = Mathf.Clamp(1 - (distance / systemHandler.closeStarGenerateRadius), 0, 1);
			system.Scale = new Vector3(scale, scale, scale) * baseSystemScale;
		}
	}

	private void UpdateGalaxyScale()
	{
		// TODO?
	}

	public void SetSystemHandler(SystemHandler systemHandler)
	{
		this.systemHandler = systemHandler;
	}

	public void SetGalaxy(InfiniteGalaxy galaxy)
	{
		this.galaxy = galaxy;
	}

	public void SetPlayer(Node3D player)
	{
		this.player = player;
	}
}
