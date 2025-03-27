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

	float maxFps;
	float maxFrameTime;
	float maxMemoryUsage;

	// Store points to allow for rescaling
	List<Vector2> fpsPoints = new List<Vector2>();
	List<Vector2> frameTimePoints = new List<Vector2>();
	List<Vector2> memoryUsagePoints = new List<Vector2>();

	// Padding of max for when rescaling graphs
	float maxFpsPadding = 30.0f;
	float maxFrametimePadding = 0.01f;
	float maxMemoryUsagePadding = 50.0f;

	public void AddDataPoint(BenchmarkDatapoint data)
	{
		ProcessDataPoint(BenchmarkDatapointEnum.FPS, data.fps, ref maxFps, maxFpsPadding, fpsPoints, fpsLine);
		ProcessDataPoint(BenchmarkDatapointEnum.FrameTime, data.frameTime * 100, ref maxFrameTime, maxFrametimePadding, frameTimePoints, frameTimeLine);
		ProcessDataPoint(BenchmarkDatapointEnum.MemoryUsage, data.memoryUsage / (1024 * 1024), ref maxMemoryUsage, maxMemoryUsagePadding, memoryUsagePoints, memoryUsageLine);
	}

	private void ProcessDataPoint(BenchmarkDatapointEnum type, float value, ref float maxValue, float padding, List<Vector2> pointsList, Line2D line)
	{
		// Update max dynamically if a new value is higher
		if (value > maxValue)
		{
			maxValue = value + padding;
			RescaleGraph(type);
		}

		// Get y position of value
		float yPosition = GetPanelPositionOfValue(value, maxValue);
		Vector2 newPoint = new Vector2(currentTime * 10, yPosition);

		// Add point to list and line
		pointsList.Add(newPoint);
		line.AddPoint(newPoint);
	}

	private void RescaleGraph(BenchmarkDatapointEnum datapointType)
	{
		switch (datapointType)
		{
			case BenchmarkDatapointEnum.FPS:
				RescaleGraph(fpsPoints, fpsLine, maxFps);
				break;
			case BenchmarkDatapointEnum.FrameTime:
				RescaleGraph(frameTimePoints, frameTimeLine, maxFrameTime);
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				RescaleGraph(memoryUsagePoints, memoryUsageLine, maxMemoryUsage);
				break;
		}
	}

	private void RescaleGraph(List<Vector2> points, Line2D line, float maxValue)
	{
		// Rescale points
		for (int i = 0; i < points.Count; i++)
		{
			float nonNormalizedValue = (points[i].Y / panelHeight) * maxValue;
			float newY = GetPanelPositionOfValue(nonNormalizedValue, maxValue);
			points[i] = new Vector2(points[i].X, newY);
		}

		// Clear and redraw line
		line.ClearPoints();
		foreach (var point in points)
		{
			line.AddPoint(point);
		}
	}

	private float GetPanelPositionOfValue(float value, float maxValue)
	{
		float nValue = value / maxValue;
		return panelHeight * (1.0f - nValue);
	}
}
