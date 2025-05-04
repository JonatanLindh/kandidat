using Godot;
using System;

public partial class PlanetBenchmark : BenchmarkScene
{
	[Export] PlanetMarchingCube planet;

	[Export] Node3D orbitPivot;
	[Export] Node3D planetPivot;
	[Export] Node3D movePivot;

	[Export] float orbitSpeed = 3.0f;
	[Export] float planetSpeed = 10.0f;
	[Export] float moveSpeed = 30.0f;

	public override void BenchmarkReady()
	{

	}

	public override void BenchmarkProcess(double delta)
	{
		orbitPivot.RotateY(Mathf.DegToRad(orbitSpeed * (float)delta));
		planetPivot.RotateZ(Mathf.DegToRad(-planetSpeed * (float)delta));
		movePivot.RotateZ(Mathf.DegToRad(moveSpeed * (float)delta));
	}
}
