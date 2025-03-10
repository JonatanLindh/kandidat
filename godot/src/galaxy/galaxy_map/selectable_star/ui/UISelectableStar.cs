using Godot;
using System;

public partial class UISelectableStar : CanvasLayer
{
	SelectableStar star;
	
	[Export] Control control;

	[Export] Label starNameLabel;
	[Export] Label starPosLabel;
	[Export] Label starSeed;

	[Export] Button closeButton;
	[Export] Button visitButton;

	[Signal]
	public delegate void ExploreStarEventHandler(int seed);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void SetStar(SelectableStar star)
	{
		this.star = star;

		starNameLabel.Text = "Star"; // todo, set actual name
		starPosLabel.Text = star.Position.ToString();
		starSeed.Text = star.GetSeed().ToString();

		Show();
	}

	public void OnCloseButtonPressed()
	{
		Hide();
	}

	public void OnVisitButtonPressed()
	{
		if (star != null) {
			EmitSignal(nameof(ExploreStar), this.star.GetSeed());
			// star.LoadSolarSystem();z
		}
		
	}
}
