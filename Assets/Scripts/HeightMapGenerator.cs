using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings heightSettings, HeightMapSettings tempSettings, HeightMapSettings humiditySettings, Vector2 sampleCentre) {

        // values holds the data for the heightmap. 
        float[,] heightValues = Noise.GenerateNoiseMap(width, height, heightSettings.noiseSettings, sampleCentre);
        float[,] tempValues = Noise.GenerateNoiseMap(width, height, tempSettings.noiseSettings, sampleCentre);
        float[,] humidityValues = Noise.GenerateNoiseMap(width, height, humiditySettings.noiseSettings, sampleCentre);


        AnimationCurve heightCurve_threadsafe = new AnimationCurve(heightSettings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        //Debug.Log("height: " + heightValues[0, 0] + "  ---  " + "temp: " + tempValues[0, 0] + "  ---   " + "humidty: " + humidityValues[0, 0]);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                heightValues[i, j] *= heightCurve_threadsafe.Evaluate(heightValues[i, j]) * heightSettings.heightMultiplier;

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
