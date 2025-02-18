using Godot;
using System;

public partial class Galaxy : Node3D
{
    [Export] FastNoiseLite noise;

    int breadth = 1000;
    float ISOlevel = 0.8f;
    int minDistance = 100;

    float offsetStrength = 1f;

    [Export] PackedScene starScene;

    // project global seed
    const int SEED = 0;

    public override void _Ready()
    {
        // temp random seed
        Random r = new Random();
        noise.Seed = r.Next();
        GD.Print(noise.Seed);

        Generate();
    }

	public override void _Process(double delta)
	{

    }

    private void Generate()
    {
        for (int x = 0; x < breadth; x += minDistance)
        {
            for (int y = 0; y < breadth; y += minDistance)
            {
                for (int z = 0; z < breadth; z += minDistance)
                {
                    Vector3 point = new Vector3(x, y, z);
                    float noiseVal = noise.GetNoise3D(point.X, point.Y, point.Z);

                    if (ISOlevel > noiseVal)
                    {
                        MeshInstance3D star = (MeshInstance3D)starScene.Instantiate();
                        star.Position = point + PositionOffset(point, noiseVal);
                        AddChild(star);
                    }
                }
            }
        }
    }

    private Vector3 PositionOffset(Vector3 basePos, float noiseVal)
    {
        return new Vector3(
            basePos.X * offsetStrength * noiseVal,
            basePos.Y * offsetStrength * noiseVal,
            basePos.Z * offsetStrength * noiseVal
        );
    }
}
