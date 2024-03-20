using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BiomeMaps : MonoBehaviour {

    public List<BiomeData> ApplyBiomeHeightMaps() {
        TextureData.Layer[] biomes = gameObject.GetComponent<TerrainGenerator>().textureSettings.layers;
        
        List<BiomeData> biomeDataList = new List<BiomeData>();

        for (int i = 0; i < biomes.Length; i++) {
            BiomeData biomeData = new BiomeData {
                startHeight = biomes[i].startHeight,
                startTemp = biomes[i].startTemperature,
                startHumidity = biomes[i].startHumidity,
                biomeHeightMaps = new List<HeightMapSettings>(),
                
                };

            if (biomes[i].biomeNoise.Length > 0) {
                for (int j = 0; j < biomes[i].biomeNoise.Length; j++) {
                    biomeData.biomeHeightMaps.Add(biomes[i].biomeNoise[j]);
                    }
                }
            biomeDataList.Add(biomeData);
            }
        return biomeDataList;
        }

    public class BiomeData {
        public float startHeight;
        public float startTemp;
        public float startHumidity;
        public List<HeightMapSettings> biomeHeightMaps;
        public List<float[,]> noiseMaps;

        public BiomeData() {
            biomeHeightMaps = new List<HeightMapSettings>();
            noiseMaps = new List<float[,]>(); // Ensure this list is also initialized
            }

        }

    //public BiomeData(float startHeight, float StartTemp, float StartHumidity, List<HeightMapSettings> biomeHeightMaps) {
    //    this.startHeight = startHeight;
    //    this.startTemp = StartTemp;
    //    this.startHumidity = StartHumidity;
    //    this.List<HeightMapSettings> biomeHeightMaps = biomeHeightMaps;
    //  }
    }
