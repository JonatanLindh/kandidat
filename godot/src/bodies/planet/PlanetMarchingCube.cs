using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;

[Tool]
public partial class PlanetMarchingCube : Node3D
{
	[ExportToolButton("Regenerate Mesh")]
	public Callable ClickMeButton => Callable.From(RegenerateMesh);
	
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
		}
	}
	
	[Export]
	public float Radius
	{
		get => _radius;
		set
		{
			_radius = value;
		}
	}
	[Export]
	public int Seed
	{
		get => _seed;
		set
		{
			_seed = value;
		}
	}

	[Export] public PackedScene Planet { get; set; }

    [Export(PropertyHint.Range, "0,1,0.1")]
    public double Warmth 
	{ 
		get => warmth; 
		set
		{
			warmth = value;
		}
	}
	private double warmth;




    private MarchingCube _marchingCube;
	private int _resolution = 16;
	private float _radius = 1;
	private int _seed = 0;
	private Vector3 _sunPosition;
	private Node3D _planet;
	private Node _atmosphere;
	private Area3D _planet_gravity_field;
	private PlanetThemeGenerator _themeGenerator;

	public override void _Ready()
	{
		_planet_gravity_field = GetNode<Area3D>("PlanetGravityField");
		_planet_gravity_field.Set("radius", _radius);
		CallDeferred(nameof(SpawnMesh));
    }

	public override void _Process(double delta)
	{
		SetAtmosphereSunDir();
	}

    /// <summary>
    /// Will regenerate the mesh of this planet.
    /// 
    /// Be careful when calling this method, it may result in planets generating multiple times depending on from where you call it. For instance, if called
    /// from a setter of an exported variable, there is a high likelihood that this bug willl occur. Be sure to check while running the main scene.
    /// </summary>
    private void RegenerateMesh()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		_themeGenerator = new PlanetThemeGenerator();
        _themeGenerator.Seed = _seed;
        _themeGenerator.Warmth = warmth;

		// Check if there's already a planet instance and remove it
		if (_planet != null && IsInstanceValid(_planet))
		{
			_planet.QueueFree();
			_planet = null;
		}
		
		// Instantiate the new planet
		if (Planet != null)
		{
			// Instantiate without immediately casting
			var instance = Planet.Instantiate();
			
			// Try to cast to Node3D    
			_planet = instance as Node3D;
			
			if (_planet != null)
			{
				_planet.Set("radius", _resolution);
				_planet.Set("seed", _seed);
				
				_planet.Scale = Vector3.One * (1 / (float)_resolution) * _radius;

				AddChild(_planet);

                // Find McSpawner node and set Warmth
                var mcSpawner = _planet.GetNodeOrNull<McSpawner>("MarchingCube");
				if (mcSpawner != null)
				{
					mcSpawner.ThemeGenerator = _themeGenerator;
                    mcSpawner.Warmth = warmth;
				}
            }
		}
		
		_atmosphere = GetNodeOrNull("Atmosphere");
		_atmosphere?.Set("radius", _radius);
        _atmosphere?.Set("planet_seed", _seed);
        CallDeferred(nameof(SetAtmosphereSunDir));
	}

	private void SetAtmosphereSunDir()
	{
		if (!IsInsideTree() || _atmosphere == null)
			return;
		
		try 
		{
			var sunDir = (_sunPosition - GlobalPosition).Normalized();
			_atmosphere.Set("sun_direction", sunDir);
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error updating atmosphere: {e.Message}");
		}
	}
}
