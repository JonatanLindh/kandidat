using Godot;
using System;

/// <summary>
/// A class for creating different types of noise to be used when generating planets
/// </summary>
[Tool]
public partial class PlanetNoise : Node, CelestialBodyNoise, ModifiableCelestialBody
{
    private NoiseTexture3D texture3d;
    private CelestialBodyInput Input;
    private CelestialBodyInput input;
    private FastNoiseLite fastNoise;

    public override void _Ready()
    {
        Input = GetParent<CelestialBodyInput>();
        if (Input == null)
        {
            GD.PrintErr("Input is null");
        }
        input = Input as CelestialBodyInput;
    }

    public float[,,] CreateDataPoints()
    {
        int radius = input.GetRadius();
        int diameter = 2 * radius;

        int width = input.GetWidth();  
        int height = input.GetHeight(); 
        int depth = input.GetDepth();

        float[,,] points = new float[width, height, depth];
        Vector3 centerPoint = new Vector3I(radius, radius, radius);
        
        fastNoise = new FastNoiseLite()
        {
            NoiseType = input.GetNoiseType(),
            Seed = input.GetSeed()
        };

         // creates a cube of points
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 for (int z = 0; z < depth; z++)
                 {
                    // Pad the boarders of the cube with empty space so marching cubes correctly generates the mesh at the edges
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1 || z == 0 || z == depth - 1)
                    {
                        points[x, y, z] = -1.0f;
                        continue;
                    }

                    // Calculate distance from center of planet to the point (x,y,z)
                    Vector3 currentPosition = new Vector3I(x, y, z);
                    float distanceToCenter = (centerPoint - currentPosition).Length();
                    float distanceAwayFromCenter = (float)radius - distanceToCenter;

                    // Apply fbm to layer noise
                    float value = Fbm(distanceAwayFromCenter, currentPosition);

                    // Update point (x,y,z) with value from fbm
                    points[x, y, z] = value;
                }
            }
         }
        
        return points;
    }

    /// <summary>
    /// Fractal Brownian Motion - applies layers of noise to value
    /// </summary>
    /// <param name="input"></param>
    /// <param name="value"></param>
    /// <param name="currentPosition"></param>
    /// <returns>
    /// The sum of all noise layers added to value
    /// </returns>
    private float Fbm(float value, Vector3 currentPosition)
    {
        float valueAfterFbm = value;

        // Get parameters from editor which will change locally in the loop
        float amplitude = input.GetAmplitude();
        float frequency = input.GetFrequency();

        // Used to slightly offset the position when getting noise-value for each octave
        Vector3 offset = Vector3.Zero;
        Random random = new Random();

        // FBM - Fractal Brownian Motion 
        for (int i = 0; i < input.GetOctaves(); i++)
        {
            // TODO Add offset before or after *frequency?
            valueAfterFbm += fastNoise.GetNoise3Dv(frequency * currentPosition + offset) * amplitude;
            amplitude *= input.GetPersistence();
            frequency *= input.GetLacunarity();
            offset += new Vector3(random.Next(input.GetOctaves()), random.Next(input.GetOctaves()), random.Next(input.GetOctaves()));
        }

        return valueAfterFbm;
    }

    public ImageTexture3D Get3DNoiseTexture(FastNoiseLite fastNoise, int width, int height, int depth, bool useMipmaps)
    {
        // Create Slices Of Noise
        Godot.Collections.Array<Image> noiseSlices = new Godot.Collections.Array<Image>();

        for (int sliceDepth = 0; sliceDepth < depth; sliceDepth++)
        {
            // A new 2D image with size diameter x diameter, not inverted, is in3Dspace and is normalized
            Image image = fastNoise.GetImage(width, height, false, true, true);
            noiseSlices.Add(image);
            fastNoise.Offset += Vector3.Back; // (0, 0, 1) - move the noise from GetImage along the z-axis 
        }
        ImageTexture3D texture = new ImageTexture3D();
        texture.Create(noiseSlices[0].GetFormat(), width, height, depth, useMipmaps, noiseSlices);
        return texture;
    }

    public float[,,] GetNoise()
    {
        return CreateDataPoints();
    }

    public int GetRadius()
    {
        return input.GetRadius();
    }
    public void SetRadius(int newRadius)
    {
        input.SetRadius(newRadius);
    }

    public void SetWidth(int newWidth)
    {
        input.SetWidth(newWidth);
    }

    public void SetHeight(int newHeight)
    {
        input.SetHeight(newHeight);
    }

    public void SetDepth(int newDepth)
    {
        input.SetDepth(newDepth);
    }

    public void SetSize(int size)
    {
        SetWidth(size);
        SetHeight(size);
        SetDepth(size);
    }

    public void SetOctaves(int newOctaves)
    {
        input.SetOctaves(newOctaves);
    }

    public void SetFrequency(float newFrequency)
    {
        input.SetFrequency(newFrequency);
    }

    public void SetAmplitude(float newAmplitude)
    {
        input.SetAmplitude(newAmplitude);
    }

    public void SetLacunarity(float newLacunarity)
    {
        input.SetLacunarity(newLacunarity);
    }

    public void SetPersistence(float newPersistence)
    {
        input.SetPersistence(newPersistence);
    }

    public void SetNoiseType(FastNoiseLite.NoiseTypeEnum newNoiseType)
    {
        input.SetNoiseType(newNoiseType);
    }

    public void SetSeed(int newSeed)
    {
        input.SetSeed(newSeed);
    }
}
