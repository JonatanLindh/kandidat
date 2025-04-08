using Godot;
using System;
using System.Transactions;

public partial class SystemBenchmark : BenchmarkScene
{
	[Export] Node3D system;
	[Export] Camera3D player;

	[Export] float speed = 60.0f;
	[Export] uint seed = 420;

	public override void BenchmarkReady()
	{
		system.Call("generateSystemFromSeed", seed);
	}

	public override void BenchmarkProcess(double delta)
	{
		player.Position += Vector3.Forward * speed * (float)delta;
	}
}
