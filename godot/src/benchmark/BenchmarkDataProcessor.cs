using Godot;
using System;
using System.Collections.Generic;

public class BenchmarkDataProcessor
{
	List<List<BenchmarkDatapoint>> data;

	List<List<float>> fps = new List<List<float>>();
	List<List<float>> frameTime = new List<List<float>>();
	List<List<float>> memoryUsage = new List<List<float>>();

	List<List<float>> fpsSorted = new List<List<float>>();
	List<List<float>> frameTimeSorted = new List<List<float>>();
	List<List<float>> memoryUsageSorted = new List<List<float>>();

	public BenchmarkDataProcessor(List<List<BenchmarkDatapoint>> data)
	{
		this.data = data;

		SeparateData();
		SortData();
	}

	private void SeparateData()
	{
		foreach (var scene in data)
		{
			List<float> fpsScene = new List<float>();
			List<float> frameTimeScene = new List<float>();
			List<float> memoryUsageScene = new List<float>();

			foreach (var datapoint in scene)
			{
				fpsScene.Add(datapoint.fps);
				frameTimeScene.Add(datapoint.frameTime);
				memoryUsageScene.Add(datapoint.memoryUsage);
			}

			fps.Add(fpsScene);
			frameTime.Add(frameTimeScene);
			memoryUsage.Add(memoryUsageScene);
		}
	}

	private void SortData()
	{
		for(int i = 0; i < data.Count; i++)
		{
			List<float> fpsScene = new List<float>(fps[i]);
			List<float> frameTimeScene = new List<float>(frameTime[i]);
			List<float> memoryUsageScene = new List<float>(memoryUsage[i]);

			fpsScene.Sort();
			frameTimeScene.Sort();
			memoryUsageScene.Sort();
			
			fpsSorted.Add(fpsScene);
			frameTimeSorted.Add(frameTimeScene);
			memoryUsageSorted.Add(memoryUsageScene);
		}
	}

	public float GetAverage(int scene, BenchmarkDatapointEnum dataType)
	{
		List<float> data = null;
		switch (dataType)
		{
			case BenchmarkDatapointEnum.FPS:
				data = fps[scene];
				break;
			case BenchmarkDatapointEnum.FrameTime:
				data = frameTime[scene];
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				data = memoryUsage[scene];
				break;
		}

		float sum = 0;
		foreach (var value in data)
		{
			sum += value;
		}

		float average = sum / data.Count;
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
	public float GetPercentageLowOrHigh(int scene, BenchmarkDatapointEnum dataType, bool low, float percentage)
	{
		List<float> dataSorted = null;
		switch (dataType)
		{
			case BenchmarkDatapointEnum.FPS:
				dataSorted = fpsSorted[scene];
				break;
			case BenchmarkDatapointEnum.FrameTime:
				dataSorted = frameTimeSorted[scene];
				break;
			case BenchmarkDatapointEnum.MemoryUsage:
				dataSorted = memoryUsageSorted[scene];
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
