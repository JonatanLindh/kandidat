using Godot;
using System;

public partial class Galaxy : Node3D
{
    float breadth = 1000;
    float centerPull = 2f;

    int starCount = 10000;
    [Export] PackedScene starScene;

    float visibleBeginDistance = 50;
    float visibleEndDistance = 700; // To lessen distant stars flickering

    public override void _Ready()
    {
        Generate();
    }

    public override void _Process(double delta)
    {

    }

    public void Generate()
    {
        for (int i = 0; i < starCount; i++)
        {
            MeshInstance3D star = (MeshInstance3D) starScene.Instantiate();

            star.VisibilityRangeBegin = visibleBeginDistance;
            star.VisibilityRangeEnd = visibleEndDistance;

            star.Position = new Vector3(
                (float)GD.RandRange(-breadth, breadth),
                (float)GD.RandRange(-breadth, breadth),
                (float)GD.RandRange(-breadth, breadth)
            );

            // weight stars towards center
            float distanceFromCenter = (float)GD.Randf() * breadth * centerPull;
            star.Position = star.Position.Normalized() * distanceFromCenter;

            AddChild(star);
        }
    }
}
