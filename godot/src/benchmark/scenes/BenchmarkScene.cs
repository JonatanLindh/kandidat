using Godot;
using System;

public abstract partial class BenchmarkScene : Node3D
{
	protected Benchmark benchmark;

	[Export] float benchmarkDuration = 30.0f;
	float currentTime = 0.0f;

	public override void _Ready()
	{
		Ready();
	}

	public override void _Process(double delta)
	{
		if(currentTime > benchmarkDuration)
		{
			benchmark.ExitScene();
			return;
		}

		Process(delta);

		currentTime += (float)delta;
	}

	public void setBenchmark(Benchmark benchmark)
	{
		this.benchmark = benchmark;
	}

	public abstract void Ready();
	public abstract void Process(double delta);
}
