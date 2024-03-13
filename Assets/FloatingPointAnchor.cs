using UnityEngine;

public class FloatingPointAnchor : MonoBehaviour {

    public GameObject player;
    public GameObject playerGhost;
    public Vector3 offset;

    private void Update() {
        offset = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        }

    public void ResetOrigin() {
        gameObject.transform.position -= offset;
        playerGhost.transform.position += offset;
        player.transform.position = new Vector3(player.transform.position.x - offset.x, player.transform.position.y, player.transform.position.z - offset.z);     
        }
    }
    
