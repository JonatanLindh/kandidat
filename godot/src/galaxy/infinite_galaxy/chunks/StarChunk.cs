using Godot;
using System;

public partial class StarChunk : Node3D
{
    [Export] FastNoiseLite starNoise;
    [Export] FastNoiseLite starOffsetNoise;

    PackedScene starScene;

    float ISOlevel = 0.3f;
    int minDistance = 100;

    float offsetStrength = 300f;

    int seed;
    int size;
    ChunkCoord pos;

    public override void _Ready()
    {

    }

    public void Generate(int seed, int size, ChunkCoord pos)
    {
        if(starScene == null)
        {
            GD.PrintErr("StarChunk: Star scene not set");
            return;
        }

        if(starNoise == null)
        {
            GD.PrintErr("StarChunk: Star noise not set");
            return;
        }

        this.seed = seed;
        starNoise.Seed = seed;
        starOffsetNoise.Seed = seed;

        this.size = size;
        this.pos = pos;

        for (int x = 0; x < size; x += minDistance)
        {
            for (int y = 0; y < size; y += minDistance)
            {
                for (int z = 0; z < size; z += minDistance)
                {
                    Vector3 localPoint = new Vector3(x, y, z);
                    Vector3 globalPoint = localPoint + ChunkPositionOffset();
                    float noiseVal = starNoise.GetNoise3Dv(globalPoint);

                    if (ISOlevel < noiseVal)
                    {
                        Node3D star = (Node3D) starScene.Instantiate();
                        star.Position = localPoint + NoisePositionOffset(globalPoint) + ChunkPositionOffset();
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

    public void SetStarScene(PackedScene star)
    {
        this.starScene = star;
    }
}
