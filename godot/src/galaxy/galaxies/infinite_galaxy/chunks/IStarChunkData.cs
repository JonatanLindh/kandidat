using Godot;
using System;

public interface IStarChunkData
{
	ChunkCoord pos { get; }
	int size { get; }
	Transform3D[] stars { get; }
}
