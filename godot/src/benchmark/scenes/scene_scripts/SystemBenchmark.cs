using Godot;
using System;
using System.Transactions;

public partial class SystemBenchmark : BenchmarkScene
{
	[Export] Node3D system;
	[Export] Node3D pivot;

	[Export] float speed = 30.0f;
	[Export] uint seed = 420;

	public override void BenchmarkReady()
	{
		system.Call("generateSystemFromSeed", seed);
	}

	public override void BenchmarkProcess(double delta)
	{
		pivot.RotateY(Mathf.DegToRad(speed * (float)delta));
	}
}
