using Godot;
using System;

public partial class GalaxyMapBenchmark : BenchmarkScene
{
	[Export] GalaxyMap galaxyMap;
	[Export] Camera3D player;

	[Export] float speed = 1500.0f;
	[Export] uint seed = 1234;

	public override void BenchmarkReady()
	{
		galaxyMap.SetPlayer(player);

		//galaxy.Set("player", player);
		InfiniteGalaxy galaxy = galaxyMap.GetNode<InfiniteGalaxy>("InfiniteGalaxy");
		galaxy.Set("seed", seed);
	}

	public override void BenchmarkProcess(double delta)
	{
		player.Position += Vector3.Forward * speed * (float)delta;
	}
}
