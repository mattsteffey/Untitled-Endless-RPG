using UnityEngine;
using Unity.Mathematics;

public static class Noise
{

   // Generates the noiseMap
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, string seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
       
        // This is what will store the actual values of the perlin noise
        // Initally it stores the values of the mapWidth and Height, but that is recalculated with noise below 
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //This takes in the seed string and hashes it to an int
        int hashedSeed = seed.GetHashCode();
        // then randomizes it
        System.Random prng = new System.Random(hashedSeed);

        // this randomizes the offset of the octaves using the seed
        Vector2[] octaveOffests = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            //If you give MathF.perlin noise a number that is too high, it seems to bug and always return the same values, limiting to +-100000
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffests[i] = new Vector2(offsetX, offsetY);   
            }

        //This prevents us from dividing by zero if the scale is not set
        if (scale <= 0) {
            scale = 0.0001f;
            }

        // not totally sure why you set min and max opposite, but these are the min and max which will be normalized later to 0-1
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // These halfwidths are used instead of full widths so we can zoom the preview to the center of the noise displayed, and not in and out of a corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // This iterates through the map at each point in the heighth and width
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Here we divide X/Y by scale so we can scale the perlin
                // We also multiply each point by frequency. The higher the frequency the further apart the sample points - this makes the height values change more rapidly
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffests[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffests[i].y;

                    // This generates the noise values on each point. It takes in X and Y coordinate SAMPLE POINTS
                    // The (* 2 - 1) changes the ranges from 0f to 1f, into -1f to 1f, this will be normalized, but allows for better range in noise.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;

                    // After each run, amplitude is multiplied by persistance. Persistance is 0-1 so this decreases the intensity of the amplitude between each run.
                    amplitude *= persistance;
                    // After each run, frequency is multiplied by lacunarity. Since Lacunarity is 1+, this increases the frequency (makes waves smaller horizontally)
                    frequency *= lacunarity;
                    
                    // Assigns these new Perlin values back into the noiseMap
                    noiseMap[x, y] = perlinValue;
                    }

                // Bounds the noiseHeight to the min and max possible values for floats
                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                    }
                if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                    }

                noiseMap[x, y] = noiseHeight;

                }
            }

        // run through the map values once again and normalize them back to 0-1
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

                return noiseMap;
      }       
}
