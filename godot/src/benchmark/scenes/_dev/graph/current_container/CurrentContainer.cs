using Godot;
using System;

public partial class CurrentContainer : VBoxContainer
{
	Label FPSLabel;
	Label frameTimeLabel;
	Label memoryLabel;

	public override void _Ready()
	{
		FPSLabel = GetNode<Label>("%FPSLabel");
		frameTimeLabel = GetNode<Label>("%FrametimeLabel");
		memoryLabel = GetNode<Label>("%MemoryLabel");
	}

	public void UpdateCurrentData(BenchmarkDatapoint data)
	{
		FPSLabel.Text = data.fps.ToString();
		frameTimeLabel.Text = Math.Round(data.frameTime * 1000, 4).ToString();
		memoryLabel.Text = Math.Round(data.memoryUsage / (1024 * 1024), 2).ToString();
	}
}
