using Godot;
using System;

public partial class GalaxyMapBenchmark : BenchmarkScene
{
	[Export] GalaxyMap galaxyMap;
	[Export] Camera3D player;

	[Export] float speed = 1500.0f;
	[Export] uint seed = 1234;

	float currentTime = 0.0f;

	public override void BenchmarkReady()
	{
		InfiniteGalaxy galaxy = galaxyMap.GetNode<InfiniteGalaxy>("InfiniteGalaxy");
		galaxy.Set("player", player);
		galaxy.Set("seed", seed);
	}

	public override void BenchmarkProcess(double delta)
	{
		player.Position += Vector3.Forward * speed * (float)delta;
		currentTime += (float)delta;
	}
}
