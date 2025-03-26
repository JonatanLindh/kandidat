using Godot;
using System;

public abstract partial class BenchmarkScene : Node3D
{
	protected Benchmark benchmark;

	// This benchmark duration in seconds
	[Export] float benchmarkDuration = 30.0f;

	// Benchmark downtime after this benchmark is done
	[Export] float downtime = 1.0f;

	float currentTime = 0.0f;
	bool exiting = false;

	public override void _Ready()
	{
		Ready();
	}

	public override void _Process(double delta)
	{
		if(currentTime > benchmarkDuration)
		{
			benchmark.ExitScene(downtime);
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
