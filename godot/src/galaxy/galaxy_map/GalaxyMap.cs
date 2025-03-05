using Godot;
using System;

public partial class GalaxyMap : Node3D
{
	InfiniteGalaxy galaxy;
	UISelectableStar uiSelectableStar;

    [Export] Node3D player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        galaxy = GetNode<InfiniteGalaxy>("%InfiniteGalaxy");
        uiSelectableStar = GetNode<UISelectableStar>("%UiSelectableStar");

        galaxy.SetPlayer(player);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SelectedStarUI(SelectableStar star)
    {
        uiSelectableStar.SetStar(star);
    }
}
