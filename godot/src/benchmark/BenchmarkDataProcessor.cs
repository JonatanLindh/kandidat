using Godot;
using System;
using System.Collections.Generic;

public class BenchmarkDataProcessor
{
	private List<float> GetDataSeparated(int scene, List<BenchmarkDatapoint> data, BenchmarkDatapointEnum dataType)
	{
		List<float> dataSeparated = new List<float>();
		foreach (BenchmarkDatapoint datapoint in data)
		{
			switch (dataType)
			{
				case BenchmarkDatapointEnum.FPS:
					dataSeparated.Add(datapoint.fps);
					break;
				case BenchmarkDatapointEnum.FrameTime:
					dataSeparated.Add(datapoint.frameTime);
					break;
				case BenchmarkDatapointEnum.MemoryUsage:
					dataSeparated.Add(datapoint.memoryUsage);
					break;
			}
		}
		return dataSeparated;
	}

	private List<float> GetDataSorted(int scene, List<BenchmarkDatapoint> data, BenchmarkDatapointEnum dataType)
	{
		List<float> dataSorted = new List<float>();
		dataSorted = GetDataSeparated(scene, data, dataType);
		dataSorted.Sort();
		return dataSorted;
	}

	public float GetAverage(int scene, List<List<BenchmarkDatapoint>> data, BenchmarkDatapointEnum dataType)
	{
		List<float> dataSeparated = null;
		switch (dataType)
		{
			case BenchmarkDatapointEnum.FPS:
				dataSeparated = GetDataSeparated(scene, data[scene], BenchmarkDatapointEnum.FPS);
				break;
			case BenchmarkDatapointEnum.FrameTime:
				dataSeparated = GetDataSeparated(scene, data[scene], BenchmarkDatapointEnum.FrameTime);
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				dataSeparated = GetDataSeparated(scene, data[scene], BenchmarkDatapointEnum.MemoryUsage);
				break;
		}

		float sum = 0;
		foreach (var value in dataSeparated)
		{
			sum += value;
		}

		float average = sum / dataSeparated.Count;
		return average;
	}

	/// <summary>
	/// Returns the average of the lowest or highest percentage of the data.
	/// "low" determines if it involves the lowest or highest percentage.
	/// </summary>
	/// <param name="dataType"></param>
	/// <param name="low"></param>
	/// <param name="percentage"></param>
	/// <returns></returns>
	public float GetPercentageLowOrHigh(int scene, List<List<BenchmarkDatapoint>> data, BenchmarkDatapointEnum dataType, bool low, float percentage)
	{
		List<float> dataSorted = null;
		switch (dataType)
		{
			case BenchmarkDatapointEnum.FPS:
				dataSorted = GetDataSorted(scene, data[scene], BenchmarkDatapointEnum.FPS);
				break;
			case BenchmarkDatapointEnum.FrameTime:
				dataSorted = GetDataSorted(scene, data[scene], BenchmarkDatapointEnum.FrameTime);
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				dataSorted = GetDataSorted(scene, data[scene], BenchmarkDatapointEnum.MemoryUsage);
				break;
		}

		int percentCount = (int)Math.Ceiling(dataSorted.Count * percentage);
		float sum = 0;

		if(low)
		{
			for (int i = 0; i < percentCount; i++)
			{
				sum += dataSorted[i];
			}
		}

		else
		{
			for (int i = dataSorted.Count - 1; i >= dataSorted.Count - percentCount; i--)
			{
				sum += dataSorted[i];
			}
		}

		float average = sum / percentCount;
		return average;
	}
}
