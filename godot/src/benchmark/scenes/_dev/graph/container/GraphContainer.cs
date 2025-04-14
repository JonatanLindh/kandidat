using Godot;
using System;
using System.Collections.Generic;

public partial class GraphContainer : VBoxContainer
{
	[Export] BenchmarkDatapointEnum dataType = BenchmarkDatapointEnum.FPS;

	Label title;
	Line2D line;
	Panel panel;
	Label currentLabel;
	GraphYAxis yAxis;

	// Time scale for the graph, to exaggerate the time axis
	[Export] float timeScale = 30;

	// Amount to shift graphs to the left when they exceed the panel width
	// E.g 0.5f removes 50% of the points.
	[Export] float graphShiftFactor = 0.5f;

	int currentLabelDecimalCount;
	float padding;

	[ExportGroup("Y-axis")]
	[Export] int yAxisCount = 4;
	[Export] PackedScene yAxisLabel;
	[Export] PackedScene yAxisHSeparator;

	float panelWidth;
	float panelHeight;
	float currentTime = 0.0f;

	float maxValue = 0.0f;
	float minValue = 0.0f;
	bool minValueSet = false;

	List<Vector2> dataPoints = new List<Vector2>();

	public override void _Ready()
	{
		title = GetNode<Label>("%Title");
		line = GetNode<Line2D>("%Line2D");
		panel = GetNode<Panel>("%Panel");
		currentLabel = GetNode<Label>("%Label");
		yAxis = new GraphYAxis(GetNode<VBoxContainer>("%YAxis"), panel, yAxisLabel, yAxisHSeparator, yAxisCount);

		// Sets initial panel width and height
		panelWidth = panel.GetRect().Size.X;
		panelHeight = panel.GetRect().Size.Y;

		// Set the title text
		switch (dataType)
		{
			case BenchmarkDatapointEnum.FPS:
				title.Text = "Frames per second (FPS)";
				padding = 30.0f;
				currentLabelDecimalCount = 0;
				break;
			case BenchmarkDatapointEnum.FrameTime:
				title.Text = "Frame time (ms)";
				padding = 5.0f;
				currentLabelDecimalCount = 4;
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				title.Text = "Memory usage (MB)";
				padding = 50.0f;
				currentLabelDecimalCount = 2;
				break;
		}
	}

	public override void _Process(double delta)
	{
		currentTime += (float)delta;
	}

	/// <summary>
	/// Processes a data point and updates the graph accordingly.
	/// </summary>
	/// <param name="value"></param>
	public void AddDataPoint(float value)
	{
		// Set initial min values
		if (!minValueSet)
		{
			minValueSet = true;
			minValue = value;
		}

		// Update max dynamically if a new value is higher
		if (value > maxValue)
		{
			maxValue = value + padding;
			RescaleGraph();
			yAxis.RedrawYAxis(maxValue);
		}

		if (value < minValue)
		{
			minValue = value;
		}

		float x = currentTime * timeScale;
		float y = GetPanelPositionOfValue(value);
		Vector2 newPoint = new Vector2(x, y);

		// Add point to list and line
		dataPoints.Add(newPoint);
		line.AddPoint(newPoint);

		// Check if the graph exceeds the panel width
		if (dataPoints.Count > 0 && newPoint.X >= panelWidth)
		{
			ShiftGraphLeft();
		}

		// Update the current label with the latest value
		currentLabel.Text = Math.Round(value, currentLabelDecimalCount).ToString();
	}

	/// <summary>
	/// Rescales the graph points based on the maximum value.
	/// </summary>
	private void RescaleGraph()
	{
		// Rescale points
		for (int i = 0; i < dataPoints.Count; i++)
		{
			float nonNormalizedValue = (dataPoints[i].Y / panelHeight) * maxValue;
			float newY = GetPanelPositionOfValue(nonNormalizedValue);
			dataPoints[i] = new Vector2(dataPoints[i].X, newY);
		}

		// Clear and redraw line
		RedrawGraph();
	}

	/// <summary>
	/// Redraws the graph with the given points.
	/// </summary>
	private void RedrawGraph()
	{
		line.ClearPoints();
		foreach (var point in dataPoints)
		{
			line.AddPoint(point);
		}
	}

	/// <summary>
	/// Calculates the panel position of a value based on the maximum value.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	private float GetPanelPositionOfValue(float value)
	{
		float nValue = value / maxValue;
		return panelHeight * (1.0f - nValue);
	}

	/// <summary>
	/// Shifts all graphs to the left by removing points that are out of bounds.
	/// </summary>
	private void ShiftGraphLeft()
	{
		int removeCount = (int)Mathf.Round(dataPoints.Count * graphShiftFactor);
		if (removeCount == 0) return;

		float shiftAmount = dataPoints[removeCount].X;

		// Adjust `currentTime` to prevent drifting
		currentTime -= shiftAmount / timeScale;

		// Remove points
		dataPoints.RemoveRange(0, removeCount);
		for (int i = 0; i < dataPoints.Count; i++)
		{
			dataPoints[i] = new Vector2(dataPoints[i].X - shiftAmount, dataPoints[i].Y);
		}

		// Remove points that are out of bounds (left)
		dataPoints.RemoveAll(point => point.X < 0);

		RedrawGraph();
	}

	/// <summary>
	/// Signal handler for when the FPS panel is resized.
	/// Updates the panel width and height variables.
	/// </summary>
	public void OnPanelResized()
	{
		panelWidth = panel.GetRect().Size.X;
		panelHeight = panel.GetRect().Size.Y;
	}

	/// <summary>
	/// Returns the data type
	/// </summary>
	/// <returns></returns>
	public BenchmarkDatapointEnum GetDataType()
	{
		return dataType;
	}
}
