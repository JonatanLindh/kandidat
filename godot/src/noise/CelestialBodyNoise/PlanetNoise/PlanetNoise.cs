using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// A class for creating different types of noise to be used when generating planets
/// </summary>
[Tool]
public partial class PlanetNoise
{
    private NoiseTexture3D texture3d;

    public PlanetNoise() {}

    public float[,,] CreateDataPoints(CelestialBodyParameters param, FastNoiseLite fastNoise, Vector3 offset = default, float voxelSize = 1)
    {
        int radius = param.Radius;
        int diameter = 2 * radius;

        int width = param.Width;  
        int height = param.Height;
        int depth = param.Depth;

        float[,,] points = new float[width, height, depth];
        Vector3 centerPoint = Vector3I.Zero;
        
        //if (offset == default)
          //  offset = Vector3.One * -radius;

        float falloffStrength = param.FalloffStrength;

        // Pad the boarders of the points-array with empty space so marching cubes correctly generates the mesh at the edges
        //PadBordersWithAir(points, width, height, depth);

        // Boarders are already padded, so only need to iterate from [1, size-1)
        Parallel.For(0, width, x =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // Calculate distance from center of planet to the point (x,y,z)
                    Vector3 currentPoint = new Vector3(x, y, z) * voxelSize;
                    currentPoint += offset;
                    float distanceToCenter = (centerPoint - currentPoint).Length();
                    float distanceToBorder = (float)radius - distanceToCenter;

                    // Apply fbm to layer noise
                    float value = Fbm(distanceToBorder, currentPoint, param, fastNoise);

                    // if > 1   --> the point is outside the planet
                    // if <= 1  --> the point is inside the planet
                    // Used for calculating the amount of falloff applied to the value
                    float falloffRatio = Mathf.Abs(distanceToCenter / (float)radius);

                    // Exponential falloff based on the ratio between the radius and the distance from the centerPoint
                    // Values outside the planet gets larger (falloffRatio > 1) and
                    // values within the planet gets smaller (falloffRatio <= 1)
                    float falloff = falloffRatio * falloffRatio * falloffStrength;

                    points[x, y, z] = value - falloff;
                }
            }
        });

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
    private float Fbm(float value, Vector3 currentPosition, CelestialBodyParameters param, FastNoiseLite fastNoise)
    {
        float valueAfterFbm = value;

        // Get parameters from editor which will change locally in the loop.
        // param's parameters should not change at this point as each planet has its own CelestialBodyParameters instance, so it's thread-safe
        float amplitude = param.Amplitude;
        float frequency = param.Frequency;
        float persistence = param.Persistence;
        float lacunarity = param.Lacunarity;

        float h = Mathf.Pow(2, persistence);

        // Used to slightly offset the position when getting noise-value for each octave
        Vector3 offset = Vector3.Zero;
        Random random = new Random(param.Seed);

        // FBM - Fractal Brownian Motion 
        for (int i = 0; i < param.Octaves; i++)
        {
            // FastNoiseLite.GetNoise3DV should be thread-safe if not changed while executing the Parallel.For-loop
            valueAfterFbm += fastNoise.GetNoise3Dv(frequency * currentPosition + offset) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
            offset += new Vector3(random.Next(param.Octaves), random.Next(param.Octaves), random.Next(param.Octaves));
        }

        return valueAfterFbm;
    }

    /// <summary>
    /// Pads the array with "air" (-1.0) at the edges of the array, i.e. x,y,z = 0 & x,y,z = size-1. Directly modifies the given array.
    /// </summary>
    /// <param name="arrayToPadWithAir"></param>
    /// <param name="size"></param>
    private static void PadBordersWithAir(float[,,] arrayToPadWithAir, int width, int height, int depth)
    {
        int widthEdge = width - 1;
        for (int y = 0; y < height; y++)
        {
            for(int z = 0; z < depth; z++)
            {
                arrayToPadWithAir[0, y, z] = -1.0f;
                arrayToPadWithAir[widthEdge, y, z] = -1.0f;
            }
        }

        int heightEdge = height - 1;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                arrayToPadWithAir[x, 0, z] = -1.0f;
                arrayToPadWithAir[x, heightEdge, z] = -1.0f;
            }
        }

        int depthEdge = depth - 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                arrayToPadWithAir[x, y, 0] = -1.0f;
                arrayToPadWithAir[x, y, depthEdge] = -1.0f;
            }
        }
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
}
