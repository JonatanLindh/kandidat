using Godot;
using System;

[Tool]
public partial class TreeTest : Node3D
{
	[Export]
	public MeshInstance3D MeshInstance { get; set; }

	[Export] public float Scale { get; set; } = 10f;
	
	[Export] public int AmountPerSide { get; set; } = 50;
	
	
	private Node3D _parentNode;


	public override void _Ready()
	{
		if (MeshInstance == null) return;
		GenTree genTree = new GenTree(AmountPerSide, Scale);
		_parentNode?.QueueFree();
		_parentNode = new Node3D();
		AddChild(_parentNode);
		Aabb bounds = MeshInstance.GetAabb();
		AddChild(genTree.SpawnTrees(bounds, GetWorld3D().DirectSpaceState));
	}
}
