using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Benchmark : Node3D
{
	[Export] PackedScene[] scenes;
	[Export] bool saveResults = true;
	[Export] bool saveFullResults = false;

	GraphPlotter plot;

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
		this.plot = GetNode<GraphPlotter>("GraphPlotter");

		string absResultPath = ProjectSettings.GlobalizePath(_resultPath);
		filePath = absResultPath + $"/{time}.txt";
		GD.Print("Benchmark getting ready...");

		if (scenes.Length == 0)
		{
			GD.Print("No scenes to benchmark");
			GetTree().Quit();
		}
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

			BenchmarkDatapoint benchmarkDatapoint = new BenchmarkDatapoint
			{
				fps = (float)fps,
				frameTime = (float)frameTime,
				memoryUsage = memoryUsage,
				time = measurementTime
			};

			result[currentSceneIndex].Add(benchmarkDatapoint);
			plot.AddDataPoint(benchmarkDatapoint);
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
		OutputPrint();
		GD.Print($"Benchmark scene done");

		this.downtime = downtime;
		benchmarkSetupDone = false;
		currentScene.QueueFree();

		if(currentSceneIndex == (scenes.Length - 1))
		{
			if (saveResults || saveFullResults)
			{
				Write();
				GD.Print($"Benchmark complete\nResults saved to: {filePath}\n");
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
		GD.Print($"Starting benchmark of {scenes[currentSceneIndex].ResourcePath}");
		currentScene = (BenchmarkScene) scenes[currentSceneIndex].Instantiate();
		currentScene.setBenchmark(this);
		AddChild(currentScene);
	}

	private void OutputPrint()
	{
		var firstTime = result[result.Count - 1][0].time;
		var lastTime = result[result.Count - 1][result[result.Count - 1].Count - 1].time;
		GD.Print($"Ran test from {firstTime} to {lastTime}");
		var timeSpan = DateTime.Parse(lastTime) - DateTime.Parse(firstTime);
		GD.Print($"Test took: {timeSpan.Minutes} min and {timeSpan.Seconds} seconds\n");

		GD.Print($"Average FPS: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FPS)}");
		GD.Print($"1% low FPS: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.01f)}");
		GD.Print($"0.1% low FPS: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.001f)}\n");

		GD.Print($"Average Frame Time: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime)}");
		GD.Print($"1% high Frame Time: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f)}");
		GD.Print($"0.1% high Frame Time: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f)}\n");

		GD.Print($"Average Memory Usage: {processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage)}");
		GD.Print($"1% high Memory Usage: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f)}");
		GD.Print($"0.1% high Memory Usage: {processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f)}\n");
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

			// Time spent
			var firstTime = result[i][0].time;
			var lastTime = result[i][result[i].Count - 1].time;
			resultFile.StoreString($"Ran test from {firstTime} to {lastTime}\n");
			
			var timeSpan = DateTime.Parse(lastTime) - DateTime.Parse(firstTime);
			resultFile.StoreString($"Test took: {timeSpan.Minutes} min and {timeSpan.Seconds} seconds\n");

			// Full results (all measured datapoints)
			if (saveFullResults)
			{
				foreach (var dataPoint in result[i])
				{
					resultFile.StoreString($"Time: {dataPoint.time}, FPS: {dataPoint.fps}, Frametime: {dataPoint.frameTime}, Memory Usage: {dataPoint.memoryUsage}\n");
				}
			}
			resultFile.StoreString("\n");

			// FPS
			var averageFPS = processor.GetAverage(i, result, BenchmarkDatapointEnum.FPS);
			var onePercentLowFPS = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FPS, low: true, 0.01f);
			var pointOnePercentLowFPS = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FPS, low: true, 0.001f);
			resultFile.StoreString($"Average FPS: {averageFPS}\n");
			resultFile.StoreString($"1% low FPS: {onePercentLowFPS}\n");
			resultFile.StoreString($"0.1% low FPS: {pointOnePercentLowFPS}\n\n");

			// Frametime
			var averageFrameTime = processor.GetAverage(i, result, BenchmarkDatapointEnum.FrameTime);
			var onePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f);
			var pointOnePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f);
			resultFile.StoreString($"Average Frametime: {averageFrameTime}\n");
			resultFile.StoreString($"1% high Frametime: {onePercentHighFrameTime}\n");
			resultFile.StoreString($"0.1% high Frametime: {pointOnePercentHighFrameTime}\n\n");

			// Memory Usage
			var averageMemoryUsage = processor.GetAverage(i, result, BenchmarkDatapointEnum.MemoryUsage);
			var onePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f);
			var pointOnePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f);
			resultFile.StoreString($"Average Memory Usage: {averageMemoryUsage}\n");
			resultFile.StoreString($"1% high Memory Usage: {onePercentHighMemoryUsage}\n");
			resultFile.StoreString($"0.1% high Memory Usage: {pointOnePercentHighMemoryUsage}\n\n");

			resultFile.StoreString("Benchmark done\n");
		}

		resultFile.Close();
	}
}
