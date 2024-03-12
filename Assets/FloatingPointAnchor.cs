using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPointAnchor : MonoBehaviour {

    public GameObject player;
    public GameObject playerGhost;
    public float distanceThreshold = 1;
    public Vector3 playerFloatPosition;
    private Vector3 originOffset;

    void Start () {
        gameObject.GetComponent<TerrainGenerator>().UpdateVisibleChunks();
        }

    private void Update() {
        originOffset = new Vector3(player.transform.position.x, 0, player.transform.position.z);

        playerGhost.transform.position += originOffset;

        player.transform.position -= originOffset;
        gameObject.transform.position -= originOffset;

        gameObject.GetComponent<TerrainGenerator>().UpdateVisibleChunks();
        }

    void LateUpdate() {

        //float dstFromOrigin = originOffset.magnitude;
        //if (dstFromOrigin > distanceThreshold) {

        //    playerGhost.transform.position += originOffset;

        //    player.transform.position -= originOffset;
        //    gameObject.transform.position -= originOffset;            
      
        //    gameObject.GetComponent<TerrainGenerator>().UpdateVisibleChunks();


        //    }


        
        }
    }



