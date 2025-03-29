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

	float sidePadding = 10.0f;

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
			Vector2 screenPosition = GetStarScreenPosition();

			// Calculate the offset for the star select panel based on the star's position
            Vector2 posOffset = new Vector2(0, -starSelect.Size.Y / 2);

			// Calculate the offset based on the distance to the star
			float distance = player.Position.DistanceTo(targetPosition);
            float offsetStrength = Mathf.Clamp(1 / distance, 0, 1) * starSelectOffsetStrength;
            Vector2 distanceOffset = new Vector2(1, 0) * offsetStrength;

            Vector2 newPos = screenPosition + (distanceOffset + posOffset);
			starSelect.Position = newPos;

			// Check if the star select panel is fully visible on the screen
			Rect2 screenSpace = GetViewport().GetVisibleRect();
			Rect2 starSelectBounds = starSelect.GetGlobalRect();

			bool isOnScreen = IsFullyVisible(screenSpace, starSelectBounds);

			// Clamp the position to the screen bounds
			if (!isOnScreen)
			{
				Vector2 dir = GetClosestDirection(screenSpace, starSelectBounds);

				if (dir.X != 0)
				{
					newPos.X = dir.X > 0 ? 
						screenSpace.Position.X + sidePadding : 
						screenSpace.End.X - starSelect.Size.X - sidePadding;
				}

				if (dir.Y != 0)
				{
					newPos.Y = dir.Y > 0 ? 
						screenSpace.Position.Y + sidePadding : 
						screenSpace.End.Y - starSelect.Size.Y - sidePadding;
				}


				//newPos = newPos.Clamped(Vector2.Zero, GetViewportRect().Size - starSelect.Size);
			}

			starSelect.Position = newPos;
            starDistance.Text = "Distance: " + ((int)distance).ToString() + " LY";
        }
	}

	private Vector2 GetStarScreenPosition()
	{
		// Check if the star is behind the camera
		Camera3D camera = GetViewport().GetCamera3D();
		Vector3 cameraForward = -camera.GlobalTransform.Basis.Z.Normalized();
		Vector3 directionToStar = (targetPosition - camera.GlobalPosition).Normalized();
		bool isBehindCamera = cameraForward.Dot(directionToStar) < 0;

		Vector2 screenPosition = GetViewport().GetCamera3D().UnprojectPosition(targetPosition);
		if (isBehindCamera)
		{
			screenPosition = -screenPosition;
		}

		return screenPosition;
	}

	private bool IsFullyVisible(Rect2 checker, Rect2 target)
	{
		return checker.Encloses(target);
		//	checker.HasPoint(target.Position) &&
		//	checker.HasPoint(target.End);
	}

	private Vector2 GetClosestDirection(Rect2 from, Rect2 to)
	{
		Vector2 direction = Vector2.Zero;

		// Calculate horizontal direction
		if (to.Position.X < from.Position.X)
		{
			direction.X = from.Position.X - to.Position.X;
		}
		else if (to.End.X > from.End.X)
		{
			direction.X = from.End.X - to.End.X;
		}

		// Calculate vertical direction
		if (to.Position.Y < from.Position.Y)
		{
			direction.Y = from.Position.Y - to.Position.Y;
		}
		else if (to.End.Y > from.End.Y)
		{
			direction.Y = from.End.Y - to.End.Y;
		}

		return direction;
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
