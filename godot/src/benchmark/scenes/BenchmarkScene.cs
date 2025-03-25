using Godot;
using System;

public partial class BenchmarkScene : Node3D
{
	protected Benchmark benchmark;

	public void setBenchmark(Benchmark benchmark)
	{
		this.benchmark = benchmark;
	}
}
