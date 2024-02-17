using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This takes in the noise map called by MapGenerator.cs to the Noise.cs
public class MapDisplay : MonoBehaviour {
    // The Renderer of the object set in the editor, this will be a plane
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public void DrawTexture(Texture2D texture) {
        // Modifying sharedMaterial will change the appearance of all objects using this material, and change material settings that are stored in the project too.
        textureRenderer.sharedMaterial.mainTexture = texture;
        // This sets the size of the plane to the same size as the noiseMap
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

    public void DrawMesh(MeshData meshData, Texture2D texture) {
        //Shared means we can fiddle with these outside of play mode
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }