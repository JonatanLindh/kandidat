using Godot;
using System;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;

	StarFinder starFinder;
	StarFactory starFactory = new StarFactory();

	Node3D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		galaxy = GetNode<InfiniteGalaxy>("%InfiniteGalaxy");
		this.player = GetTree().CurrentScene.GetNode<Node3D>("Player");
		galaxy.SetPlayer(player);

		starFinder = GetNode<StarFinder>("%StarFinder");
		starFinder.galaxy = galaxy;

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

				Vector3 starPos = starFinder.FindStar(this.player.Position, dir);

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
