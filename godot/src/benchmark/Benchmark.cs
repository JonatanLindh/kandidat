using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Benchmark : Node3D
{
	[Export] PackedScene[] scenes;
	[Export] bool saveResults = true;
	[Export] bool saveFullResults = false;
	[Export] bool showCurrentValues = true;

	[ExportGroup("Graph (Impacts performance)")]
	[Export] bool plotGraph = false;

	[ExportSubgroup("Graph settings")]
	[Export] bool hideFPS = false;
	[Export] bool hideFrameTime = false;
	[Export] bool hideMemoryUsage = false;

	bool showingCurrentValues = false;
	bool showingGraphs = false;

	GraphPlotter plot;

	string _resultPath = "res://../resources/benchmark";
	string filePath;
	string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
	FileAccess resultFile;

	BenchmarkScene currentScene;
	int currentSceneIndex = -1;

	List<List<BenchmarkDatapoint>> result = new List<List<BenchmarkDatapoint>>();
	BenchmarkDataProcessor processor = new BenchmarkDataProcessor();

	float measurementInterval = 0.0f;
	float currentTime = 0.0f;

	// Time to wait before starting the benchmark. Necessary because the FPS will evaluate
	// to 1 during the first second, since, I believe, no average has been calculated yet.
	float downtime = 1.0f;
	bool benchmarkSetupDone = false;

	public override void _Ready()
	{
		this.plot = GetNode<GraphPlotter>("GraphPlotter");

		if (plotGraph)
		{
			plot.HideAllGraphs(false);
			plot.HideCurrentValues(true);
			showingGraphs = true;
		}

		else if (showCurrentValues)
		{
			plot.HideAllGraphs(true);
			plot.HideCurrentValues(false);
			showingCurrentValues = true;
		}

		else
		{
			plot.HideAllGraphs(true);
			plot.HideCurrentValues(true);
		}

		if(hideFPS) plot.HideGraphType(BenchmarkDatapointEnum.FPS, true);
		if (hideFrameTime) plot.HideGraphType(BenchmarkDatapointEnum.FrameTime, true);
		if (hideMemoryUsage) plot.HideGraphType(BenchmarkDatapointEnum.MemoryUsage, true);

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
			double frameTime = Math.Round(delta, 6);
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
			if(showingGraphs) plot.AddDataPoint(benchmarkDatapoint);
			if(showingCurrentValues) plot.UpdateCurrentData(benchmarkDatapoint);
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

		GD.Print($"Average FPS: {Math.Round(processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FPS), 1)}");
		GD.Print($"1% low FPS: {Math.Round(processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.01f), 1)}");
		GD.Print($"0.1% low FPS: {Math.Round(processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FPS, low: true, 0.001f), 1)}\n");

		float averageFrameTime = processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime);
		float onePercentHighFrameTime = processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f);
		float pointOnePercentHighFrameTime = processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f);
		GD.Print($"Average Frame Time: {averageFrameTime} s | {Math.Round(averageFrameTime * 1000, 2)} ms");
		GD.Print($"1% high Frame Time: {onePercentHighFrameTime} s | {Math.Round(onePercentHighFrameTime * 1000, 2)} ms");
		GD.Print($"0.1% high Frame Time: {pointOnePercentHighFrameTime} s | {Math.Round(pointOnePercentHighFrameTime * 1000, 2)} ms\n");

		float averageMemoryUsage = processor.GetAverage(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage);
		float onePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f);
		float pointOnePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(currentSceneIndex, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f);
		GD.Print($"Average Memory Usage: {averageMemoryUsage} bytes | {Math.Round(averageMemoryUsage / (1024 * 1024), 1)} MB");
		GD.Print($"1% high Memory Usage: {onePercentHighMemoryUsage} bytes | {Math.Round(onePercentHighMemoryUsage / (1024 * 1024), 1)} MB");
		GD.Print($"0.1% high Memory Usage: {Math.Round(pointOnePercentHighMemoryUsage, 1)} bytes | {Math.Round(pointOnePercentHighMemoryUsage / (1024 * 1024), 1)} MB\n");
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
			resultFile.StoreString($"Average FPS: {Math.Round(averageFPS, 1)}\n");
			resultFile.StoreString($"1% low FPS: {Math.Round(onePercentLowFPS, 1)}\n");
			resultFile.StoreString($"0.1% low FPS: {Math.Round(pointOnePercentLowFPS, 1)}\n\n");

			// Frametime
			var averageFrameTime = processor.GetAverage(i, result, BenchmarkDatapointEnum.FrameTime);
			var onePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f);
			var pointOnePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f);
			resultFile.StoreString($"Average Frametime: {averageFrameTime} s | {Math.Round(averageFrameTime * 1000, 2)} ms\n");
			resultFile.StoreString($"1% high Frametime: {onePercentHighFrameTime} s | {Math.Round(onePercentHighFrameTime * 1000, 2)} ms\n");
			resultFile.StoreString($"0.1% high Frametime: {pointOnePercentHighFrameTime} s | {Math.Round(pointOnePercentHighFrameTime * 1000, 2)} ms\n\n");

			// Memory Usage
			var averageMemoryUsage = processor.GetAverage(i, result, BenchmarkDatapointEnum.MemoryUsage);
			var onePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f);
			var pointOnePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, result, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f);
			resultFile.StoreString($"Average Memory Usage: {averageMemoryUsage} bytes | {Math.Round(averageMemoryUsage / (1024 * 1024), 1)} MB\n");
			resultFile.StoreString($"1% high Memory Usage: {onePercentHighMemoryUsage} bytes | {Math.Round(onePercentHighMemoryUsage / (1024 * 1024), 1)} MB\n");
			resultFile.StoreString($"0.1% high Memory Usage: {pointOnePercentHighMemoryUsage} bytes | {Math.Round(pointOnePercentHighMemoryUsage / (1024 * 1024), 1)} MB\n\n");

			resultFile.StoreString("Benchmark done\n");
		}

		resultFile.Close();
	}
}
