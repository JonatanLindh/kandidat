using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;

[Tool]
public partial class PlanetMarchingCube : Node3D
{
    [ExportToolButton("Click me!")]
    public Callable ClickMeButton => Callable.From(SpawnMesh);
    
    [Export]
    public Vector3 SunPosition
    {
        get => _sunPosition;
        set
        {
            _sunPosition = value;
            CallDeferred("SetAtmosphereSunDir");
        }
    }

    [Export]
    public int Resolution
    {
        get => _resolution;
        set
        {
            _resolution = value;
            OnResourceSet();
        }
    }
    
    [Export]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            OnResourceSet();
        }
    }
    
    
    private MarchingCube _marchingCube;
    private MeshInstance3D _meshInstance3D;
    private int _resolution = 16;
    private float _radius = 1;
    private Vector3 _sunPosition;
    private Node _atmosphere;

    public override void _Ready()
    {
        // Called every time the node is added to the scene.
        /*
        _marchingCube = new MarchingCube();
        var dataPoints = GenerateDataPoints();
        var meshInstance3D = _marchingCube.GenerateMesh(dataPoints);
        meshInstance3D.Translate(Vector3.Zero);
        meshInstance3D.Scale = Vector3.One * (1 / (float)Resolution);
        AddChild(meshInstance3D);
        */
        SpawnMesh();
    }

    public override void _EnterTree()
    {
        //SpawnMesh();
    }

    public override void _Process(double delta)
    {
        SetAtmosphereSunDir();
    }
    private void OnResourceSet()
    {
        SpawnMesh();
    }
    private void SpawnMesh()
    {
        if(_meshInstance3D != null) RemoveChild(_meshInstance3D);
        _marchingCube ??= new MarchingCube();

        _atmosphere = GetNodeOrNull("Atmosphere");
        //GD.Print(atmosphere.Get("radius"));
        _atmosphere?.Set("radius", _radius);
        CallDeferred("SetAtmosphereSunDir");


        float[,,] dataPoints = GenerateDataPoints();
        _meshInstance3D = _marchingCube.GenerateMesh(dataPoints);
        
        var material = new OrmMaterial3D();
        _meshInstance3D.MaterialOverride = material;
        material.CullMode = BaseMaterial3D.CullModeEnum.Back;
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
        material.AlbedoColor = Colors.YellowGreen;
        _meshInstance3D.Translate(-1 * _radius * Vector3.One);
        _meshInstance3D.Scale = Vector3.One * (1 / (float)_resolution);
        _meshInstance3D.Scale *= _radius;
        AddChild(_meshInstance3D);

    }
    
    private void SetAtmosphereSunDir()
    {
        var sunDir = (_sunPosition - GlobalPosition).Normalized();
        _atmosphere?.Set("sun_direction", sunDir);
    }
    
    private float[,,] GenerateDataPoints()
    {
        var radius = _resolution;
        var diameter = radius * 2 + 1;
        var dataPoints = new float[diameter, diameter, diameter];
		
        Vector3 center = new Vector3(radius, radius, radius);
		
        Parallel.For(0, diameter, x =>
        {
            for (var y = 0; y < diameter; y++)
            {
                for (var z = 0; z < diameter; z++)
                {
                    Vector3 point = new Vector3(x, y, z);
                    float distance = center.DistanceTo(point);
				
                    // Adjust threshold to match radius more precisely
                    // Using radius - 0.5 creates a more visually accurate sphere
                    float value = (radius - 0.5f) - distance;
				
                    // Clamp to ensure values stay in [-1, 1] range
                    value = Mathf.Clamp(value, -1.0f, 1.0f);
				
                    dataPoints[x, y, z] = value;
                }
            }
        });
        return dataPoints;
    }
}
