using Godot;
using System;

public interface IStarChunkData
{
	ChunkCoord pos { get; }
	int size { get; }
	Vector3[] stars { get; }
}
