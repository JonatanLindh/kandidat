using Godot;
using System;

public partial class UISelectableStar : CanvasLayer
{
	SelectableStar star;

	Node3D player;

	bool isTraveling = false;
	Vector3 targetPosition;
	[Export] float travelSpeed = 500.0f;
	float travelDistance = 10.0f;

	[Export] Control control;

	[Export] Panel starSelect;
	float starSelectOffsetStrength = 600;

	[Export] Label starNameLabel;
	[Export] Label starPosLabel;
	[Export] Label starSeed;
	[Export] Label starDistance;

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
		if (isTraveling)
		{
			player.Position = player.Position.Lerp(targetPosition, (float)delta * 0.01f * travelSpeed);

			if (player.Position.DistanceTo(targetPosition) < travelDistance)
			{
				isTraveling = false;
			}
		}

		if (star != null)
		{
			float distance = player.Position.DistanceTo(targetPosition);
			
			starDistance.Text = "Distance: " + ((int)distance).ToString() + " LY";
			Vector3 cameraPosition = GetViewport().GetCamera3D().GlobalTransform.Origin;
			Vector3 cameraForward = GetViewport().GetCamera3D().GlobalTransform.Basis.Z;
			Vector3 toTarget = targetPosition - cameraPosition;
			
			if (cameraForward.Dot(toTarget) < 0){
				Vector2 screenPosition = GetViewport().GetCamera3D().UnprojectPosition(targetPosition);
				Vector2 posOffset = new Vector2(0, -starSelect.Size.Y / 2);
				
				float offsetStrength = Mathf.Clamp(1 / distance, 0, 1) * starSelectOffsetStrength;
				Vector2 distanceOffset = new Vector2(1, 0) * offsetStrength;

				Vector2 offset = distanceOffset + posOffset;
				starSelect.Position = screenPosition + offset;
			}
			
		}
	}

	public void SetStar(SelectableStar star)
	{
		this.star = star;
		this.targetPosition = star.Position;

		starNameLabel.Text = "Star"; // todo, set actual name
		starPosLabel.Text = star.Position.ToString("F2");
		starSeed.Text = star.GetSeed().ToString();

		Vector2 screenPosition = GetViewport().GetCamera3D().UnprojectPosition(targetPosition);
		starSelect.Position = screenPosition;

		Show();
	}

	public void SetPlayer(Node3D player)
	{
		this.player = player;
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
	public void OnTravelButtonPressed()
	{
		if (player != null)
		{
			isTraveling = true;
		}
	}
}
