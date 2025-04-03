using Godot;
using System;
using System.Collections.Generic;

public partial class GraphPlotter : Control
{
	Line2D fpsLine;
	Line2D frameTimeLine;
	Line2D memoryUsageLine;

	Panel fpsPanel;
	Panel frameTimePanel;
	Panel memoryUsagePanel;

	Label fpsLabel;
	Label frameTimeLabel;
	Label memoryUsageLabel;

	[Export] float timeScale = 10;

	[ExportGroup("Y-axis")]
	[Export] int yAxisCount = 4;
	[Export] PackedScene yAxisLabel;
	[Export] PackedScene yAxisHSeparator;

	GraphYAxis fpsYAxis;
	GraphYAxis frametimeYAxis;
	GraphYAxis memoryYAxis;

	float panelWidth;
	float panelHeight;
	float currentTime = 0.0f;

	float maxFps;
	float maxFrameTime;
	float maxMemoryUsage;

	bool minValuesSet = false;
	float minFps;
	float minFrameTime;
	float minMemoryUsage;

	float currentFps;
	float currentFrameTime;
	float currentMemoryUsage;

	int graphShiftFactor = 3;

	// Store points to allow for rescaling
	List<Vector2> fpsPoints = new List<Vector2>();
	List<Vector2> frameTimePoints = new List<Vector2>();
	List<Vector2> memoryUsagePoints = new List<Vector2>();

	// Padding of max for when rescaling graphs
	float maxFpsPadding = 30.0f;
	float maxFrametimePadding = 0.01f;
	float maxMemoryUsagePadding = 50.0f;

	public override void _Ready()
	{
		fpsLine = GetNode<Line2D>("%FPSLine2D");
		frameTimeLine = GetNode<Line2D>("%FrametimeLine2D");
		memoryUsageLine = GetNode<Line2D>("%MemoryLine2D");

		fpsPanel = GetNode<Panel>("%FPSPanel");
		frameTimePanel = GetNode<Panel>("%FrametimePanel");
		memoryUsagePanel = GetNode<Panel>("%MemoryPanel");

		fpsLabel = GetNode<Label>("%FPSLabel");
		frameTimeLabel = GetNode<Label>("%FrametimeLabel");
		memoryUsageLabel = GetNode<Label>("%MemoryLabel");

		fpsYAxis = new GraphYAxis(GetNode<VBoxContainer>("%FPSYAxis"), fpsPanel, yAxisLabel, yAxisHSeparator, yAxisCount);
		frametimeYAxis = new GraphYAxis(GetNode<VBoxContainer>("%FrametimeYAxis"), frameTimePanel, yAxisLabel, yAxisHSeparator, yAxisCount);
		memoryYAxis = new GraphYAxis(GetNode<VBoxContainer>("%MemoryYAxis"), memoryUsagePanel, yAxisLabel, yAxisHSeparator, yAxisCount);

		// Set initial panel width and height
		panelWidth = fpsPanel.GetRect().Size.X;
		panelHeight = fpsPanel.GetRect().Size.Y;
	}

	public override void _Process(double delta)
	{
		currentTime += (float)delta;
	}

	/// <summary>
	/// Adds a data point to the graph for FPS, frame time, and memory usage.
	/// </summary>
	/// <param name="data"></param>
	public void AddDataPoint(BenchmarkDatapoint data)
	{
		currentFps = data.fps;
		currentFrameTime = data.frameTime;
		currentMemoryUsage = data.memoryUsage;

		// Set initial min values
		if (!minValuesSet)
		{
			minValuesSet = true;
			minFps = currentFps;
			minFrameTime = currentFrameTime;
			minMemoryUsage = currentMemoryUsage;
		}

		float filteredFrameTime = currentFrameTime * 100; // seconds to ms
		float filteredMemoryUsage = currentMemoryUsage / (1024 * 1024); // bytes to MB

		fpsLabel.Text = currentFps.ToString();
		frameTimeLabel.Text = Math.Round(filteredFrameTime, 4).ToString();
		memoryUsageLabel.Text = Math.Round(filteredMemoryUsage, 2).ToString();

		ProcessDataPoint(BenchmarkDatapointEnum.FPS, currentFps, ref maxFps, ref minFps, maxFpsPadding, fpsPoints, fpsLine);
		ProcessDataPoint(BenchmarkDatapointEnum.FrameTime, filteredFrameTime, ref maxFrameTime, ref minFrameTime, maxFrametimePadding, frameTimePoints, frameTimeLine);
		ProcessDataPoint(BenchmarkDatapointEnum.MemoryUsage, filteredMemoryUsage, ref maxMemoryUsage, ref minMemoryUsage, maxMemoryUsagePadding, memoryUsagePoints, memoryUsageLine);
	}

