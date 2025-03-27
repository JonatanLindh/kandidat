using Godot;
using System;

public partial class ChunkCoord
{
	public int x;
	public int y;
	public int z;

	public ChunkCoord(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	/// <summary>
	/// Converts a global position to a chunk coordinate
	/// </summary>
	/// <param name="chunkSize"></param>
	/// <param name="globalPos"></param>
	/// <returns></returns>
	public static ChunkCoord ToChunkCoord(int chunkSize, Vector3 globalPos)
	{
		int x = Mathf.FloorToInt(globalPos.X / chunkSize);
		int y = Mathf.FloorToInt(globalPos.Y / chunkSize);
		int z = Mathf.FloorToInt(globalPos.Z / chunkSize);
		return new ChunkCoord(x, y, z);
	}

	public override bool Equals(object obj)
	{
		if (obj is ChunkCoord coord)
		{
			return (this.x == coord.x && this.y == coord.y && this.z == coord.z);
		}

		else if (obj is Vector3 vec)
		{
			return (this.x == vec.X && this.y == vec.Y && this.z == vec.Z);
		}

		return false;
	}
}
