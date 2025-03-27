using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public partial class GraphPlotter : Control
{
	Line2D fpsLine;
	Line2D frameTimeLine;
	Line2D memoryUsageLine;

	Panel fpsPanel;
	Panel frameTimePanel;
	Panel memoryUsagePanel;
	float panelHeight;

	float currentTime = 0.0f;

	public override void _Ready()
	{
		fpsLine = GetNode<Line2D>("%FPSLine2D");
		frameTimeLine = GetNode<Line2D>("%FrametimeLine2D");
		memoryUsageLine = GetNode<Line2D>("%MemoryLine2D");

		fpsPanel = GetNode<Panel>("%FPSPanel");
		frameTimePanel = GetNode<Panel>("%FrametimePanel");
		memoryUsagePanel = GetNode<Panel>("%MemoryPanel");
	}

	public override void _Process(double delta)
	{
		panelHeight = fpsPanel.GetRect().Size.Y;
		
		currentTime += (float)delta;
	}

	private float maxFps = 60.0f; // Initial estimate, updated dynamically
	private List<Vector2> fpsPoints = new List<Vector2>(); // Store points to rescale
	private float maxfpspadding = 30.0f; // Padding for max FPS

	public void AddDataPoint(BenchmarkDatapoint data)
	{
		// Update max dynamically if a new value is higher
		if (data.fps > maxFps)
		{
			maxFps = data.fps + maxfpspadding;
			RescaleGraph();
		}

		float normalizedFps = data.fps / maxFps;
		float yPosition = panelHeight * (1.0f - normalizedFps);
		Vector2 point = new Vector2(currentTime * 10, yPosition);
		fpsPoints.Add(point);
		fpsLine.AddPoint(point);
	}

	private void RescaleGraph()
	{
		for (int i = 0; i < fpsPoints.Count; i++)
		{
			float normalizedFps = (fpsPoints[i].Y / panelHeight) * maxFps; // Convert back to FPS
			float newY = panelHeight * (1.0f - (normalizedFps / maxFps));  // Rescale
			fpsPoints[i] = new Vector2(fpsPoints[i].X, newY);
		}

		fpsLine.ClearPoints();
		foreach (var point in fpsPoints)
		{
			fpsLine.AddPoint(point);
		}
	}
}
