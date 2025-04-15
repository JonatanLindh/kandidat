using Godot;
using System;

public abstract partial class BenchmarkScene : Node3D
{
	private Benchmark benchmark;

	// The benchmarks runtime in seconds.
	// If useBenchmarkDuration is false the benchmark will run indefinitely.
	// If so, make sure to call ExitScene() when you're done.
	[Export] bool useBenchmarkDuration = true;
	[Export] float benchmarkDuration = 30.0f;

	// Benchmark downtime after this benchmark is done
	[Export] float downtime = 1.0f;

	float currentTime = 0.0f;

	public override void _Ready()
	{
		BenchmarkReady();
	}

	public override void _Process(double delta)
	{
		if(currentTime > benchmarkDuration && useBenchmarkDuration)
		{
			benchmark.ExitScene(downtime);
			return;
		}

		BenchmarkProcess(delta);

		currentTime += (float)delta;
	}

	public void setBenchmark(Benchmark benchmark)
	{
		this.benchmark = benchmark;
	}

	protected void ExitScene()
	{
		if(!useBenchmarkDuration)
		{
			benchmark.ExitScene(downtime);
		}

		else
		{
			GD.PrintErr("Benchmark duration is enabled; do not call ExitScene() manually");
		}
	}

	public abstract void BenchmarkReady();
	public abstract void BenchmarkProcess(double delta);
}
