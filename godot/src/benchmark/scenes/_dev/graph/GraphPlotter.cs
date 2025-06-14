using Godot;
using System;
using System.Collections.Generic;

public partial class GraphPlotter : Control
{
	List<GraphContainer> graphContainers = new List<GraphContainer>();
	CurrentContainer currentContainer;

	bool fpsHidden = false;
	bool frameTimeHidden = false;
	bool memoryHidden = false;

	public override void _Ready()
	{
		currentContainer = GetNode<CurrentContainer>("%CurrentContainer");

		List<Node> children = new List<Node>();
		foreach (Node child in GetChildren())
		{
			if(child.GetChildCount() > 0)
			{
				children.Add(child);
				children.AddRange(child.GetChildren());
			}

			else
			{
				children.Add(child);
			}
		}

		foreach (Node child in children)
		{
			if (child is GraphContainer graphContainer)
			{
				graphContainers.Add(graphContainer);
			}
		}
	}

	/// <summary>
	/// Adds a data point to the graph for FPS, frame time, and memory usage.
	/// </summary>
	/// <param name="data"></param>
	public void AddDataPoint(BenchmarkDatapoint data)
	{
		float currentFps = data.fps;
		float filteredFrameTime = data.frameTime * 1000; // seconds to ms
		float filteredMemoryUsage = data.memoryUsage / (1024 * 1024); // bytes to MB

		foreach(GraphContainer graphContainer in graphContainers)
		{
			BenchmarkDatapointEnum dataType = graphContainer.GetDataType();
			switch (dataType)
			{
				case BenchmarkDatapointEnum.FPS:
					graphContainer.AddDataPoint(currentFps);
					break;
				case BenchmarkDatapointEnum.FrameTime:
					graphContainer.AddDataPoint(filteredFrameTime);
					break;
				case BenchmarkDatapointEnum.MemoryUsage:
					graphContainer.AddDataPoint(filteredMemoryUsage);
					break;
			}
		}
	}

	public void UpdateCurrentData(BenchmarkDatapoint data)
	{
		currentContainer.UpdateCurrentData(data);
	}

	public void HideAllGraphs(bool hide)
	{
		foreach (GraphContainer graphContainer in graphContainers)
		{
			if (hide)
				graphContainer.Hide();
			else
				graphContainer.Show();
		}
	}

	public void HideCurrentValues(bool hide)
	{
		if(hide)
			currentContainer.Hide();
		else
			currentContainer.Show();	
	}

	public void HideGraphType(BenchmarkDatapointEnum dataType, bool hide)
	{
		foreach (GraphContainer graphContainer in graphContainers)
		{
			if (graphContainer.GetDataType() == dataType)
			{
				if (hide)
				{
					graphContainer.Hide();
					switch (dataType)
					{
						case BenchmarkDatapointEnum.FPS:
							fpsHidden = true;
							break;
						case BenchmarkDatapointEnum.FrameTime:
							frameTimeHidden = true;
							break;
						case BenchmarkDatapointEnum.MemoryUsage:
							memoryHidden = true;
							break;
					}
				}

				else
				{
					graphContainer.Show();
					switch (dataType)
					{
						case BenchmarkDatapointEnum.FPS:
							fpsHidden = false;
							break;
						case BenchmarkDatapointEnum.FrameTime:
							frameTimeHidden = false;
							break;
						case BenchmarkDatapointEnum.MemoryUsage:
							memoryHidden = false;
							break;
					}
				}
			}
		}
	}
}
