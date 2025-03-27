using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A class for creating different types of noise to be used when generating planets
/// </summary>
[Tool]
public partial class PlanetNoise : Node, CelestialBodyNoise
{
    private NoiseTexture3D texture3d;
    private CelestialBodyInput Input;
    private CelestialBodyInput input;
    private FastNoiseLite fastNoise;

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

        // Get parameters from editor
        float amplitude = input.GetAmplitude();
        float persistence = input.GetPersistence();
        float frequency = input.GetFrequency();
        float lacunarity = input.GetLacunarity();
        int octaves = input.GetOctaves();

        // Used to slightly offset the position when getting noise-value for each octave
        Vector3 offset = Vector3.Zero;
        Random random = new Random();

        // FBM - Fractal Brownian Motion 
        for (int i = 0; i < octaves; i++)
        {
            // TODO Add offset before or after *frequency?
            valueAfterFbm += fastNoise.GetNoise3Dv(frequency * currentPosition + offset) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
            offset += new Vector3(random.Next(octaves), random.Next(octaves), random.Next(octaves));
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

    private void RandomizeParameters()
    {
        RandomPlanet random = new RandomPlanet();

        input.SetRadius(random.GetRadius());
        input.SetSize(random.GetSize());

        input.SetOctaves(random.GetOctaves());
        input.SetSeed(random.GetSeed());

        input.SetAmplitude(random.GetAmplitude());
        input.SetPersistence(random.GetPersistence());
        input.SetFrequency(random.GetFrequency());
        input.SetLacunarity(random.GetLacunarity());

        input.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
    }

    public float[,,] GetNoise(bool useRandomGeneration)
    {
        Input = GetParent<CelestialBodyInput>();
        if (Input is not CelestialBodyInput)
        {
            throw new Exception("Input is not CelestialBodyInput");
        }
        input = Input as CelestialBodyInput;

        if (useRandomGeneration)
        {
            RandomizeParameters();
        }

        return CreateDataPoints();
    }
}
