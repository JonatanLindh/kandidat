using Godot;
using System;
using System.Collections.Generic;

public partial class Skybox : Node3D
{
    [Export] float size = 1000;

    [Export] PackedScene starScene;
    [Export] int starCount = 10000;

    [Export] int removeStarsTooCloseDistance = 500;

    // should be project global seed
    const int SEED = 0;

    public override void _Ready()
    {
        GD.Seed(SEED);
        Generate();
    }

    public void Generate()
    {
        for (int i = 0; i < starCount; i++)
        {
            Vector3 starPos = new Vector3(
                (float)GD.RandRange(-size, size),
                (float)GD.RandRange(-size, size),
                (float)GD.RandRange(-size, size)
            );

            // remove stars that are too close to the center
            if (this.Position.DistanceTo(starPos) < removeStarsTooCloseDistance)
            {
                continue;
            }

            MeshInstance3D star = (MeshInstance3D)starScene.Instantiate();
            star.Position = starPos;

            AddChild(star);
        }
    }
}
