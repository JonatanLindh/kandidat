using Godot;
using Godot.Collections;
using System;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;

public partial class Benchmark : Node3D
{
	[Export] PackedScene[] scenes;

	string _resultPath = "res://../resources/benchmark";
	string filePath;
	string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
	FileAccess resultFile;

	Node currentScene;
	int currentSceneIndex = -1;

	string result = "";

	public override void _Ready()
	{
		string absResultPath = ProjectSettings.GlobalizePath(_resultPath);
		filePath = absResultPath + $"/{time}.txt";
		Write($"Benchmark started at {time}\n");

		NextScene();
	}

	public override void _Process(double delta)
	{
		var fps = Engine.GetFramesPerSecond();
		var currentTime = DateTime.Now.ToString("HH:mm:ss.fff");
		var memoryUsage = OS.GetStaticMemoryUsage();

		result += $"Time: {currentTime}, FPS: {fps}, Delta: {delta}, Memory: {memoryUsage} bytes\n";
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
			GD.Print("Benchmark finished");
			GD.Print("Results saved to: " + filePath);

			Write(result);
			GetTree().Quit();
		}

		currentSceneIndex++;
		if(currentSceneIndex < scenes.Length)
		{
			if(currentScene != null)
			{
				currentScene.QueueFree();
			}

			GD.Print($"Starting benchmark of {scenes[currentSceneIndex].ToString()}");
			result += $"\n------------------------------------\n" +
				$"Starting benchmark of {scenes[currentSceneIndex].ToString()}" +
				$"\n------------------------------------\n";

			currentScene = scenes[currentSceneIndex].Instantiate();
			currentScene.Call("setBenchmark", this);
			AddChild(currentScene);
		}
	}

	private void Write(string text)
	{
		resultFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
		resultFile.StoreString(text);
		resultFile.Close();
	}
}
