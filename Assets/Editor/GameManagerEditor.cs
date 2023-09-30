using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Codice.CM.Triggers.TriggerRunner;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GameManager manager = (GameManager)target;


        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Snap Entrance Locations")) {
            for (int i = 0; i < manager.taskSpecs.Count; i++) {
                if (manager.taskSpecs[i].task.taskType == Task.TaskType.Departure) {
                    continue;
                }
                Vector3 vec = manager.taskSpecs[i].entranceLocation;
                vec.x = Mathf.Round(vec.x);
                vec.y = Mathf.Round(vec.y);
                vec.z = Mathf.Round(vec.z );
                manager.taskSpecs[i].entranceLocation = vec;
            }
        }

        EditorGUILayout.EndVertical();
    }

    public void OnSceneGUI() {
        GameManager manager = (GameManager)target;

        for(int i = 0; i < manager.taskSpecs.Count; i++) {
            GameManager.TaskSpec spec = manager.taskSpecs[i];
            if(spec.task.taskType == Task.TaskType.Departure) {
                continue;
            }
            spec.entranceLocation = Handles.PositionHandle(spec.entranceLocation, Quaternion.identity);
            GUI.color = spec.task.destination switch {
                Task.Destination.North => Color.blue,
                Task.Destination.South => Color.yellow,
                Task.Destination.East => Color.magenta,
                Task.Destination.West => Color.cyan,
                Task.Destination.Up => Color.red,
                Task.Destination.Local => Color.green,
                _ => Color.black
            };
            Handles.Label(spec.entranceLocation, string.Format("{0}:{1}-{2}", i, spec.appearanceTime, spec.task.cargoType));

        }
    }
}
