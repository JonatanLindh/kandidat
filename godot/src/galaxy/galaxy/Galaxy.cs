using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public partial class Galaxy : Node3D
{
    [Export] PackedScene starChunk;
    [Export] Node3D player;

    private List<StarChunk> starChunks;
    int chunkSize = 1000;

    public override void _Ready()
    {
        starChunks = new List<StarChunk>();

        GenerateChunk();
    }

    public override void _PhysicsProcess(double delta)
    {

    }

    private void GenerateChunk()
    {
        StarChunk chunk = (StarChunk)starChunk.Instantiate();
        chunk.Generate(chunkSize, ChunkCoord.ToChunkCoord(chunkSize, player.Position));

        starChunks.Add(chunk);
        AddChild(chunk);

        GD.Print("Chunk (" + chunk.GetPos().x + ", " + chunk.GetPos().y + ", " + chunk.GetPos().z + ") generated");
    }
}
