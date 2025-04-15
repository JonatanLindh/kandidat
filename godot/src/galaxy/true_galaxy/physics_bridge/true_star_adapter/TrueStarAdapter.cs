using Godot;
using System;

public class TrueStarAdapter
{
	/// <summary>
	/// Converts the stars to a dictionary format for physics data.
	/// </summary>
	/// <param name="stars"></param>
	/// <returns></returns>
	public Godot.Collections.Dictionary StarsToPhysicsData(TrueStar[] stars)
	{
		Vector3[] position = new Vector3[stars.Length];
		Vector3[] velocity = new Vector3[stars.Length];
		float[] mass = new float[stars.Length];

		for (int i = 0; i < stars.Length; i++)
		{
			position[i] = stars[i].transform.Origin;
			velocity[i] = stars[i].velocity;
			mass[i] = stars[i].mass;
		}

		Godot.Collections.Dictionary dict = new Godot.Collections.Dictionary
		{
			{ "position", position },
			{ "velocity", velocity },
			{ "mass", mass }
		};

		return dict;
	}
}
