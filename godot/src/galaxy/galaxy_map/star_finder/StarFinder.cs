using Godot;
using System;

public partial class StarFinder : Node
{
	public InfiniteGalaxy galaxy { private get; set; }

	/// <summary>
	/// Finds along a line from one point to another.
	/// Checking at set intervals with a given radius.
	/// 
	/// radius should probably equal interval to avoid most misses
	/// </summary>
	public Star FindStar(Vector3 from, Vector3 to, float radius, float interval)
	{
		IStarChunkData[] chunks = galaxy.GetGeneratedChunks();
		IStarChunkData currentChunk = chunks[0];

		// TODO get actual star
		//

		//ChunkCoord pos = ChunkCoord.ToChunkCoord(currentChunk.size, from);
		//GD.Print(pos.ToString());

		Star star = CreateStar(currentChunk.stars[0], galaxy.GetSeed());
		return star;
	}

	private Star CreateStar(Vector3 position, uint seed)
	{
		// TODO
		// String starName = ...
		Transform3D starTransform = new Transform3D(Basis.Identity, position);

		SeedGenerator seedGen = new SeedGenerator();
		uint starSeed = seedGen.GenerateSeed(galaxy.GetSeed(), position);

		Star star = new Star(starTransform, starSeed);
		return star;
	}
}
