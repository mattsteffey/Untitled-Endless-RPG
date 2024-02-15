using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class TextureGenerator 
{
    //Recieves the colorMap from ??? and applies it to a texture
    public static Texture2D TextureFromColorMap(Color32[] colorMap, int width, int height) {

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels32(colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
        }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        //gets the length of the noise map we are recieving. 0 and 1 refers to the dimensions in float[0,1]
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // Generates an array of colors, width times height.
        // This makes it to where there is a color for each coordinate on the x/y grid but in a line
        Color32[] colorMap = new Color32[width * height];
      
        // Loops through all the values of the noise map
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                // Since a color array is 2D, where our noise map is 3D, we will need to multiply y and width, and add x
                // Don't totally understand this but basically it sets the 'new line' of each iteration and runs through it

                //Lerp is the blend, it sets the color between black ...
                colorMap[y * width + x] = Color32.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }

        // This returns runs the ulti
        return TextureFromColorMap(colorMap, width, height);
        }
}
