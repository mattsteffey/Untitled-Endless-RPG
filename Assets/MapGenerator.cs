
using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMaps, ColorMap, Mesh }
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public NoiseData elevationData;
    public NoiseData tempData;
    public NoiseData humidityData;

    public string seed;

    //96 divisible by all even numbers through 12 except 10 (1,2,4,6,8,12)


    public bool useFlatShading;

    [Range(0, 6)]
    public int editorPreviewLOD;


    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;


    public bool autoUpdate;
    public TerrainType[] biomes;
    static MapGenerator instance;


    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawMapInEditor();
            }
        }

    private void OnValidate() {
        if (elevationData != null) {
            elevationData.OnValuesUpdated -= OnValuesUpdated;
            elevationData.OnValuesUpdated += OnValuesUpdated;
            }
        if (tempData != null) {
            tempData.OnValuesUpdated -= OnValuesUpdated;
            tempData.OnValuesUpdated += OnValuesUpdated;
            }
        if (humidityData != null) {
            humidityData.OnValuesUpdated -= OnValuesUpdated;
            humidityData.OnValuesUpdated += OnValuesUpdated;
            }
        }

    public static int mapChunkSize {
        get {
            if (instance == null) {
                instance = FindFirstObjectByType<MapGenerator>();
                }

            if (instance.useFlatShading) {
                return 95;
                }
            else {
                return 239;
                }
            }
        }
    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);
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
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD, useFlatShading), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
        }

    // This is what is responsible for spinning up MapDataThread which will run its contents on another thread.
    public void RequestMapData(Vector2 center, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
            };

        new Thread(threadStart).Start();
        }

    void MapDataThread(Vector2 center, Action<MapData> callback) {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
            };

        new Thread(threadStart).Start();
        }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, lod, useFlatShading);
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


    MapData GenerateMapData(Vector2 center) {
        //Creates a new noiseMap, by passing in the values set in the inspector and sending it to the "Noise.cs" script.
        float[,] elevationMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, elevationData.noiseScale, elevationData.octaves, elevationData.persistance, elevationData.lacunarity, center + elevationData.offset, normalizeMode);
        float[,] temperatureMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, tempData.noiseScale, tempData.octaves, tempData.persistance, tempData.lacunarity, center + tempData.offset, normalizeMode);
        float[,] humidityMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, humidityData.noiseScale, humidityData.octaves, humidityData.persistance, humidityData.lacunarity, center + humidityData.offset, normalizeMode);

        // creates a 1D colorMap from the 2D noiseMap, again.
        Color32[] colorMap = new Color32[mapChunkSize * mapChunkSize];
        Texture2D[] textureMap = new Texture2D[mapChunkSize * mapChunkSize];

        // Loops through the elevation noiseMap
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                // sets this float to the current point of the noisemap
                float currentElevation = elevationMap[x, y];
                float currentTemp = temperatureMap[x, y];
                float currentHumidity = humidityMap[x, y];
                for (int i = 0; i < biomes.Length; i++) {
                    if (currentTemp >= biomes[i].minTemp && currentHumidity >= biomes[i].minHumidity && currentElevation >= biomes[i].minElevation) {
                        colorMap[y * mapChunkSize + x] = biomes[i].color;


                        //This needs to pass terrain color data into the shaderGraph instead

                        }
                    else {
                        break;
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
    public Texture2D baseTexture;
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



