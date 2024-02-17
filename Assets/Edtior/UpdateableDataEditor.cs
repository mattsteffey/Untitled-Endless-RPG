using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoUpdateData), true)]
public class UpdateableDataEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        AutoUpdateData data = (AutoUpdateData)target;

        if (GUILayout.Button("Update")) {
            data.NotifyOfUpdatedValues();
            }

        }
    }
