using Godot;
using System;

public partial class PlanetBenchmark : BenchmarkScene
{
	[Export] PlanetMarchingCube planet;
	[Export] Camera3D player;

	[Export] float speed = 1500.0f;
	[Export] uint seed = 420;

	public override void BenchmarkReady()
	{
		//planet.
	}

	public override void BenchmarkProcess(double delta)
	{
		//player.Position += Vector3.Forward * speed * (float)delta;
	}
}
