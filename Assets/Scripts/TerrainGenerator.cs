using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;

public class TerrainGenerator : MonoBehaviour {

    //const float playerMoveThresholdForChunkUpdate = 25f;
    //const float sqrPlayerMoveThresholdForChunkUpdate = playerMoveThresholdForChunkUpdate * playerMoveThresholdForChunkUpdate;


    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public HeightMapSettings tempSettings;
    public HeightMapSettings humiditySettings;

    public Transform playerGhost;
    public Material mapMaterial;

    Vector2 playerPosition;

    float meshWorldSize;
    int chunksVisibleInViewDst;

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

    void Update() {
         foreach (TerrainChunk chunk in visibleTerrainChunks) {
            chunk.UpdateCollisionMesh();
            }

        }

    public void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
            }

        playerPosition = new Vector2(playerGhost.transform.position.x, playerGhost.transform.position.z);
        int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                    else {
                        //Debug.Log("hit");
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, tempSettings, humiditySettings, meshSettings, detailLevels, colliderLODIndex, transform, playerGhost, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                        }
                    }
                }
            }
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