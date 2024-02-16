using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] elevationMap, float heightMultiplier , AnimationCurve heightCurve, int levelOfDetail) {

        int width = elevationMap.GetLength(0);
        int height = elevationMap.GetLength(1);

        // this adjusts the position of the mesh so that 0,0 is centered
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail ==0)?1: levelOfDetail * 2;
        int verticiesPerLine = (width-1)/meshSimplificationIncrement + 1;


        MeshData meshData = new MeshData (verticiesPerLine, verticiesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                meshData.verticies[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(elevationMap[x,y]) * heightMultiplier, topLeftZ - y);
                
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width,y/(float)height);

                // this cycles through the verticies and adds triangles 
                if(x < width - 1 && y < height - 1) {
                    meshData.addTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.addTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                    }

                vertexIndex++;
                }
            }
        return meshData;
        }
}

public class MeshData {
    public Vector3[] verticies;
    public int[] triangles;
    public Vector2[] uvs;


    int triangleIndex;

    // Constructor Function
    public MeshData(int meshWidth, int meshHeight) {
        // verticies are the number of points, this is simple W*H
        verticies = new Vector3[meshWidth * meshHeight];
        //you have to have uvs for textures to display
        uvs = new Vector2[meshWidth * meshHeight];
        //Triangles I still am not sure why it is this way
        triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
        }

    public void addTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
        }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
        }

    }
