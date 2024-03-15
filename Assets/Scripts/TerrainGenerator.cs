using UnityEngine;
using System;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

    const float viewerMoveThresholdForChunkUpdate = 100f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public HeightMapSettings tempSettings;
    public HeightMapSettings humiditySettings;

    public Material mapMaterial;

    public Transform ghostTransform;
    public Transform playerTransform;

    float meshWorldSize;
    int chunksVisibleInViewDst;
    Vector3 currentOffset;
    Vector3 totalOffset;


    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start() {

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        UpdateVisibleChunks();
        }

    void FixedUpdate() {

      

        // Checks if the player meets the threshold distance from 0,0
        if (new Vector2(playerTransform.position.x, playerTransform.position.z).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            foreach (TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollisionMesh();
                }
            // If the player meets the threshold, it moves the distance the player has travelled...
            ghostTransform.position = new Vector3(ghostTransform.position.x + playerTransform.position.x, 0, ghostTransform.position.z + playerTransform.position.z);
            currentOffset = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            totalOffset += new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            playerTransform.position = new Vector3(0, playerTransform.position.y, 0);
            gameObject.transform.position -= currentOffset;
            UpdateVisibleChunks();

           
                    }
        }

    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
            }

        int ghostCurrentChunkCoordX = Mathf.RoundToInt(ghostTransform.position.x / meshWorldSize);
        int ghostCurrentChunkCoordY = Mathf.RoundToInt(ghostTransform.position.z / meshWorldSize);
        int playerCurrentChunkCoordX = Mathf.RoundToInt(playerTransform.position.x / meshWorldSize);
        int playerCurrentChunkCoordY = Mathf.RoundToInt(playerTransform.position.z / meshWorldSize);


        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(ghostCurrentChunkCoordX + xOffset, ghostCurrentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                    else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, tempSettings, humiditySettings, meshSettings, detailLevels, colliderLODIndex, transform, ghostTransform, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load((meshObject) => TerrainLoadComplete(meshObject));
                        }
                    }
                }
            }
        }



    public void TerrainLoadComplete(GameObject loadedChunk) {
        loadedChunk.transform.position -= (totalOffset);
        }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) {
            visibleTerrainChunks.Add(chunk);
            }
        else {
            visibleTerrainChunks.Remove(chunk);
            }
        }

    }

[System.Serializable]
public struct LODInfo {
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;


    public float sqrVisibleDstThreshold {
        get {
            return visibleDstThreshold * visibleDstThreshold;
            }
        }
    }