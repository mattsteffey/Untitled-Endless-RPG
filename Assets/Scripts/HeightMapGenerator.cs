using System.Collections.Generic;
using UnityEngine;


public static class HeightMapGenerator {


    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings heightSettings, HeightMapSettings tempSettings, HeightMapSettings humiditySettings, List<BiomeMaps.BiomeData> biomeDataList, Vector2 sampleCentre) {
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(heightSettings.heightCurve.keys);
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // values holds the data for the heightmap. 
        float[,] heightValues = Noise.GenerateNoiseMap(width, height, heightSettings.noiseSettings, sampleCentre);
        float[,] tempValues = Noise.GenerateNoiseMap(width, height, tempSettings.noiseSettings, sampleCentre);
        float[,] humidityValues = Noise.GenerateNoiseMap(width, height, humiditySettings.noiseSettings, sampleCentre);

        List<TerrainToBlend> terrainToBlendList = new List<TerrainToBlend>();


        // Copy values from heightValues to adjustedHeightValues
        float[,] adjustedHeightValues = new float[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                adjustedHeightValues[i, j] = heightValues[i, j];
                }
            }

        // Creates a list of different biome Noise Maps
        List<float[,]> biomeNoiseValues = new List<float[,]>();
        for (int i = 0; i < biomeDataList.Count; i++) {
            float[,] biomeNoise = Noise.GenerateNoiseMap(width, height, biomeDataList[i].biomeHeightMaps[0].noiseSettings, sampleCentre);
            biomeNoiseValues.Add(biomeNoise);
            }


        // Find Biomes
        int oldBiomeValue = 0;
        int currentBiomeValue = 0;

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {

                for (int k = 0; k < biomeDataList.Count; k++) {
                    if (k < biomeDataList.Count) {
                        if (heightValues[i, j] * heightCurve_threadsafe.Evaluate(heightValues[i, j]) >= biomeDataList[k].startHeight) {
                            if (tempValues[i, j] >= biomeDataList[k].startTemp) {
                                if (humidityValues[i, j] >= biomeDataList[k].startHumidity) {
                                    currentBiomeValue = k;
                                    }
                                }
                            }
                        }
                    }

   
                        TerrainToBlend terrainPoint = new TerrainToBlend();
                        terrainPoint.blendPointsX = i;
                        terrainPoint.blendPointsY = j;
                        terrainPoint.currentTerrain = currentBiomeValue;
                        terrainPoint.previousTerrain = oldBiomeValue;
                        terrainToBlendList.Add(terrainPoint);                     
                        oldBiomeValue = currentBiomeValue;
                        


                if (heightValues[i, j] > maxValue) {
                    maxValue = heightValues[i, j];
                    }
                if (heightValues[i, j] < minValue) {
                    minValue = heightValues[i, j];
                    }

                if (adjustedHeightValues[i, j] > maxValue) {
                    maxValue = adjustedHeightValues[i, j];
                    }
                if (adjustedHeightValues[i, j] < minValue) {
                    minValue = adjustedHeightValues[i, j];
                    }
                }
            }


        for (int t = 0; t < terrainToBlendList.Count; t++) {
           int currentTerrain = terrainToBlendList[t].currentTerrain;
           int previousTerrain = terrainToBlendList[t].previousTerrain;
           int blendPointsX = terrainToBlendList[t].blendPointsX;
           int blendPointsY = terrainToBlendList[t].blendPointsY;





            if (currentTerrain != previousTerrain) {

                for (int i = -2; i <= 2; i++) {
                    for (int j = -2; j <= 2; j++) {
                        int newX = blendPointsX + i;
                        int newY = blendPointsY + j;
                        //Keep these 2's here, they are the border - until you are ready to iterate over to other chunks...
                        if (newX >= 2 && newX < height - 2 && newY >= 2 && newY < width - 2) {
                            adjustedHeightValues[newX, newY] = heightCurve_threadsafe.Evaluate(heightValues[newX, newY]) * heightSettings.heightMultiplier + ((biomeNoiseValues[currentTerrain][newX, newY] 
                                * biomeDataList[currentTerrain].biomeHeightMaps[0].heightMultiplier + biomeNoiseValues[previousTerrain][newX, newY] * biomeDataList[previousTerrain].biomeHeightMaps[0].heightMultiplier) / 2);
                            }
                        else {
                            adjustedHeightValues[blendPointsX, blendPointsY] = heightCurve_threadsafe.Evaluate(heightValues[blendPointsX, blendPointsY]) * heightSettings.heightMultiplier + (biomeNoiseValues[currentTerrain][blendPointsX, blendPointsY]
                              * biomeDataList[currentTerrain].biomeHeightMaps[0].heightMultiplier);
                            }
                        }
                    }
                }

            if (currentTerrain == previousTerrain) {
                adjustedHeightValues[blendPointsX, blendPointsY] = heightCurve_threadsafe.Evaluate(heightValues[blendPointsX, blendPointsY]) * heightSettings.heightMultiplier + (biomeNoiseValues[currentTerrain][blendPointsX, blendPointsY]
                               * biomeDataList[currentTerrain].biomeHeightMaps[0].heightMultiplier);
                }
         }



        return new HeightMap(adjustedHeightValues, tempValues, humidityValues, minValue, maxValue);
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

public struct TerrainToBlend {
    public int blendPointsX;
    public int blendPointsY;
    public int currentTerrain;
    public int previousTerrain;
    public float blendedTerrainValue;

    public TerrainToBlend(int blendPointsX, int blendPointsY, int currentTerrain, int previousTerrain, float blendedTerrainValue) {
        this.blendPointsX = blendPointsX;
        this.blendPointsY = blendPointsY;
        this.currentTerrain = currentTerrain;
        this.previousTerrain = previousTerrain;
        this.blendedTerrainValue = blendedTerrainValue;
        }
    }