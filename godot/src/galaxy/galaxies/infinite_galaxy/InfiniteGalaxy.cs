using Godot;
using System;
using System.Collections.Generic;

public partial class InfiniteGalaxy : Node3D
{
	[Export] Mesh starMesh;
	[Export] FastNoiseLite noise;
	[Export] Node3D player;

	ChunkCoord playerChunkCoord;
	StarChunk playerChunk;

	private List<StarChunk> starChunks;
	[Export] uint seed;

	[ExportCategory("Chunks")]
	[Export] PackedScene starChunk;
	[Export(PropertyHint.Range, "1, 10, 1")] int viewDistance = 1;
	[Export(PropertyHint.Range, "-1, 1, 0.01")] float IsoLevel = 0.3f;
	[Export] int chunkSize = 1000;
	[Export] int starCount = 1000;
	[Export] float minimumDistance = 5000f;

	[ExportCategory("Color")]
	[Export] bool colorStars = true;

	public override void _Ready()
	{
		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint) new Random().Next();

		starChunks = new List<StarChunk>();
	}

	public override void _Process(double delta)
	{
		if (player == null)
		{
			GD.PrintErr("Player object is null.");
			return;
		}

		playerChunkCoord = ChunkCoord.ToChunkCoord(chunkSize, player.Position);

		for (int x = -viewDistance; x <= viewDistance; x++)
		{
			for (int y = -viewDistance; y <= viewDistance; y++)
			{
				for (int z = -viewDistance; z <= viewDistance; z++)
				{
					ChunkCoord chunkPos = new ChunkCoord(playerChunkCoord.x + x, playerChunkCoord.y + y, playerChunkCoord.z + z);
					if (!IsChunkGenerated(chunkPos))
					{
						GenerateChunk(chunkPos);
					}
				}
			}
		}

		UpdatePlayerChunk(playerChunkCoord);
		CullChunks(playerChunkCoord);
	}

	private void GenerateChunk(ChunkCoord pos)
	{
		StarChunk chunk = (StarChunk) starChunk.Instantiate();
		chunk.Name = "Chunk (" + pos.x + ", " + pos.y + ", " + pos.z + ")";

		chunk.starMesh = starMesh;
		chunk.galaxyNoise = noise;

		chunk.Generate(seed, chunkSize, starCount, IsoLevel, pos, colorStars, minimumDistance);

		starChunks.Add(chunk);
		AddChild(chunk);
	}

	private bool IsChunkGenerated(ChunkCoord chunk)
	{
		foreach (StarChunk c in starChunks)
		{
			if (c.pos.Equals(chunk))
			{
				return true;
			}
		}

		return false;
	}

	private void CullChunks(ChunkCoord playerChunk)
	{
		for(int i = starChunks.Count - 1; i >= 0; i--)
		{
			StarChunk chunk = starChunks[i];
			if (Math.Abs(chunk.pos.x - playerChunk.x) > viewDistance || 
				Math.Abs(chunk.pos.y - playerChunk.y) > viewDistance || 
				Math.Abs(chunk.pos.z - playerChunk.z) > viewDistance)
			{
				starChunks.RemoveAt(i);
				chunk.QueueFree();
			}
		}
	}

	public IStarChunkData[] GetGeneratedChunks()
	{
		return starChunks.ToArray();
	}

	private void UpdatePlayerChunk(ChunkCoord playerPos)
	{
		foreach (StarChunk chunk in starChunks)
		{
			if (chunk.pos.Equals(playerChunkCoord))
			{
				this.playerChunk = chunk;
			}
		}
	}

	public StarChunk GetPlayerChunk()
	{
		return this.playerChunk;
	}

	public uint GetSeed()
	{
		return seed;
	}

	public void SetSeed(uint seed)
	{
		this.seed = seed;
	}

	public void SetPlayer(Node3D player)
	{
		this.player = player;
	}
}
