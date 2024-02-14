using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMaps, ColorMap }
    public DrawMode drawMode;

    public NoiseData[] noiseData;
     // This will allow us to set the size, scale, and shape of the map in the inspector.

    public string seed;


    public bool autoUpdate;
    public TerrainType[] biomes;
    public void GenerateMap() {

        for (int i = 0; i < noiseData.Length; i++) {
            // This creates a new noiseMap, by passing in the values set in the inspector and sending it to the "Noise.cs" script.
            float[,] noiseMap = Noise.GenerateNoiseMap(noiseData[i].mapWidth, noiseData[i].mapHeight, seed, noiseData[i].noiseScale, noiseData[i].octaves, noiseData[i].persistance, noiseData[i].lacunarity, noiseData[i].offset);

            // creates a 1D colorMap from the 2D noiseMap, again.
            Color32[] colorMap = new Color32[noiseData[i].mapWidth * noiseData[i].mapHeight];
            // Loops through the noiseMap
            for (int y = 0; y < noiseData[i].mapHeight; y++) {
                for (int x = 0; x < noiseData[i].mapWidth; x++) {
                    // sets this float to the current point of the noisemap
                    float currentHeight = noiseMap[x, y];

                    // Loops through each biome and figures out what biome it belings to based on height
                    // this currently works by checking when the height applies and breaking out of the loop. So this functionally finds the start point of each height biome and banks the color
                    for (int ii = 0; ii < biomes.Length; ii++) {
                        if (currentHeight <= biomes[ii].height) {
                            colorMap[y * noiseData[i].mapWidth + x] = biomes[ii].color;
                            break;
                            }
                        }
                    }
                }



            // This is the reference to the MapDisplay class by finding the object it is on.
            MapDisplay display = FindFirstObjectByType<MapDisplay>();

            if (drawMode == DrawMode.NoiseMaps) {
                // This runs the function DrawNoiseMap with our noiseMap we recieved back from Noise.GenerateNoiseMap
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap), i);
                }

            if (drawMode == DrawMode.ColorMap) {
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, noiseData[i].mapWidth, noiseData[i].mapHeight), i);
                }
            }
        }

    }

// This struct is what is seen in the inspector that allows to set attributes of different terrains (ocean, forest, etc.) This is currently only tied to height, but we will need to calculate humidity and temp values for this.
[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color32 color;
    }
