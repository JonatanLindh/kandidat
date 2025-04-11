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
		Godot.Collections.Array<Transform3D> transform = new Godot.Collections.Array<Transform3D>();
		Godot.Collections.Array<Vector3> velocity = new Godot.Collections.Array<Vector3>();
		Godot.Collections.Array<float> mass = new Godot.Collections.Array<float>();

		foreach (var star in stars)
		{
			transform.Add(star.transform);
			velocity.Add(star.velocity);
			mass.Add(star.mass);
		}

		Godot.Collections.Dictionary dict = new Godot.Collections.Dictionary
		{
			{ "transform", transform },
			{ "velocity", velocity },
			{ "mass", mass }
		};

		return dict;
	}
}
