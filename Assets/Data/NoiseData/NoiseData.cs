using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject {
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public int lacunarity;
    public Vector2 offset;



    //OnValidate is a standard Unity Function that fires off whenever something is changed in the inspector, in this case the MapGenerator?
    public void OnValidate() {

        if (lacunarity < 1) {
            lacunarity = 1;
            }
        if (octaves < 0) {
            octaves = 0;
            }


        }
    }





