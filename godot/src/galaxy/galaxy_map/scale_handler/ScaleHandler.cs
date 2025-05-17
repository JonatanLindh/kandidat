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

	[Export] ScalingType scalingType = ScalingType.Linear;

	public override void _Process(double delta)
	{
		activeSystems = systemHandler.GetActiveSystems();
		if (activeSystems.Count > 0)
		{
			UpdateSystemScale();
		}
	}

	private void UpdateSystemScale()
	{
		foreach (Node3D system in activeSystems)
		{
			float distance = player.Position.DistanceTo(system.Position);

			Vector3 newScale = Vector3.One;
			if (scalingType == ScalingType.Linear) newScale = LinearScaling(distance);
			else if (scalingType == ScalingType.Early) newScale = EarlyScaling(distance);

			// Clamps to avoid det==0 errors
			newScale = new Vector3(
				Mathf.Max(newScale.X, Mathf.Epsilon), 
				Mathf.Max(newScale.Y, Mathf.Epsilon), 
				Mathf.Max(newScale.Z, Mathf.Epsilon)
			);

			system.Scale = newScale;
		}
	}

	private Vector3 LinearScaling(float distance)
	{
		float scale = Mathf.Clamp(1 - (distance / systemHandler.closeStarEarlyGenerateRadius), 0, 1);
		return new Vector3(scale, scale, scale) * baseSystemScale;
	}

	private Vector3 EarlyScaling(float distance)
	{
		if (distance <= systemHandler.closeStarLateGenerateRadius)
		{
			return Vector3.One * baseSystemScale;
		}

		float scaleFactor = 1.0f - ((distance - systemHandler.closeStarLateGenerateRadius) /
									(systemHandler.closeStarEarlyGenerateRadius - systemHandler.closeStarLateGenerateRadius));

		float scale = Mathf.Clamp(scaleFactor, 0, 1);
		return new Vector3(scale, scale, scale) * baseSystemScale;
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

enum ScalingType
{
	Linear,
	Early
}
