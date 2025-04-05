using Godot;
using System;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;

	StarFinder starFinder;
	StarFactory starFactory;

	Node3D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		galaxy = GetNode<InfiniteGalaxy>("%InfiniteGalaxy");

		if(GetTree().CurrentScene.HasNode("Player"))
		{
			player = GetTree().CurrentScene.GetNode<Node3D>("Player");
			galaxy.SetPlayer(player);
		}

		// Use the current node (galaxy center) as the player if no player is found
		else
		{
			player = this;
			galaxy.SetPlayer(player);
		}

		starFinder = GetNode<StarFinder>("%StarFinder");
		starFactory = new StarFactory();

		uiSelectableStar = GetNode<UISelectableStar>("%UiSelectableStar");
		AddToGroup("GalaxyMap");
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
				Vector3 starPos = starFinder.FindStar(this.player.Position, dir, chunks);

				if (starPos != Vector3.Zero)
				{
					Star star = starFactory.CreateStar(starPos, galaxy.GetSeed());
					GD.Print($"Selected star: [Name: {star.name} | Position: {star.transform.Origin} | Seed: {star.seed}]");

					uiSelectableStar.SetStar(star);
					uiSelectableStar.SetPlayer(player);
				}
			}
		}
	}
}
