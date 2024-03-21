using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Rendering;

public static class HeightMapGenerator {


    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings heightSettings, HeightMapSettings tempSettings, HeightMapSettings humiditySettings, List<BiomeMaps.BiomeData> biomeDataList, Vector2 sampleCentre) {
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(heightSettings.heightCurve.keys);
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // values holds the data for the heightmap. 
        float[,] heightValues = Noise.GenerateNoiseMap(width, height, heightSettings.noiseSettings, sampleCentre);
        float[,] tempValues = Noise.GenerateNoiseMap(width, height, tempSettings.noiseSettings, sampleCentre);
        float[,] humidityValues = Noise.GenerateNoiseMap(width, height, humiditySettings.noiseSettings, sampleCentre);

       

        // Creates a list of different biome Noise Maps
        List<float[,]> biomeNoiseValues = new List<float[,]>();
        for (int i = 0; i < biomeDataList.Count; i++) {
            float[,] biomeNoise = Noise.GenerateNoiseMap(width, height, biomeDataList[i].biomeHeightMaps[0].noiseSettings, sampleCentre);
            biomeNoiseValues.Add(biomeNoise);

            }




        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                
                float biomeHeightValue = new float();
                float biomeHeightMultiplier = new float();

                float oldBiomeValue = -1;
                float currentBiomeValue = -2;

               

                for (int k = 0; k < biomeDataList.Count; k++) {
                    if (k < biomeDataList.Count) {
                        if (heightValues[i, j] * heightCurve_threadsafe.Evaluate(heightValues[i, j]) >= biomeDataList[k].startHeight) {
                            if (tempValues[i, j] >= biomeDataList[k].startTemp) {
                                if (humidityValues[i, j] >= biomeDataList[k].startHumidity) {
                                    biomeHeightValue = biomeNoiseValues[k][i, j];
                                    biomeHeightMultiplier = biomeDataList[k].biomeHeightMaps[0].heightMultiplier;
                                    currentBiomeValue = k;
                                    //Debug.Log(k + " " + currentBiomeValue+ "  "+ oldBiomeValue);
                                    }
                                }
                            }
                        }
                    }

                if (oldBiomeValue != currentBiomeValue) {
                    if (i > 0 && i < width && j > 0 && j < height) {

                        }
                    }

                oldBiomeValue = currentBiomeValue;





                heightValues[i, j] *= heightCurve_threadsafe.Evaluate(heightValues[i, j]) * heightSettings.heightMultiplier + (biomeHeightValue * biomeHeightMultiplier);

                if (heightValues[i, j] > maxValue) {
                    maxValue = heightValues[i, j];
                    }
                if (heightValues[i, j] < minValue) {
                    minValue = heightValues[i, j];
                    }
                }
            }

        return new HeightMap(heightValues, tempValues, humidityValues, minValue, maxValue);
        }
    }

public struct HeightMap {
    public readonly float[,] heightValues;
    public readonly float[,] tempValues;
    public readonly float[,] humidityValues;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] heightValues, float[,] tempValues, float[,] humidityValues, float minValue, float maxValue) {
        this.heightValues = heightValues;
        this.tempValues = tempValues;
        this.humidityValues = humidityValues;
        this.minValue = minValue;
        this.maxValue = maxValue;
        }
    }