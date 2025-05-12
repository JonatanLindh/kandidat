using Godot;
using System;

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
        int radius = param.Radius - 1;
        int diameter = 2 * radius;

        int width = param.Width;  
        int height = param.Height;
        int depth = param.Depth;

        float[,,] points = new float[width, height, depth];
        Vector3 centerPoint = Vector3I.Zero;
        
        //if (offset == default)
          //  offset = Vector3.One * -radius;

         // creates a cube of points
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 for (int z = 0; z < depth; z++)
                 {
                    // Calculate distance from center of planet to the point (x,y,z)
                    Vector3 currentPosition = new Vector3(x, y, z) * voxelSize;
                    currentPosition += offset;

                    // Pad the borders of the planet with empty space so marching cubes correctly generates the mesh at the edges
                    if (currentPosition.X <= -radius || currentPosition.X >= radius ||
                        currentPosition.Y <= -radius || currentPosition.Y >= radius ||
                        currentPosition.Z <= -radius || currentPosition.Z >= radius)
                    {
                        points[x, y, z] = -1.0f;
                        continue;
                    }
                    
                    float distanceToCenter = (centerPoint - currentPosition).Length();
                    float distanceAwayFromCenter = (float)radius - distanceToCenter;

                    // Apply fbm to layer noise
                    float value = Fbm(distanceAwayFromCenter, currentPosition, param, fastNoise);

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
    private float Fbm(float value, Vector3 currentPosition, CelestialBodyParameters param, FastNoiseLite fastNoise)
    {
        float valueAfterFbm = value;

        // Get parameters from editor which will change locally in the loop
        float amplitude = param.Amplitude;
        float frequency = param.Frequency;

        // Used to slightly offset the position when getting noise-value for each octave
        Vector3 offset = Vector3.Zero;
        Random random = new Random(param.Seed);

        // FBM - Fractal Brownian Motion 
        for (int i = 0; i < param.Octaves; i++)
        {
            // TODO Add offset before or after *frequency?
            valueAfterFbm += fastNoise.GetNoise3Dv(frequency * currentPosition + offset) * amplitude;
            amplitude *= param.Persistence;
            frequency *= param.Frequency;
            offset += new Vector3(random.Next(param.Octaves), random.Next(param.Octaves), random.Next(param.Octaves));
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
}
