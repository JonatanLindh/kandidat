using Godot;
using System;

public partial class StarChunk : Node3D
{
    [Export] FastNoiseLite starNoise;
    [Export] FastNoiseLite starOffsetNoise;

    [Export] PackedScene starScene;

    float ISOlevel = 0.3f;
    int minDistance = 100;

    float offsetStrength = 300f;

    int size;
    ChunkCoord pos;

    // project global seed
    const int SEED = 0;

    public override void _Ready()
    {
        // temp random seed
        Random r = new Random();
        int seed = r.Next();

        starNoise.Seed = seed;
        starOffsetNoise.Seed = seed;
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
                    float noiseVal = starNoise.GetNoise3Dv(point);

                    if (ISOlevel < noiseVal)
                    {
                        MeshInstance3D star = (MeshInstance3D)starScene.Instantiate();
                        star.Position = point + NoisePositionOffset(point) + ChunkPositionOffset();
                        AddChild(star);
                    }
                }
            }
        }
    }

    private Vector3 NoisePositionOffset(Vector3 basePos)
    {
        // Offsets star positions based on noise, sampled at basePos + an arbitrary offset, to reduce grid-like patterns
        return new Vector3(
            starOffsetNoise.GetNoise3Dv(basePos + Vector3.Left * 100) * offsetStrength,
            starOffsetNoise.GetNoise3Dv(basePos + Vector3.Up * 200) * offsetStrength,
            starOffsetNoise.GetNoise3Dv(basePos + Vector3.Right * 300) * offsetStrength
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

    public ChunkCoord GetPos()
    {
        return this.pos;
    }
}
