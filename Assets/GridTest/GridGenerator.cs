
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GridGenerator : MonoBehaviour {
    [SerializeField]
    private GridObject[] gridObjects; // Array of objects and their densities


    public int rows = 10; // Number of grid elements vertically
    public int columns = 10; // Number of grid elements horizontally


    private Vector2 meshSize;
    private List<Vector3> availablePositions;
    private List<Vector3> borderPositions;
    private List<Vector3> innerPositions;
    private Vector2 cellSize;

    void Start() {
        CalculateMeshSize();
        cellSize = CalculateCellSize();

        availablePositions = GenerateAvailablePositions();
        borderPositions = FindBorders();
        innerPositions = FindInners();

        RandomizePositions();


        //for (int i = 0; i < innerPositions.Count; i++) {
        //    Vector3 endPoint = innerPositions[i] + Vector3.up * .5f;
        //    Debug.DrawLine(innerPositions[i], endPoint, Color.red, 100f);
        //}

        PopulateGrid();

        }



    void CalculateMeshSize() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        meshSize = new Vector2(mesh.bounds.size.x * transform.localScale.x, mesh.bounds.size.z * transform.localScale.z);
        }

    Vector2 CalculateCellSize() {
        return cellSize = new Vector2(meshSize.x / columns, meshSize.y / rows);
        }

    List<Vector3> GenerateAvailablePositions() {
        availablePositions = new List<Vector3>();
        float spacingX = meshSize.x / (columns - 1);
        float spacingZ = meshSize.y / (rows - 1);

        for (int i = 0; i < columns; i++) {
            for (int j = 0; j < rows; j++) {
                Vector3 position = new Vector3(transform.position.x + (i * spacingX) - meshSize.x / 2, transform.position.y, transform.position.z + (j * spacingZ) - meshSize.y / 2);
                availablePositions.Add(position);
                }
            }
        return availablePositions;
     }

    List<Vector3> FindBorders() {
        List<Vector3> borderList = new List<Vector3>();
        for (int i = 0; i < availablePositions.Count; i++) {
            //Top border
            if (i < rows) {
                borderList.Add(availablePositions[i]);
                Vector3 endPoint = availablePositions[i] + Vector3.up * .5f;
                //Debug.DrawLine(availablePositions[i], endPoint, Color.red, 100f);

                }
            //Left Border
            if (i >= rows && i % rows == 0) {
                borderList.Add(availablePositions[i]);
                Vector3 endPoint = availablePositions[i] + Vector3.up * .5f;
                //Debug.DrawLine(availablePositions[i], endPoint, Color.blue, 100f);

                }
            //Bottom Border
            if (i >= availablePositions.Count - rows) {
                borderList.Add(availablePositions[i]);
                Vector3 endPoint = availablePositions[i] + Vector3.up * .5f;
                //Debug.DrawLine(availablePositions[i], endPoint, Color.yellow, 100f);

                }
            //Right Border
            if (i < availablePositions.Count - rows && i % rows == rows - 1 && i > rows) {
                borderList.Add(availablePositions[i]);
                Vector3 endPoint = availablePositions[i] + Vector3.up * .5f;
                //Debug.DrawLine(availablePositions[i], endPoint, Color.green, 100f);

                }
            }
        return borderList;
        }

    List <Vector3> FindInners() {
        List<Vector3> innerList = new List<Vector3>();
        for (int i = 0; i < availablePositions.Count; i++) {
            innerList.Add(availablePositions[i]);
            if (i < rows) {
                innerList.Remove(availablePositions[i]);
                }
            if (i >= rows && i % rows == 0) {
                innerList.Remove(availablePositions[i]);
                }
            if (i >= availablePositions.Count - rows) {
                innerList.Remove(availablePositions[i]);
                }
            if (i < availablePositions.Count - rows && i % rows == rows - 1 && i > rows) {
                innerList.Remove(availablePositions[i]);

                }
            }
        return innerList;
        }



    void PopulateGrid() {
        for (int i = 0; i < gridObjects.Length; i++) {
            //Makes sure the minimum count is always met
            for (int j = 0; j < gridObjects[i].minimumCount; j++) {
                int positionIndex = Random.Range(0, availablePositions.Count);

                // Rotation Toggle
                int rotation = 0; ;
                if (gridObjects[i].randomizeRotation) {
                    rotation = 360;
                    }

                //Random Placement within each Cell
                Vector3 positionVariance = new Vector3(Random.Range(-cellSize.x / 2, cellSize.x / 2) * 0.7f, 0, Random.Range(-cellSize.y / 2, cellSize.y / 2) * 0.7f);
                Vector3 finalPosition = availablePositions[positionIndex] + positionVariance;

                // Selects a random prefab variant 
                int variantIndex = Random.Range(0, gridObjects[i].prefab.Length);
                
                GameObject instantiatedObject = Instantiate(gridObjects[i].prefab[variantIndex], finalPosition, Quaternion.Euler(0, Random.Range(0, rotation), 0));

            

                //Random Scaling
                float randomScale = Random.Range(gridObjects[i].sizeVarianceDown, gridObjects[i].sizeVarianceUp);
                instantiatedObject.transform.localScale = new Vector3(gridObjects[i].prefab[variantIndex].transform.localScale.x * randomScale, 
                    gridObjects[i].prefab[variantIndex].transform.localScale.y * randomScale, gridObjects[i].prefab[variantIndex].transform.localScale.z * randomScale);

                availablePositions.Remove(availablePositions[positionIndex]);
                }

            //Iterates between the (minimum) and (maximum) counts and has a (probabilty) chance to spawn
            for (int j = 0; j < gridObjects[i].maximumCount - gridObjects[i].minimumCount; j++) {
                int variantIndex = Random.Range(0, gridObjects[i].prefab.Length);
                if (gridObjects[i].probability > Random.value) {
                    int positionIndex = Random.Range(0, availablePositions.Count);
                    int rotation = 0; ;
                    if (gridObjects[i].randomizeRotation) {
                        rotation = 360;
                        }

                    //Random Placement within each Cell
                    Vector3 positionVariance = new Vector3(Random.Range(-cellSize.x / 2, cellSize.x / 2) * 0.5f, 0, Random.Range(-cellSize.y / 2, cellSize.y / 2) * 0.5f);
                    Vector3 finalPosition = availablePositions[positionIndex] + positionVariance;

                    GameObject instantiatedObject = Instantiate(gridObjects[i].prefab[variantIndex], finalPosition, Quaternion.Euler(0, Random.Range(0, rotation), 0));
                    
                    //Random Scaling
                    float randomScale = Random.Range(gridObjects[i].sizeVarianceDown, gridObjects[i].sizeVarianceUp);
                    instantiatedObject.transform.localScale = new Vector3(gridObjects[i].prefab[variantIndex].transform.localScale.x * randomScale,
                        gridObjects[i].prefab[variantIndex].transform.localScale.y * randomScale, gridObjects[i].prefab[variantIndex].transform.localScale.z * randomScale);

                    //When object is placed, remove that position from the list of possible positions
                    availablePositions.Remove(availablePositions[positionIndex]);
                    }
                }
            }


        }








    void RandomizePositions() {
        availablePositions = availablePositions.OrderBy(x => Random.value).ToList();
        }


    // Selects a random object from the selector, and verifies that the count doesn't go over the maximum values
    GridObject SelectRandomObject() {
        List<GridObject> availableObjects = new List<GridObject>();
        for (int i = 0; i < gridObjects.Length; i++) {
            if (gridObjects[i].currentCount < gridObjects[i].maximumCount) {
                availableObjects.Add(gridObjects[i]);
                }
            }

        if (availableObjects.Count != 0) {
            int randomIndex = Random.Range(0, availableObjects.Count);
            if (availableObjects[randomIndex].currentCount < availableObjects[randomIndex].maximumCount || availableObjects[randomIndex].maximumCount == 0) {
                return availableObjects[randomIndex];
                }
            else return null;
            }
        else return null;
        }
    }

[System.Serializable] // Makes it visible in the Unity inspector
public class GridObject {
    public GameObject[] prefab;
    public float probability; // Likelihood of this object's placement, between 0 and 1
    public float sizeVarianceDown = 1;
    public float sizeVarianceUp = 1;
    public bool randomizeRotation;
    public bool spawnInner;
    public bool spawnOuter;
    public int minimumCount;
    public int maximumCount;
    [HideInInspector] public int currentCount;

    }