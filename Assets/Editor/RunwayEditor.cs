using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Runway))]
public class RunwayEditor : Editor
{

    private bool showTaxiPath = true;
    private bool showLaunchPath = true;

    private float snapResolution = 0.5f;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        Runway runway = (Runway)target;

        snapResolution = EditorGUILayout.FloatField("Snap Resolution", snapResolution);
        showTaxiPath = EditorGUILayout.Toggle("Show Taxi Path", showTaxiPath);
        showLaunchPath = EditorGUILayout.Toggle("Show Launch Path", showLaunchPath);

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(string.Format("Snap Paths to Nearest: {0}", snapResolution))) {
            var snapRes = 1/snapResolution;
            for (int i = 0; i < runway.TaxiPath.Count; i++) {
                Vector3 vec = runway.TaxiPath[i];
                vec.x = Mathf.Round(vec.x * snapRes) / snapRes;
                vec.y = Mathf.Round(vec.y * snapRes) / snapRes;
                vec.z = Mathf.Round(vec.z * snapRes) / snapRes;
                runway.TaxiPath[i] = vec;
            }
        }

        EditorGUILayout.EndVertical();
    }

    public void OnSceneGUI() {
        Runway runway = (Runway)target;

        List<Vector3> linePoints = new List<Vector3>();

        if (showTaxiPath) {
            Handles.color = Color.magenta;
            for (int i = 0; i < runway.TaxiPath.Count; i++) {
                linePoints.Add(runway.TaxiPath[i]);
                runway.TaxiPath[i] = Handles.PositionHandle(runway.TaxiPath[i], Quaternion.identity);
                GUI.color = Color.magenta;
                Handles.Label(runway.TaxiPath[i], "T" + i.ToString());
            }
            Handles.DrawAAPolyLine(5, linePoints.ToArray());
        }

        if (showLaunchPath) {
            linePoints.Clear();
            Handles.color = Color.cyan;
            for (int i = 0; i < runway.LaunchPath.Count; i++) {
                linePoints.Add(runway.LaunchPath[i]);
                runway.LaunchPath[i] = Handles.PositionHandle(runway.LaunchPath[i], Quaternion.identity);
                GUI.color = Color.cyan;
                Handles.Label(runway.LaunchPath[i], "L" + i.ToString());
            }
            Handles.DrawAAPolyLine(5, linePoints.ToArray());
        }

        
        

    }
}
