using Godot;
using System;

public partial class StarChunk : Node3D
{
    [Export] FastNoiseLite starNoise;
    [Export] PackedScene starScene;

    float ISOlevel = 0.8f;
    int minDistance = 100;

    float offsetStrength = 100f;

    int size;
    ChunkCoord pos;

    // project global seed
    const int SEED = 0;

    public override void _Ready()
    {
        // temp random seed
        Random r = new Random();
        starNoise.Seed = r.Next();
        GD.Print(starNoise.Seed);
    }

    public void Generate(int size, ChunkCoord pos)
    {
        this.size = size;
        this.pos = pos;

        for (int x = 0; x < size; x += minDistance)
        {
            for (int y = 0; y < size; y += minDistance)
            {
                for (int z = 0; z < size; z += minDistance)
                {
                    Vector3 point = new Vector3(x, y, z);
                    float noiseVal = starNoise.GetNoise3D(point.X, point.Y, point.Z);

                    if (ISOlevel > noiseVal)
                    {
                        MeshInstance3D star = (MeshInstance3D)starScene.Instantiate();
                        star.Position = point + NoisePositionOffset(point, noiseVal) + ChunkPositionOffset();
                        AddChild(star);
                    }
                }
            }
        }
    }

    private Vector3 NoisePositionOffset(Vector3 basePos, float noiseVal)
    {
        return new Vector3(
            basePos.X + (offsetStrength * noiseVal),
            basePos.Y + (offsetStrength * noiseVal),
            basePos.Z + (offsetStrength * noiseVal)
        );
    }

    private Vector3 ChunkPositionOffset()
    {
        return new Vector3(
            this.pos.x * size,
            this.pos.y * size,
            this.pos.z * size
        );
    }
}
