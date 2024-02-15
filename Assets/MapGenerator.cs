using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TreeEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMaps, ColorMap }
    public DrawMode drawMode;

    public NoiseData elevationData;
    public NoiseData tempData;
    public NoiseData humidityData;
    // This will allow us to set the size, scale, and shape of the map in the inspector.
    public string seed;
    public int mapHeight;
    public int  mapWidth;


    public bool autoUpdate;
    public TerrainType[] biomes;
  
    public void GenerateMap() {

       
        //Creates a new noiseMap, by passing in the values set in the inspector and sending it to the "Noise.cs" script.
        float[,] elevationMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, elevationData.noiseScale, elevationData.octaves, elevationData.persistance, elevationData.lacunarity, elevationData.offset);
        float[,] temperatureMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, tempData.noiseScale, tempData.octaves, tempData.persistance, tempData.lacunarity, tempData.offset);
        float[,] humidityMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, humidityData.noiseScale, humidityData.octaves, humidityData.persistance, humidityData.lacunarity, humidityData.offset);

        //Gathers the Biome Variables (min/max) values


        // creates a 1D colorMap from the 2D noiseMap, again.
        Color32[] colorMap = new Color32[mapWidth * mapHeight];

        // Loops through the elevation noiseMap
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    // sets this float to the current point of the noisemap
                    float currentElevation = elevationMap[x, y];
                    float currentTemp = temperatureMap[x, y];
                    float currentHumidity = humidityMap[x, y];
                // Loops through possible biome values and sets the biome at each point on the heightMap
                for (int i = 0; i < biomes.Length; i++) { 
                    if (currentTemp >= biomes[i].minTemp && currentTemp <= biomes[i].maxTemp && currentHumidity >= biomes[i].minHumidity && currentHumidity <= biomes[i].maxHumidity 
                        && currentElevation >= biomes[i].minElevation && currentElevation <= biomes[i].maxElevation) {
                        colorMap[y * mapWidth + x] = biomes[i].color;
                        } 
                  
                        

                    }
                }
            }



        // This is the reference to the MapDisplay class by finding the object it is on.
        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMaps) {
            // This runs the function DrawNoiseMap with our noiseMap we recieved back from Noise.GenerateNoiseMap
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(elevationMap));
            }

        if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            }
        }

    public void OnValidate() {
        if (mapWidth < 1) {
            mapWidth = 1;
            }
        if (mapHeight < 1) {
            mapHeight = 1;
            }
        }


    }

 


// This struct is what is seen in the inspector that allows to set attributes of different terrains (ocean, forest, etc.) This is currently only tied to height, but we will need to calculate humidity and temp values for this.
[System.Serializable]
public struct TerrainType {
    public string name;
    public Color32 color;
    public float minHumidity;
    public float maxHumidity;
    public float minTemp;
    public float maxTemp;
    public float minElevation;
    public float maxElevation;
    }

