using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFlattener : MonoBehaviour {
    // LayerMask to filter which objects to affect. Set this in the Inspector.
    public LayerMask affectedLayers;

    void Start() {
        FlattenBelow();
        }

    void FlattenBelow() {
        MeshFilter thisMeshFilter = GetComponent<MeshFilter>();
        Bounds bounds = thisMeshFilter.mesh.bounds;
        bounds.center = transform.position; // Convert local bounds to world space

        // Expand bounds slightly vertically to ensure it captures meshes right below.
        Vector3 expandedBoundsSize = new Vector3(bounds.size.x, 0.1f, bounds.size.z);
        Collider[] collidersBelow = Physics.OverlapBox(bounds.center, expandedBoundsSize / 2, Quaternion.identity, affectedLayers);

        foreach (Collider col in collidersBelow) {
            MeshFilter meshFilter = col.GetComponent<MeshFilter>();
            if (meshFilter != null) {
                AdjustMeshVertices(meshFilter, bounds);
                }
            }
        }

    void AdjustMeshVertices(MeshFilter meshFilter, Bounds referenceBounds) {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        // Convert vertices to world space to check if they're within the reference bounds.
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]);
            if (referenceBounds.Contains(worldVertex)) {
                minY = Mathf.Min(minY, worldVertex.y);
                maxY = Mathf.Max(maxY, worldVertex.y);
                }
            }

        // Calculate the average Y of vertices within bounds
        float averageY = (minY + maxY) / 2;

        // Adjust vertex Y positions to the average, for those within bounds
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]);
            if (referenceBounds.Contains(worldVertex)) {
                Vector3 adjustedVertex = new Vector3(worldVertex.x, averageY, worldVertex.z);
                vertices[i] = meshFilter.transform.InverseTransformPoint(adjustedVertex);
                }
            }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        }
    }