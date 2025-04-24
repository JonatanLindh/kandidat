using Godot;

[GlobalClass]
[Tool]
public partial class SurfaceFeature: Resource
{
	[Export] public Mesh FeatureMesh { get; set; }
	
	[Export] public float Weight { get; set; }
	
	[Export] public float Scale { get; set; }
	
	
	public SurfaceFeature() : this(null, 1f, 1f) {}
	
	public SurfaceFeature(Mesh featureMesh, float weight, float scale)
	{
		CallDeferred(nameof(Init), featureMesh, weight, scale);
	}

	private void Init(Mesh featureMesh, float weight, float scale)
	{
		FeatureMesh = featureMesh;
		Weight = weight;
		Scale = scale;
	}
}