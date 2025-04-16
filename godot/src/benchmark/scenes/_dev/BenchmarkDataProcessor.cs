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
		// Special handling for FPS calculation
		if (dataType == BenchmarkDatapointEnum.FPS)
		{
			// Get all frame times
			List<float> frameTimes = GetDataSeparated(scene, data[scene], BenchmarkDatapointEnum.FrameTime);
			
			// Calculate average frame time
			float sum = 0;
			foreach (var frameTime in frameTimes)
			{
				sum += frameTime;
			}
			
			float avgFrameTime = sum / frameTimes.Count;
			return 1.0f / avgFrameTime;
		}
		else
		{
			List<float> dataSeparated = GetDataSeparated(scene, data[scene], dataType);
			
			float sum = 0;
			foreach (var value in dataSeparated)
			{
				sum += value;
			}
			
			float average = sum / dataSeparated.Count;
			return average;
		}
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
		// Special handling for FPS calculation
		if (dataType == BenchmarkDatapointEnum.FPS)
		{
			// Get the frame times
			List<float> frameTimes = GetDataSorted(scene, data[scene], BenchmarkDatapointEnum.FrameTime);
			int percentCount = (int)Math.Ceiling(frameTimes.Count * percentage);
			float sum = 0;

			// Calculate frame time averages
			if (!low) // FPS is highest when frame times are lowest
			{
				for (int i = 0; i < percentCount; i++)
				{
					sum += frameTimes[i];
				}
			}
			else
			{
				for (int i = frameTimes.Count - 1; i >= frameTimes.Count - percentCount; i--)
				{
					sum += frameTimes[i];
				}
			}
			
			float avgFrameTime = sum / percentCount;
			return 1.0f / avgFrameTime;
		}
		else
		{
			List<float> dataSorted = GetDataSorted(scene, data[scene], dataType);
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
}
