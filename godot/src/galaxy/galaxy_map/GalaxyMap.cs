using Godot;
using System;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;

	StarFinder starFinder;
	StarFactory starFactory;
	SystemHandler systemHandler;
	ScaleHandler scaleHandler;

	Node3D player;

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

		galaxy.SetPlayer(player);

		scaleHandler.SetSystemHandler(systemHandler);
		scaleHandler.SetPlayer(player);
		scaleHandler.SetGalaxy(galaxy);

		AddToGroup("GalaxyMap");
	}

	public override void _Process(double delta)
	{
		CheckCloseStars();
	}

	private void CheckCloseStars()
	{
		IStarChunkData currentChunk = galaxy.GetPlayerChunk();
		if (currentChunk == null) return;

		Vector3 starPos = starFinder.FindStarInSphere(player.Position, systemHandler.closeStarGenerateRadius, galaxy.GetPlayerChunk());

		if(starPos != Vector3.Zero)
		{
			if(systemHandler.SystemExists(starPos)) return;

			Star star = starFactory.CreateStar(starPos, galaxy.GetSeed());
			if (debugPrint) GD.Print($"GalaxyMap: Created close star: [Name: {star.name} | Position: {star.transform.Origin} | Seed: {star.seed}]");
			systemHandler.GenerateSystem(star);
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
}
