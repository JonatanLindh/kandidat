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

	public override void _Ready()
	{
		Hide();
	}

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

		// Update the star UI
		if (star != null)
		{
			Vector2 screenPosition = GetVector3ScreenPosition(targetPosition);

			// Offset to center the star ui on the star
			Vector2 posOffset = new Vector2(0, -starSelect.Size.Y / 2);

			// Offset based on the distance to the star
			float distance = player.Position.DistanceTo(targetPosition);
            float offsetStrength = Mathf.Clamp(1 / distance, 0, 1) * starSelectOffsetStrength;
            Vector2 distanceOffset = new Vector2(1, 0) * offsetStrength;

			// Proposed new UI position
            Vector2 newPos = screenPosition + (distanceOffset + posOffset);
			starSelect.Position = newPos;

			// Make sure that the new position ensures that the entire panel is within screen bounds
			newPos = GetClampedPositionIfOutside(newPos);

			starSelect.Position = newPos;
            starDistance.Text = "Distance: " + ((int)distance).ToString() + " LY";
        }
	}

	/// <summary>
	/// Calculates the screen position of a 3D point in world space.
	/// If the point is behind the camera or outside the screen, it will be clamped to the edges.
	/// </summary>
	/// <param name="pos"></param>
	/// <returns>A <c>Vector2</c> of the screen position</returns>
	private Vector2 GetVector3ScreenPosition(Vector3 pos)
	{
		Camera3D camera = GetViewport().GetCamera3D();
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		Vector2 screenPosition = camera.UnprojectPosition(pos);
		
		Vector3 directionToStar = (pos - camera.GlobalPosition).Normalized();
		float dotProduct = directionToStar.Dot(-camera.GlobalTransform.Basis.Z);

		bool isBehindCamera = dotProduct <= 0;
		bool isOutsideScreen = screenPosition.X < 0 || screenPosition.X > screenSize.X || screenPosition.Y < 0 || screenPosition.Y > screenSize.Y;
		
		// If star is behind camera or position is outside screen, clamp to edges
		if (isBehindCamera || isOutsideScreen)
		{
			Vector2 screenCenter = screenSize / 2;
			Vector2 direction;
			
			if (isBehindCamera) {
				// Invert the direction
				direction = (screenCenter - screenPosition).Normalized();
			} else { // If outside screen
				direction = (screenPosition - screenCenter).Normalized();
			}
			
			float maxX = screenSize.X - sidePadding;
			float maxY = screenSize.Y - sidePadding;
			
			// Calculate scale for each edge
			float scaleLeft 	= direction.X != 0 ? (sidePadding - screenCenter.X) / direction.X 	: float.MaxValue;
			float scaleRight 	= direction.X != 0 ? (maxX - screenCenter.X) / direction.X 			: float.MaxValue;
			float scaleTop 		= direction.Y != 0 ? (sidePadding - screenCenter.Y) / direction.Y 	: float.MaxValue;
			float scaleBottom 	= direction.Y != 0 ? (maxY - screenCenter.Y) / direction.Y 			: float.MaxValue;
			
			// Find closest edge
			float scale = float.MaxValue;
			if (scaleLeft > 0 && scaleLeft < scale) 	scale = scaleLeft;
			if (scaleRight > 0 && scaleRight < scale) 	scale = scaleRight;
			if (scaleTop > 0 && scaleTop < scale) 		scale = scaleTop;
			if (scaleBottom > 0 && scaleBottom < scale)	scale = scaleBottom;
			
			// Position at screen edge in direction of star
			screenPosition = screenCenter + direction * scale;
		}
		
		return screenPosition;
	}

	/// <summary>
	/// Checks if the star select panel is fully visible on the screen.
	/// If not, it clamps the position to the screen bounds.
	/// </summary>
	/// <param name="basePos"></param>
	/// <returns>A <c>Vector2</c> of the (potentially) clamped screen position</returns>
	private Vector2 GetClampedPositionIfOutside(Vector2 basePos)
	{
		Vector2 clampedPos = basePos;

		// Check if the star select panel is fully visible on the screen
		Rect2 screenSpace = GetViewport().GetVisibleRect();
		Rect2 starSelectBounds = starSelect.GetGlobalRect();

		bool isOnScreen = IsFullyVisible(screenSpace, starSelectBounds);

		// Clamp the position to the screen bounds
		if (!isOnScreen)
		{
			Vector2 dir = GetClosestDirection(screenSpace, starSelectBounds);

			GD.Print(dir);

			if (dir.X != 0)
			{
				clampedPos.X = dir.X > 0 ?
					screenSpace.Position.X + sidePadding :
					screenSpace.End.X - starSelect.Size.X - sidePadding;
			}

			if (dir.Y != 0)
			{
				clampedPos.Y = dir.Y > 0 ?
					screenSpace.Position.Y + sidePadding :
					screenSpace.End.Y - starSelect.Size.Y - sidePadding;
			}
		}

		return clampedPos;
	}

	private bool IsFullyVisible(Rect2 checker, Rect2 target)
	{
		return checker.Encloses(target);
	}

	/// <summary>
	/// Calculates the closest direction from the <c>from</c> rectangle to the <c>to</c> rectangle.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns>A non-normalized <c>Vector2</c> of the closest direction</returns>
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

	/// <summary>
	/// Sets the star to be displayed by the UI.
	/// </summary>
	/// <param name="star"></param>
	public void SetStar(SelectableStar star)
	{
		this.star = star;
		this.targetPosition = star.Position;

		starNameLabel.Text = "Star"; // todo, set actual name
		starPosLabel.Text = star.Position.ToString("F2");
		starSeed.Text = star.GetSeed().ToString();

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
