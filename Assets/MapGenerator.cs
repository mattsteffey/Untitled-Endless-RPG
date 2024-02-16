
using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMaps, ColorMap, Mesh }
    public DrawMode drawMode;

    public NoiseData elevationData;
    public NoiseData tempData;
    public NoiseData humidityData;

    public string seed;
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;


    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;


    public bool autoUpdate;
    public TerrainType[] biomes;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData();


        // This is the reference to the MapDisplay class by finding the object it is on.
        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMaps) {
            // This runs the function DrawNoiseMap with our noiseMap we recieved back from Noise.GenerateNoiseMap
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.elevationMap));
            }

        else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
        else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
        }

    // This is what is responsible for spinning up MapDataThread which will run its contents on another thread.
    public void RequestMapData(Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(callback);
            };

        new Thread(threadStart).Start();
        }

    void MapDataThread(Action<MapData> callback) {
        MapData mapData = GenerateMapData();
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, callback);
            };

        new Thread(threadStart).Start();
        }

    void MeshDataThread(MapData mapData, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

    void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
                }
            }

        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
                }
            }
        }


    MapData GenerateMapData() {
        //Creates a new noiseMap, by passing in the values set in the inspector and sending it to the "Noise.cs" script.
        float[,] elevationMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, elevationData.noiseScale, elevationData.octaves, elevationData.persistance, elevationData.lacunarity, elevationData.offset);
        float[,] temperatureMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, tempData.noiseScale, tempData.octaves, tempData.persistance, tempData.lacunarity, tempData.offset);
        float[,] humidityMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, humidityData.noiseScale, humidityData.octaves, humidityData.persistance, humidityData.lacunarity, humidityData.offset);

        // creates a 1D colorMap from the 2D noiseMap, again.
        Color32[] colorMap = new Color32[mapChunkSize * mapChunkSize];

        // Loops through the elevation noiseMap
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                // sets this float to the current point of the noisemap
                float currentElevation = elevationMap[x, y];
                float currentTemp = temperatureMap[x, y];
                float currentHumidity = humidityMap[x, y];
                // Loops through possible biome values and sets the biome at each point on the heightMap
                for (int i = 0; i < biomes.Length; i++) {
                    if (currentTemp >= biomes[i].minTemp && currentHumidity >= biomes[i].minHumidity && currentElevation >= biomes[i].minElevation) {
                        colorMap[y * mapChunkSize + x] = biomes[i].color;
                        }
                    }
                }
            }
        return new MapData(elevationMap, colorMap);

        }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
            }
        }


    // This struct is what is seen in the inspector that allows to set attributes of different terrains (ocean, forest, etc.) This is currently only tied to height, but we will need to calculate humidity and temp values for this.
    [System.Serializable]
    public struct TerrainType {
        public string name;
        public Color32 color;
        public float minHumidity;
        public float minTemp;
        public float minElevation;
        }


    public struct MapData {
        public readonly float[,] elevationMap;
        public readonly Color32[] colorMap;

        public MapData(float[,] elevationMap, Color32[] colorMap) {
            this.elevationMap = elevationMap;
            this.colorMap = colorMap;
            }
        }


    

