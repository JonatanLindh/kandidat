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

	Label fps1Label;
	Label fps0Label;

	Label frametime1Label;
	Label frametime0Label;

	Label memory1Label;
	Label memory0Label;

	VBoxContainer fpsYaxis;
	VBoxContainer frametimeYaxis;
	VBoxContainer memoryYaxis;

	int yAxisCount = 4;
	Label[] fpsYAxisLabels;
	Label[] frametimeYAxisLabels;
	Label[] memoryYAxisLabels;
	HSeparator[] fpsYAxisSeparators;
	HSeparator[] frametimeYAxisSeparators;
	HSeparator[] memoryYAxisSeparators;

	[Export] PackedScene yAxisLabel;
	[Export] PackedScene yAxisHSeparator;

	float panelWidth;
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

		fpsLabel = GetNode<Label>("%FPSLabel");
		frameTimeLabel = GetNode<Label>("%FrametimeLabel");
		memoryUsageLabel = GetNode<Label>("%MemoryLabel");

		fps1Label = GetNode<Label>("%1FPS");
		fps0Label = GetNode<Label>("%0FPS");

		frametime1Label = GetNode<Label>("%1Frametime");
		frametime0Label = GetNode<Label>("%0Frametime");

		memory1Label = GetNode<Label>("%1Memory");
		memory0Label = GetNode<Label>("%0Memory");

		fpsYaxis = GetNode<VBoxContainer>("%FPSYAxis");
		frametimeYaxis = GetNode<VBoxContainer>("%FrametimeYAxis");
		memoryYaxis = GetNode<VBoxContainer>("%MemoryYAxis");

		fpsYAxisLabels = new Label[yAxisCount];
		frametimeYAxisLabels = new Label[yAxisCount];
		memoryYAxisLabels = new Label[yAxisCount];
		fpsYAxisSeparators = new HSeparator[yAxisCount];
		frametimeYAxisSeparators = new HSeparator[yAxisCount];
		memoryYAxisSeparators = new HSeparator[yAxisCount];
		InitYAxis();
	}

	public override void _Process(double delta)
	{
		panelWidth = fpsPanel.GetRect().Size.X;
		panelHeight = fpsPanel.GetRect().Size.Y;
		currentTime += (float)delta;
	}

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

	private void ProcessDataPoint(BenchmarkDatapointEnum type, float value, ref float maxValue, ref float minValue, float padding, List<Vector2> pointsList, Line2D line)
	{
		// Update max dynamically if a new value is higher
		if (value > maxValue)
		{
			maxValue = value + padding;
			RescaleGraph(type);
			RedrawYAxis(type);

			UpdateMaxMinLabels();
		}

		if(value < minValue)
		{
			minValue = value;
			UpdateMaxMinLabels();
		}

		float x = currentTime * 10;
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
		RedrawGraph(points, line);
	}

	private void RedrawYAxis(BenchmarkDatapointEnum datapointType)
	{
		Panel panel = fpsPanel;
		VBoxContainer yAxis = fpsYaxis;
		float maxVal = 0.0f;

		Label[] labels = fpsYAxisLabels;
		HSeparator[] separators = fpsYAxisSeparators;

		float[] labelYPositions = new float[yAxisCount];

		switch (datapointType)
		{
			case BenchmarkDatapointEnum.FPS:
				panel = fpsPanel;
				yAxis = fpsYaxis;
				maxVal = maxFps;
				labels = fpsYAxisLabels;
				separators = fpsYAxisSeparators;
				break;
			case BenchmarkDatapointEnum.FrameTime:
				panel = frameTimePanel;
				yAxis = frametimeYaxis;
				maxVal = maxFrameTime;
				labels = frametimeYAxisLabels;
				separators = frametimeYAxisSeparators;
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				panel = memoryUsagePanel;
				yAxis = memoryYaxis;
				maxVal = maxMemoryUsage;
				labels = memoryYAxisLabels;
				separators = memoryYAxisSeparators;
				break;
		}

		for(int i = 0; i < yAxisCount; i++)
		{
			Label cL = labels[i];
			HSeparator cS = separators[i];
			cL.Text = Math.Round(maxVal - (maxVal / yAxisCount * i), 0).ToString();

			float y = cL.Position.Y + cL.Size.Y / yAxisCount;
			cS.Position = new Vector2(0, y);
		}
	}

	private void InitYAxis()
	{
		for (int i = 0; i < yAxisCount; i++)
		{
			Label labelFps = yAxisLabel.Instantiate<Label>();
			fpsYaxis.AddChild(labelFps);
			fpsYAxisLabels[i] = labelFps;

			Label labelFrametime = yAxisLabel.Instantiate<Label>();
			frametimeYaxis.AddChild(labelFrametime);
			frametimeYAxisLabels[i] = labelFrametime;

			Label labelMemory = yAxisLabel.Instantiate<Label>();
			memoryYaxis.AddChild(labelMemory);
			memoryYAxisLabels[i] = labelMemory;

			HSeparator separatorFps = yAxisHSeparator.Instantiate<HSeparator>();
			fpsPanel.AddChild(separatorFps);
			fpsYAxisSeparators[i] = separatorFps;

			HSeparator separatorFrametime = yAxisHSeparator.Instantiate<HSeparator>();
			frameTimePanel.AddChild(separatorFrametime);
			frametimeYAxisSeparators[i] = separatorFrametime;

			HSeparator separatorMemory = yAxisHSeparator.Instantiate<HSeparator>();
			memoryUsagePanel.AddChild(separatorMemory);
			memoryYAxisSeparators[i] = separatorMemory;
		}
	}

	private void RedrawGraph(List<Vector2> points, Line2D line)
	{
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

	private void ShiftAllGraphsLeft()
	{
		int removeCount = fpsPoints.Count / graphShiftFactor;
		if (removeCount == 0) return;

		float shiftAmount = fpsPoints[removeCount].X;

		// Adjust `currentTime` to prevent drifting
		currentTime -= shiftAmount / 10.0f;

		// Apply shift to all datasets
		ShiftGraphLeft(fpsPoints, fpsLine, shiftAmount);
		ShiftGraphLeft(frameTimePoints, frameTimeLine, shiftAmount);
		ShiftGraphLeft(memoryUsagePoints, memoryUsageLine, shiftAmount);
	}

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

	private void UpdateMaxMinLabels()
	{
		fps1Label.Text = Math.Round(maxFps, 0).ToString();
		fps0Label.Text = Math.Round(minFps, 0).ToString();

		frametime1Label.Text = Math.Round(maxFrameTime, 2).ToString();
		frametime0Label.Text = Math.Round(minFrameTime, 2).ToString();

		memory1Label.Text = Math.Round(maxMemoryUsage, 2).ToString();
		memory0Label.Text = Math.Round(minMemoryUsage, 2).ToString();
	}
}
