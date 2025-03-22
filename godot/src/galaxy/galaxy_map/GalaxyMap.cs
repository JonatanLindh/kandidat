using Godot;
using System;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;
	
	StarFinder starFinder;
	float radius = 10;

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
				Vector3 worldDir = GetViewport().GetCamera3D().ProjectPosition(eventButton.Position, 1).Normalized();
				Star star = starFinder.FindStar(this.player.Position, worldDir, radius, radius);

				if(star != null)
				{
					GD.Print($"Selected star: [Name: {star.name} | Position: {star.transform.Origin} | Seed: {star.seed}]");

					uiSelectableStar.SetStar(star);
					uiSelectableStar.SetPlayer(player);
				}
			}
		}
	}
}
