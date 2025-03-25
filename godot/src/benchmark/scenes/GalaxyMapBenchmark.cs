using Godot;
using System;

public partial class GalaxyMapBenchmark : BenchmarkScene
{
	[Export] GalaxyMap galaxyMap;
	[Export] Camera3D player;

	[Export] float speed = 10000.0f;
	[Export] float duration = 2.0f;
	float currentTime = 0.0f;

	public override void _Ready()
	{
		InfiniteGalaxy galaxy = galaxyMap.GetNode<InfiniteGalaxy>("InfiniteGalaxy");
		galaxy.Set("player", player);
	}

	public override void _Process(double delta)
	{
		if(currentTime >= duration)
		{
			benchmark.ExitScene();
		}

		player.Position += Vector3.Forward * speed * (float)delta;
		currentTime += (float)delta;
	}
}
