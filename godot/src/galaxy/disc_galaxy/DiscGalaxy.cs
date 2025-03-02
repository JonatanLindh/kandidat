using Godot;
using System;

public partial class DiscGalaxy : Node3D
{
    [Export] FastNoiseLite noise;

    int starCount = 10000;
    [Export] PackedScene starScene;

    float rotationSpeed = 0.05f;

    float baseISOLevel = 0.5f;
    float radius = 1000;

    [Export] int seed;
    Random random;

    public override void _Ready()
    {
        // Sets a random seed if no seed is provided
        if (seed == 0) seed = new Random().Next();
        
        random = new Random(seed);
        noise.Seed = seed;

        Generate();
    }

    public override void _Process(double delta)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        RotateY((float)delta * rotationSpeed);
    }

    private void Generate()
    {
        int starsGenerated = 0;

        while (starsGenerated < starCount)
        {
            Vector3 point = SamplePointInSphere();
            float noiseVal = noise.GetNoise3D(point.X, point.Y, point.Z);

            if (GetISOLevel(point) > noiseVal)
            {
                MeshInstance3D star = (MeshInstance3D)starScene.Instantiate();
                star.Position = point;
                AddChild(star);
                starsGenerated++;
            }
        }
    }

    private Vector3 SamplePointInSphere()
    {
        double u = random.NextDouble();
        double v = random.NextDouble();
        double w = random.NextDouble();

        double theta = 2 * Math.PI * u;
        double phi = Math.Acos(2 * v - 1);
        double r = radius * w;
        //double r = radius * Math.Cbrt(w); // uniform distribution

        double x = r * Math.Sin(phi) * Math.Cos(theta);
        double y = r * Math.Sin(phi) * Math.Sin(theta);
        double z = r * Math.Cos(phi);

        return new Vector3((float)x, (float)y, (float)z);
    }

    private float GetISOLevel(Vector3 pos)
    {
        float yDistFromCenter = Math.Abs(pos.Y);
        float iso = baseISOLevel - yDistFromCenter * 0.005f;
        return iso;
    }
}
