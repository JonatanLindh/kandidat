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
	float starSelectOffsetStrength = 800;

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
            Vector2 screenPosition = GetViewport().GetCamera3D().UnprojectPosition(targetPosition);
            Vector2 posOffset = new Vector2(0, -starSelect.Size.Y / 2);

            float distance = player.Position.DistanceTo(targetPosition);
            float offsetStrength = Mathf.Clamp(1 / distance, 0, 1) * starSelectOffsetStrength;
            Vector2 distanceOffset = new Vector2(1, 0) * offsetStrength;

            Vector2 newPos = screenPosition + (distanceOffset + posOffset);
            Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

            // Check if the star is behind the player
            Vector3 toStar = targetPosition - player.Position;
            Vector3 forward = player.GlobalTransform.Basis.Z;
            bool isBehind = forward.Dot(toStar) > 0;

            if (isBehind)
            {
                // Adjust the position to the side of the screen
                newPos.X = viewportSize.X - starSelect.Size.X - 10; // 10 pixels from the right edge
                newPos.Y = viewportSize.Y / 2 - starSelect.Size.Y / 2; // Center vertically
            }

            else
            {
                // Clamp the position to keep the panel within the screen bounds
                newPos.X = Mathf.Clamp(newPos.X, 0, viewportSize.X - starSelect.Size.X);
                newPos.Y = Mathf.Clamp(newPos.Y, 0, viewportSize.Y - starSelect.Size.Y);
            }

            starSelect.Position = newPos;

            starDistance.Text = "Distance: " + ((int)distance).ToString() + " LY";
        }
	}

	public void SetStar(SelectableStar star)
	{
		this.star = star;
		this.targetPosition = star.Position;

		starNameLabel.Text = "Solar system"; // todo, set actual name
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
