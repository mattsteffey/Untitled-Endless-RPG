using UnityEngine;

public class MovableObject : MonoBehaviour {
    public float speed = 5f; // Speed of the object, adjustable in the editor
    public bool move = true; // Checkbox to start or stop movement

    void Update() {
        if (move) {
            // Move the object forward relative to its own orientation
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
    }