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

			var fps = Engine.GetFramesPerSecond();
			var measurementTime = DateTime.Now.ToString("HH:mm:ss.fff");
			var memoryUsage = OS.GetStaticMemoryUsage();
			var frameTime = delta;

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

			GD.Print("setup done");
			NextScene();
		}

		currentTime += (float)delta;
	}

	public void ExitScene()
	{
		GD.Print($"Benchmark of {currentScene.Name} done");
		NextScene();
	}

	private void NextScene()
	{
		if (currentSceneIndex == (scenes.Length - 1))
		{
			Write();
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

	private void Write()
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

			var averageFPS = CalculateAverageFPS(i);
			resultFile.StoreString($"Average FPS: {averageFPS}\n");

			var onePercentLowFPS = CalculatePercentLowFPS(0.01f, i);
			resultFile.StoreString($"1% Low FPS: {onePercentLowFPS}\n");

			var pointOnePercentLowFPS = CalculatePercentLowFPS(0.001f, i);
			resultFile.StoreString($"0.1% Low FPS: {pointOnePercentLowFPS}\n");

			GD.Print($"Benchmark complete");
			GD.Print($"Average FPS of {scenes[i].ResourcePath}: {averageFPS}");
			GD.Print($"1% Low FPS of {scenes[i].ResourcePath}: {onePercentLowFPS}");
			GD.Print($"0.1% Low FPS of {scenes[i].ResourcePath}: {pointOnePercentLowFPS}");
		}

		resultFile.Close();
	}

	private float CalculateAverageFPS(int scene)
	{
		float sum = 0;
		foreach (var dataPoint in result[scene])
		{
			sum += dataPoint.fps;
		}

		return sum / result[scene].Count;
	}

	private float CalculatePercentLowFPS(float percentage, int scene)
	{
		var fpsValues = new List<float>();
		foreach (var dataPoint in result[scene])
		{
			fpsValues.Add(dataPoint.fps);
		}

		fpsValues.Sort();

		int percentCount = (int)Math.Ceiling(fpsValues.Count * percentage);
		float sum = 0;
		for (int i = 0; i < percentCount; i++)
		{
			sum += fpsValues[i];
		}

		return sum / percentCount;
	}
}
