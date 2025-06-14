using Godot;
using System;
using System.Collections.Generic;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;

	StarFinder starFinder;
	StarFactory starFactory;
	SystemHandler systemHandler;
	ScaleHandler scaleHandler;

	Node3D player;
	Node gameSettings;

	[ExportCategory("Seamless Galaxy")]
	[Export] bool useSeamlessGalaxy = true;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		galaxy = GetNode<InfiniteGalaxy>("%InfiniteGalaxy");
		uiSelectableStar = GetNode<UISelectableStar>("%UiSelectableStar");
		starFinder = GetNode<StarFinder>("%StarFinder");
		systemHandler = GetNode<SystemHandler>("%SystemHandler");
		scaleHandler = GetNode<ScaleHandler>("%ScaleHandler");
		starFactory = new StarFactory();

		Node3D initPlayer;
		if (GetTree().CurrentScene.HasNode("Player")) initPlayer = GetTree().CurrentScene.GetNode<Node3D>("Player");
		else initPlayer = this; // Use the current node (galaxy center) as the player if no player is found
		player = initPlayer;

		gameSettings = GetNode<Node>("/root/GameSettings");

		galaxy.SetPlayer(player);

		uint seed = (uint)gameSettings.Get("SEED");
		galaxy.SetSeed(seed);

		scaleHandler.SetSystemHandler(systemHandler);
		scaleHandler.SetPlayer(player);
		scaleHandler.SetGalaxy(galaxy);

		AddToGroup("GalaxyMap");
	}

	public override void _Process(double delta)
	{
		if(useSeamlessGalaxy) InstantiateCloseStars();
	}

	private void InstantiateCloseStars()
	{
		IStarChunkData[] chunks = galaxy.GetGeneratedChunks();
		if (chunks == null || chunks.Length == 0) return;

		List<Vector3> starPos = starFinder.FindAllStarsInSphere(player.Position, systemHandler.closeStarEarlyGenerateRadius, chunks);

		if (starPos != null)
		{
			foreach (Vector3 pos in starPos)
			{
				if (systemHandler.SystemExists(pos)) continue;

				Star star = starFactory.CreateStar(pos, galaxy.GetSeed());
				if (debugPrint) GD.Print($"GalaxyMap: Created close star: [Name: {star.name} | Position: {star.transform.Origin} | Seed: {star.seed}]");
				systemHandler.GenerateSystem(star);
			}
		}

		systemHandler.CullFarSystems(player.Position);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton eventButton)
		{
			if(eventButton.Pressed && eventButton.ButtonIndex == MouseButton.Left)
			{
				Camera3D camera = GetViewport().GetCamera3D();
				Vector3 dir = camera.ProjectRayNormal(eventButton.Position);

				IStarChunkData[] chunks = galaxy.GetGeneratedChunks();
				Vector3 starPos = starFinder.FindStarInLine(this.player.Position, dir, chunks);

				if (starPos != Vector3.Zero)
				{
					Star star = starFactory.CreateStar(starPos, galaxy.GetSeed());
					if(debugPrint) GD.Print($"GalaxyMap: Selected star: [Name: {star.name} | Position: {star.transform.Origin} | Seed: {star.seed}]");

					uiSelectableStar.SetStar(star);
					uiSelectableStar.SetPlayer(player);
				}
			}
		}
	}

	public void SetPlayer(Node3D player)
	{
		this.player = player;
		galaxy.SetPlayer(player);
		scaleHandler.SetPlayer(player);
		uiSelectableStar.SetPlayer(player);
	}
}
