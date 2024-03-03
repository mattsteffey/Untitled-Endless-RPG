using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour {
    public HeightMapSettings tempSettings;
    public HeightMapSettings humiditySettings;
    MeshSettings meshSettings;
    void Start() {
        MeshSettings meshSettings = FindFirstObjectByType<TerrainGenerator>().meshSettings;
        }

    public void ChunkLoadz(Vector2 coord) {
        //float[,] tempValues = Noise.GenerateNoiseMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, tempSettings.noiseSettings, coord);
        //Debug.Log("temp chunk loadz");
        //for (int i = 0; i < tempValues.Length; i++) {

        //    }
        //}


        }
    }
