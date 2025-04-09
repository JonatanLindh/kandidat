using Godot;
using System;

public partial class PlanetBenchmark : BenchmarkScene
{
	[Export] Node3D orbitPivot;
	[Export] Node3D planetPivot;

	[Export] float orbitSpeed = 5.0f;
	[Export] float planetSpeed = 15.0f;
	[Export] uint seed = 420;

	public override void BenchmarkReady()
	{

	}

	public override void BenchmarkProcess(double delta)
	{
		orbitPivot.RotateY(Mathf.DegToRad(orbitSpeed * (float)delta));
		planetPivot.RotateZ(Mathf.DegToRad(-planetSpeed * (float)delta));
	}
}
