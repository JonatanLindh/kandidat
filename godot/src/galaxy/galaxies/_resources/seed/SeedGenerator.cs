using Godot;
using System;

public static class SeedGenerator
{
    public static uint GenerateSeed(uint seed, Vector3 pos)
    {
        unchecked
        {
            uint hash = 17;
            hash = hash * 31 + seed;
            hash = hash * 31 + (uint)Math.Abs(pos.X);
            hash = hash * 31 + (uint)Math.Abs(pos.Y);
            hash = hash * 31 + (uint)Math.Abs(pos.Z);
            hash = hash * 31 + (uint)((pos.X + 1) * (pos.Y + 2) * (pos.Z + 3));
            return hash;
        }
    }

    public static uint GenerateSeed(uint seed, ChunkCoord pos)
    {
        Vector3 chunkPos = new Vector3(pos.x, pos.y, pos.z);
        return GenerateSeed(seed, chunkPos);
    }
}
