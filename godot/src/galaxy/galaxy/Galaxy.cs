using Godot;
using System;
using System.Drawing;

public partial class Galaxy : Node3D
{
    [Export] PackedScene starChunk;
    [Export] Node3D player;

    int chunkSize = 100;

    public override void _Ready()
    {
        GenerateChunk();
    }

    public override void _PhysicsProcess(double delta)
    {

    }

    private void GenerateChunk()
    {
        StarChunk chunk = (StarChunk)starChunk.Instantiate();
        AddChild(chunk);


    }
}
