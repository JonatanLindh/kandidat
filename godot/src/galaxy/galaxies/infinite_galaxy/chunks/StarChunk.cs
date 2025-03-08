using Godot;
using System;

public partial class StarChunk : Node3D
{
    public FastNoiseLite galaxyNoise { private get; set; }
    public PackedScene starScene { private get; set; }

    int chunkSize;
    int starCount;
    float ISOlevel;
    public ChunkCoord chunkPos { get; private set; }

    public void Generate(uint galaxySeed, int chunkSize, int starCount, float ISOlevel, ChunkCoord pos)
    {
        galaxyNoise.Seed = (int)galaxySeed;
        this.chunkSize = chunkSize;
        this.starCount = starCount;
        this.ISOlevel = ISOlevel;
        this.chunkPos = pos;

        uint placementSeed = (uint) HashCode.Combine(galaxySeed, chunkPos);
        GD.Seed(placementSeed);

        for (int i = 0; i < starCount; i++)
        {
            Vector3 localPoint = new Vector3(
                (float)GD.RandRange(0f, chunkSize),
                (float)GD.RandRange(0f, chunkSize),
                (float)GD.RandRange(0f, chunkSize)
            );

            Vector3 globalPoint = localPoint + ChunkPositionOffset();
            float noiseVal = galaxyNoise.GetNoise3Dv(globalPoint);

            if (ISOlevel < noiseVal)
            {
                Node3D star = (Node3D)starScene.Instantiate();
                star.Position = localPoint + ChunkPositionOffset();

                if (star is SelectableStar)
                {
                    SelectableStar selectableStar = (SelectableStar)star;
                    uint starSeed = SeedGenerator.GenerateSeed(galaxySeed, globalPoint);
                    selectableStar.SetSeed(starSeed);
                }

                AddChild(star);
            }
        }
    }

    private Vector3 ChunkPositionOffset()
    {
        return new Vector3(
            this.chunkPos.x * chunkSize,
            this.chunkPos.y * chunkSize,
            this.chunkPos.z * chunkSize
        );
    }
}
