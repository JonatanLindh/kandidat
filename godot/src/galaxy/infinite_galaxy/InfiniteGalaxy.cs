using Godot;
using System;
using System.Collections.Generic;

public partial class InfiniteGalaxy : Node3D
{
    [Export] PackedScene starChunk;
    [Export] Node3D player;

    private List<StarChunk> starChunks;
    int chunkSize = 1000;

    int chunkDistance = 1;

    [Export] int seed;

    [Export] UISelectableStar selectedStarUI;

    public override void _Ready()
    {
        // Sets a random seed if no seed is provided
        if (seed == 0) seed = new Random().Next();

        starChunks = new List<StarChunk>();
    }

    public override void _Process(double delta)
    {
        ChunkCoord playerChunk = ChunkCoord.ToChunkCoord(chunkSize, player.Position);

        for (int x = -chunkDistance; x <= chunkDistance; x++)
        {
            for (int y = -chunkDistance; y <= chunkDistance; y++)
            {
                for (int z = -chunkDistance; z <= chunkDistance; z++)
                {
                    ChunkCoord chunk = new ChunkCoord(playerChunk.x + x, playerChunk.y + y, playerChunk.z + z);
                    if (!IsChunkGenerated(chunk))
                    {
                        GenerateChunk(chunk);
                    }
                }
            }
        }

        CullChunks(playerChunk);
    }

    private void GenerateChunk(ChunkCoord pos)
    {
        StarChunk chunk = (StarChunk) starChunk.Instantiate();
        chunk.Name = "Chunk (" + pos.x + ", " + pos.y + ", " + pos.z + ")";
        chunk.Generate(seed, chunkSize, pos);

        starChunks.Add(chunk);
        AddChild(chunk);
    }

    private bool IsChunkGenerated(ChunkCoord chunk)
    {
        foreach (StarChunk c in starChunks)
        {
            if (c.GetPos().Equals(chunk))
            {
                return true;
            }
        }

        return false;
    }

    private void CullChunks(ChunkCoord playerChunk)
    {
        for(int i = 0; i < starChunks.Count; i++)
        {
            StarChunk chunk = starChunks[i];
            if (Math.Abs(chunk.GetPos().x - playerChunk.x) > chunkDistance || 
                Math.Abs(chunk.GetPos().y - playerChunk.y) > chunkDistance || 
                Math.Abs(chunk.GetPos().z - playerChunk.z) > chunkDistance)
            {
                starChunks.Remove(chunk);
                chunk.QueueFree();
            }
        }
    }

    public void SelectedStarUI(SelectableStar star)
    {
        selectedStarUI.SetStar(star);
    }
}
