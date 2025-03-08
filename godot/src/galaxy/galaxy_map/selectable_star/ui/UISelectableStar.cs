using Godot;
using System;

public partial class UISelectableStar : CanvasLayer
{
    SelectableStar star;

    Node3D player;

    bool isTraveling = false;
    Vector3 targetPosition;
    float travelSpeed = 500.0f;
    float travelDistance = 10.0f;

    [Export] Control control;

    [Export] Label starNameLabel;
    [Export] Label starPosLabel;
    [Export] Label starSeed;

    [Export] Button closeButton;
    [Export] Button visitButton;

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
            player.Position = player.Position.Lerp(targetPosition, 0.01f);

            if (player.Position.DistanceTo(targetPosition) < travelDistance)
            {
                isTraveling = false;
            }
        }
    }

    public void SetStar(SelectableStar star)
    {
        this.star = star;
        this.targetPosition = star.Position;

        starNameLabel.Text = "Star"; // todo, set actual name
        starPosLabel.Text = star.Position.ToString();
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
        if (star != null) star.LoadSolarSystem();
    }

    public void OnTravelButtonPressed()
    {
        if (player != null)
        {
            isTraveling = true;
        }
    }
}
