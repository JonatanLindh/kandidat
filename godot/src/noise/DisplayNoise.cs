/*using Godot;
using System.Collections.Generic;

[Tool]
public partial class DisplayNoise : Node
{
    private bool _reload;
    [Export]
    public bool reload
    {
        get => _reload;
        set
        {
            _reload = !value;
            UpdateNoise();
        }
    }

    public override void _Ready()
    {
        Display(0,0,0);
    }

    private void UpdateNoise()
    {
        GD.Print("Updated noise");
        PlanetNoise n = new PlanetNoise();
        GD.Print(n);

        float[,,] datapoints = n.GetSphere(64);
        for (var x = 0; x < datapoints.GetLength(0) - 1; x++)
        {
            for (var y = 0; y < datapoints.GetLength(1) - 1; y++)
            {
                for (var z = 0; z < datapoints.GetLength(2) - 1; z++)
                {
                    Display(x, y, z);
                }
            }
        }
    }

    private void Display(int x, int y, int z)
    {
        MeshInstance3D node = new MeshInstance3D();
        SphereMesh sphere = new SphereMesh();
        var material = new OrmMaterial3D();

        node.Mesh = sphere;
        node.Translate(new Vector3(x, y, z));
        node.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

        sphere.Radius = 0.1f;
        sphere.Height = 0.1f * 2.0f;
        sphere.Material = material;

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
        material.AlbedoColor = Colors.YellowGreen;

        this.AddChild(node);
    }
}
*/