using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This takes in the noise map called by MapGenerator.cs to the Noise.cs
public class MapDisplay : MonoBehaviour {
    // The Renderer of the object set in the editor, this will be a plane
    public Renderer textureRenderer;
    public void DrawTexture(Texture2D texture) {


        // Modifying sharedMaterial will change the appearance of all objects using this material, and change material settings that are stored in the project too.
        textureRenderer.sharedMaterial.mainTexture = texture;
        // This sets the size of the plane to the same size as the noiseMap
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }
    }
