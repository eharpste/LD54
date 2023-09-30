using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Runway))]
public class RunwayEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        Runway runway = (Runway)target;


        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Snap Path")) {
            for (int i = 0; i < runway.TaxiPoints.Count; i++) {
                Vector2 vec = runway.TaxiPoints[i];
                vec.x = Mathf.Round(vec.x * 2) / 2;
                vec.y = Mathf.Round(vec.y * 2) / 2;
                runway.TaxiPoints[i] = vec;
            }
        }

        EditorGUILayout.EndVertical();
    }

    public void OnSceneGUI() {
        Runway runway = (Runway)target;

        List<Vector3> linePoints = new List<Vector3>();
        Handles.color = Color.green;

        for (int i = 0; i < runway.TaxiPoints.Count; i++) {
            linePoints.Add(runway.TaxiPoints[i]);
            runway.TaxiPoints[i] = Handles.PositionHandle(runway.TaxiPoints[i], Quaternion.identity);
            GUI.color = Color.red;
            Handles.Label(runway.TaxiPoints[i], "T"+i.ToString());

        }

        runway.launchPoint = Handles.PositionHandle(runway.launchPoint, Quaternion.identity);
        GUI.color = Color.red;
        Handles.Label(runway.launchPoint, "Launch Point");

        Handles.DrawAAPolyLine(8, linePoints.ToArray());

    }
}
