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
        galaxy.SetPlayer(player);

        uiSelectableStar = GetNode<UISelectableStar>("%UiSelectableStar");
        AddToGroup("GalaxyMap");
    }

    public void SelectedStarUI(SelectableStar star)
    {
        uiSelectableStar.SetStar(star);
    }
}
