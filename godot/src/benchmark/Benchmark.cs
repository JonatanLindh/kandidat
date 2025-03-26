using Godot;
using System;
using System.Collections.Generic;

public partial class Benchmark : Node3D
{
	[Export] PackedScene[] scenes;

	string _resultPath = "res://../resources/benchmark";
	string filePath;
	string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
	FileAccess resultFile;

	Node currentScene;
	int currentSceneIndex = -1;

	List<List<BenchmarkDatapoint>> result = new List<List<BenchmarkDatapoint>>();

	float measurementInterval = 0.1f; // 100ms
	float currentTime = 0.0f;

	// Time to wait before starting the benchmark.
	// Necessary because the FPS will evaluate to 1, since no average has been calculated yet.
	float benchmarkSetupTime = 1.0f;
	bool benchmarkSetupDone = false;

	public override void _Ready()
	{
		string absResultPath = ProjectSettings.GlobalizePath(_resultPath);
		filePath = absResultPath + $"/{time}.txt";
		GD.Print("Getting ready...");
	}

	public override void _Process(double delta)
	{
		if(currentTime > measurementInterval && benchmarkSetupDone)
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

		if(currentTime > benchmarkSetupTime && !benchmarkSetupDone)
		{
			currentTime = 0;
			benchmarkSetupDone = true;
			NextScene();
		}

		currentTime += (float)delta;
	}

	public void ExitScene()
	{
		GD.Print($"Benchmark of {scenes[currentSceneIndex].ResourcePath} done");
		NextScene();
	}

	private void NextScene()
	{
		if (currentSceneIndex == (scenes.Length - 1))
		{
			BenchmarkDataProcessor processor = new BenchmarkDataProcessor(result);

			Write(processor);

			GD.Print($"\nBenchmark finished\nResults saved to: {filePath}\n----------");
			GetTree().Quit();
		}

		currentSceneIndex++;
		if(currentSceneIndex < scenes.Length)
		{
			if(currentScene != null)
			{
				currentScene.QueueFree();
			}

			GD.Print($"Starting benchmark of {scenes[currentSceneIndex].ResourcePath}");

			currentScene = scenes[currentSceneIndex].Instantiate();
			currentScene.Call("setBenchmark", this);
			AddChild(currentScene);
		}
	}

	private void Write(BenchmarkDataProcessor processor)
	{
		resultFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
		resultFile.StoreString($"Benchmark started at {time}\n\n");

		resultFile.StoreString($"Computer specifications:\n");
		resultFile.StoreString($"OS: {OS.GetName()}\n");
		resultFile.StoreString($"CPU: {OS.GetProcessorName()}\n");
		resultFile.StoreString($"GPU: {RenderingServer.GetRenderingDevice().GetDeviceName()}\n");

		var memoryInfo = OS.GetMemoryInfo();
		var physicalMemory = memoryInfo["physical"];
		resultFile.StoreString($"RAM: {(ulong)physicalMemory / (1024 * 1024)} MB\n\n");

		for (int i = 0; i < scenes.Length; i++)
		{
			resultFile.StoreString($"Benchmark of {scenes[i].ResourcePath}:\n");
			foreach (var dataPoint in result[i])
			{
				resultFile.StoreString($"Time: {dataPoint.time}, FPS: {dataPoint.fps}, Frametime: {dataPoint.frameTime}, Memory Usage: {dataPoint.memoryUsage}\n");
			}
			resultFile.StoreString("\n");

			// Averages
			var averageFPS = processor.GetAverage(i, BenchmarkDatapointEnum.FPS);
			var averageFrameTime = processor.GetAverage(i, BenchmarkDatapointEnum.FrameTime);
			var averageMemoryUsage = processor.GetAverage(i, BenchmarkDatapointEnum.MemoryUsage);
			resultFile.StoreString($"Average FPS: {averageFPS}\n");
			resultFile.StoreString($"Average Frametime: {averageFrameTime}\n");
			resultFile.StoreString($"Average Memory Usage: {averageMemoryUsage}\n\n");

			// 1% lows/highs
			var onePercentLowFPS = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.FPS, low: true, 0.01f);
			var onePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.FrameTime, low: false, 0.01f);
			var onePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.01f);
			resultFile.StoreString($"1% low FPS: {onePercentLowFPS}\n");
			resultFile.StoreString($"1% high Frametime: {onePercentHighFrameTime}\n");
			resultFile.StoreString($"1% high Memory Usage: {onePercentHighMemoryUsage}\n\n");

			// 0.1% lows/highs
			var pointOnePercentLowFPS = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.FPS, low: true, 0.001f);
			var pointOnePercentHighFrameTime = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.FrameTime, low: false, 0.001f);
			var pointOnePercentHighMemoryUsage = processor.GetPercentageLowOrHigh(i, BenchmarkDatapointEnum.MemoryUsage, low: false, 0.001f);
			resultFile.StoreString($"0.1% low FPS: {pointOnePercentLowFPS}\n");
			resultFile.StoreString($"0.1% high Frametime: {pointOnePercentHighFrameTime}\n");
			resultFile.StoreString($"0.1% high Memory Usage: {pointOnePercentHighMemoryUsage}\n\n");
		}

		resultFile.Close();
	}
}
