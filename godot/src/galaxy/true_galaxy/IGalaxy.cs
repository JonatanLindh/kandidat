using Godot;
using System;

public interface IGalaxy
{
	public uint GetSeed();
	public Vector3[] GetStarPositions();
	public void RedrawStars(Transform3D[] stars);
}
