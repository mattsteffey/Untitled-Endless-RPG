using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MapGenerator))]
// Extending from Editor instead of monobehavior
public class MapGeneratorEditor : Editor
{
    // OnInspectorGUI() makes a custom inspector completely from scratch
    public override void OnInspectorGUI() {
        // target is the editor that this custom object is inspecting,in this case, the MapGenerator script
        MapGenerator mapGen = (MapGenerator)target;
        
        // Since we reset the inspector with override OnInspectorGUI(), we need to bring back in the default inspector
       // This will tell if an update has been made to the inspector, and autoUpdate is checked in MapGenerator, we can redraw the map
        if( DrawDefaultInspector()) {
            if (mapGen.autoUpdate) {
                mapGen.GenerateMap();
                }
            }

        // Adds a GUI Button with the label "Generate"
        if (GUILayout.Button("Generate")) {
            // This calls the function to actually generate the map in MapGenerator
            mapGen.GenerateMap();
            }

        }

    }
