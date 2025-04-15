using Godot;
using System;

public class TrueStarAdapter
{
	/// <summary>
	/// <para>Converts the stars to a dictionary format for physics data.</para>
	/// The dictionary contains the following:
	/// <code>
	/// Dictionary dict = New Dictionary {
	///		{"position", Vector3[]},
	///		{"velocity", Vector3[]},
	///		{"mass", float[]}
	/// }
	/// </code>
	/// </summary>
	/// <param name="stars">An array of TrueStar objects to be converted.</param>
	/// <returns>A dictionary containing the physics data of the stars.</returns>
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
