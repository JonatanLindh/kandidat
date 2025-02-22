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


        return new ChunkCoord(0, 0, 0);
    }
}