	/// <summary>
	/// Processes a data point for the specified type and updates its graph accordingly.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	/// <param name="maxValue"></param>
	/// <param name="minValue"></param>
	/// <param name="padding"></param>
	/// <param name="pointsList"></param>
	/// <param name="line"></param>
	private void ProcessDataPoint(BenchmarkDatapointEnum type, float value, ref float maxValue, ref float minValue, float padding, List<Vector2> pointsList, Line2D line)
	{
		// Update max dynamically if a new value is higher
		if (value > maxValue)
		{
			maxValue = value + padding;
			RescaleGraph(type);
			RedrawYAxis(type, maxValue);
		}

		if(value < minValue)
		{
			minValue = value;
		}

		float x = currentTime * timeScale;
		float y = GetPanelPositionOfValue(value, maxValue);
		Vector2 newPoint = new Vector2(x, y);

		// Add point to list and line
		pointsList.Add(newPoint);
		line.AddPoint(newPoint);

		// Check if the graph exceeds the panel width
		if (pointsList.Count > 0 && newPoint.X >= panelWidth)
		{
			ShiftAllGraphsLeft();
		}
	}

	/// <summary>
	/// Redraws the Y-axis labels and separators based on the maximum value of the specified type.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="maxValue"></param>
	private void RedrawYAxis(BenchmarkDatapointEnum type, float maxValue)
	{
		switch (type)
		{
			case BenchmarkDatapointEnum.FPS:
				fpsYAxis.RedrawYAxis(maxValue);
				break;
			case BenchmarkDatapointEnum.FrameTime:
				frametimeYAxis.RedrawYAxis(maxValue);
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				memoryYAxis.RedrawYAxis(maxValue);
				break;
		}
	}

	/// <summary>
	/// Rescales the graph based on the maximum value of the specified type.
	/// </summary>
	/// <param name="datapointType"></param>
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

	/// <summary>
	/// Rescales the graph points based on the maximum value.
	/// </summary>
	/// <param name="points"></param>
	/// <param name="line"></param>
	/// <param name="maxValue"></param>
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
		RedrawGraph(points, line);
	}

	/// <summary>
	/// Redraws the graph with the given points.
	/// </summary>
	/// <param name="points"></param>
	/// <param name="line"></param>
	private void RedrawGraph(List<Vector2> points, Line2D line)
	{
		line.ClearPoints();
		foreach (var point in points)
		{
			line.AddPoint(point);
		}
	}

	/// <summary>
	/// Calculates the panel position of a value based on the maximum value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="maxValue"></param>
	/// <returns></returns>
	private float GetPanelPositionOfValue(float value, float maxValue)
	{
		float nValue = value / maxValue;
		return panelHeight * (1.0f - nValue);
	}

	/// <summary>
	/// Shifts all graphs to the left by removing points that are out of bounds.
	/// </summary>
	private void ShiftAllGraphsLeft()
	{
		int removeCount = fpsPoints.Count / graphShiftFactor;
		if (removeCount == 0) return;

		float shiftAmount = fpsPoints[removeCount].X;

		// Adjust `currentTime` to prevent drifting
		currentTime -= shiftAmount / timeScale;

		// Apply shift to all datasets
		ShiftGraphLeft(fpsPoints, fpsLine, shiftAmount);
		ShiftGraphLeft(frameTimePoints, frameTimeLine, shiftAmount);
		ShiftGraphLeft(memoryUsagePoints, memoryUsageLine, shiftAmount);
	}

	/// <summary>
	/// Shifts the specified graph to the left by removing points that are out of bounds.
	/// </summary>
	/// <param name="pointsList"></param>
	/// <param name="line"></param>
	/// <param name="shiftAmount"></param>
	private void ShiftGraphLeft(List<Vector2> pointsList, Line2D line, float shiftAmount)
	{
		pointsList.RemoveRange(0, pointsList.Count / 4);

		for (int i = 0; i < pointsList.Count; i++)
		{
			pointsList[i] = new Vector2(pointsList[i].X - shiftAmount, pointsList[i].Y);
		}

		// Remove points that are out of bounds (left)
		pointsList.RemoveAll(point => point.X < 0);

		RedrawGraph(pointsList, line);
	}

	/// <summary>
	/// Signal handler for when the FPS panel is resized.
	/// Updates the panel width and height variables.
	/// </summary>
	public void OnFpsPanelResized()
	{
		panelWidth = fpsPanel.GetRect().Size.X;
		panelHeight = fpsPanel.GetRect().Size.Y;
	}
}
