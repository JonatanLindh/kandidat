using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Benchmark : Node3D
{
	[Export] PackedScene[] scenes;
	[Export] bool saveResults = true;
	[Export] bool saveFullResults = false;

	string _resultPath = "res://../resources/benchmark";
	string filePath;
	string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
	FileAccess resultFile;

	BenchmarkScene currentScene;
	int currentSceneIndex = -1;

	List<List<BenchmarkDatapoint>> result = new List<List<BenchmarkDatapoint>>();
	BenchmarkDataProcessor processor = new BenchmarkDataProcessor();

	float measurementInterval = 0.01f; // 10ms
	float currentTime = 0.0f;

	// Time to wait before starting the benchmark. Necessary because the FPS will evaluate
	// to 1 during the first second, since, I believe, no average has been calculated yet.
	float downtime = 1.0f;
	bool benchmarkSetupDone = false;

	public override void _Ready()
	{
		string absResultPath = ProjectSettings.GlobalizePath(_resultPath);
		filePath = absResultPath + $"/{time}.txt";
		GD.Print("Getting ready...");
	}

	public override void _Process(double delta)
	{
		currentTime += (float)delta;

		if (currentTime > measurementInterval && benchmarkSetupDone)
		{
			currentTime = 0;

			double fps = Engine.GetFramesPerSecond();
			double frameTime = Math.Round(delta, 7);
			ulong memoryUsage = OS.GetStaticMemoryUsage();
			string measurementTime = DateTime.Now.ToString("HH:mm:ss.fff");

			if (result.Count <= currentSceneIndex)
			{
				result.Add(new List<BenchmarkDatapoint>());
			}

			result[currentSceneIndex].Add(new BenchmarkDatapoint
			{
				fps = (float)fps,
				frameTime = (float)frameTime,
				memoryUsage = memoryUsage,
				time = measurementTime
			});
		}

		if(currentTime > downtime && !benchmarkSetupDone)
		{
			currentTime = 0;
			benchmarkSetupDone = true;
			NextScene();
		}
	}

	public void ExitScene(float downtime)
	{
		GD.Print($"\nAverage FPS: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FPS)}");
		GD.Print($"Average Frame Time: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime)}");
		GD.Print($"Average Memory Usage: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage)}\n");

		GD.Print($"1% low FPS: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.01f)}");
		GD.Print($"1% high Frame Time: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f)}");
		GD.Print($"1% high Memory Usage: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f)}\n");

		GD.Print($"0.1% low FPS: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.001f)}");
		GD.Print($"0.1% high Frame Time: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f)}");
		GD.Print($"0.1% high Memory Usage: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f)}\n");

		GD.Print($"Benchmark done");

		this.downtime = downtime;
		benchmarkSetupDone = false;
		currentScene.QueueFree();

		if(currentSceneIndex == (scenes.Length - 1))
		{
			if (saveResults || saveFullResults)
			{
				Write();
				GD.Print($"Full benchmark complete\nResults saved to: {filePath}\n");
			}

			else
			{
				GD.Print($"Benchmark complete");
			}

			GetTree().Quit();
		}
	}

	private void NextScene()
	{
		currentSceneIndex++;
		if(currentSceneIndex < scenes.Length)
		{
			GD.Print($"Starting benchmark of {scenes[currentSceneIndex].ResourcePath}");

			currentScene = (BenchmarkScene) scenes[currentSceneIndex].Instantiate();
			currentScene.setBenchmark(this);
			AddChild(currentScene);
		}
	}

	private void Write()
	{
		resultFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
		resultFile.StoreString($"Benchmark started at {time}\n\n");

		resultFile.StoreString($"Specifications:\n");
		resultFile.StoreString($"OS: {OS.GetName()}\n");
		resultFile.StoreString($"CPU: {OS.GetProcessorName()}\n");
		resultFile.StoreString($"GPU: {RenderingServer.GetRenderingDevice().GetDeviceName()}\n");

		var memoryInfo = OS.GetMemoryInfo();
		var physicalMemory = memoryInfo["physical"];
		resultFile.StoreString($"RAM: {(ulong)physicalMemory / (1024 * 1024)} MB\n");

		for (int i = 0; i < scenes.Length; i++)
		{
			resultFile.StoreString("\n------------------------------------------------");
			resultFile.StoreString($"\nStarting benchmark of {scenes[i].ResourcePath}:\n");
			if (saveFullResults)
			{
				foreach (var dataPoint in result[i])
				{
					resultFile.StoreString($"Time: {dataPoint.time}, FPS: {dataPoint.fps}, Frametime: {dataPoint.frameTime}, Memory Usage: {dataPoint.memoryUsage}\n");
				}
			}
			resultFile.StoreString("\n");

			// Averages
			var averageFPS = processor.GetAverage(i, result, BenchmarkDatapointEnum.FPS);
			var averageFrameTime = processor.GetAverage(i, result, BenchmarkDatapointEnum.FrameTime);
			var averageMemoryUsage = processor.GetAverage(i, result, BenchmarkDatapointEnum.MemoryUsage);
			resultFile.StoreString($"Average FPS: {averageFPS}\n");
			resultFile.StoreString($"Average Frametime: {averageFrameTime}\n");
			resultFile.StoreString($"Average Memory Usage: {averageMemoryUsage}\n\n");

			// 1% lows/highs
			var onePercentLowFPS = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FPS, low: true, 0.01f);
			var onePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f);
			var onePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f);
			resultFile.StoreString($"1% low FPS: {onePercentLowFPS}\n");
			resultFile.StoreString($"1% high Frametime: {onePercentHighFrameTime}\n");
			resultFile.StoreString($"1% high Memory Usage: {onePercentHighMemoryUsage}\n\n");

			// 0.1% lows/highs
			var pointOnePercentLowFPS = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FPS, low: true, 0.001f);
			var pointOnePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f);
			var pointOnePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f);
			resultFile.StoreString($"0.1% low FPS: {pointOnePercentLowFPS}\n");
			resultFile.StoreString($"0.1% high Frametime: {pointOnePercentHighFrameTime}\n");
			resultFile.StoreString($"0.1% high Memory Usage: {pointOnePercentHighMemoryUsage}\n\n");

			resultFile.StoreString("Benchmark done\n");
		}

		resultFile.Close();
	}
}
