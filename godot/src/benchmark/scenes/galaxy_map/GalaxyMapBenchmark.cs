using Godot;
using System;

public partial class GalaxyMapBenchmark : BenchmarkScene
{
	[Export] GalaxyMap galaxyMap;
	[Export] Camera3D player;

	[ExportCategory("CHANGES INVALIDATE OLD BENCHMARKS")]
	[Export] float speed = 10000.0f;
	[Export] uint seed = 1234;

	float currentTime = 0.0f;

	public override void Ready()
	{
		InfiniteGalaxy galaxy = galaxyMap.GetNode<InfiniteGalaxy>("InfiniteGalaxy");
		galaxy.Set("player", player);
		galaxy.Set("seed", seed);
	}

	public override void Process(double delta)
	{
		player.Position += Vector3.Forward * speed * (float)delta;
		currentTime += (float)delta;
	}
}
