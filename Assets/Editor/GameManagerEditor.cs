using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Codice.CM.Triggers.TriggerRunner;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor {

    float cellWdith = 50;
    bool showChallenges = false;
    bool showChallengeTable = false;
    Vector2 scrollPos = Vector2.zero;
    
    public override void OnInspectorGUI() {
       
        GameManager manager = (GameManager)target;


        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Challenges", EditorStyles.boldLabel);
        showChallenges =  EditorGUILayout.Foldout(showChallenges, "Challenge Settings");
        if (showChallenges) {
            //EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Challenges are now arrayed on a Timeline with each frame showing how many new tasks of each type will be generated. " +
                "Each task is randomly selected from the set of the appropriate type or randomly generated if necessary.\n\n" +
                "IC = Inbound Cargo\n" +
                "IP = Inbound Passengers\n" +
                "OC = Outbound Cargo\n" +
                "OP = Outbound Passgeners\n" +
                "FB = Flybys\n" +
                "RK = Rockets\n" +
                "HL = Haulers", MessageType.None);

            manager.challengeListSetting = (GameManager.ChallengeListSetting)EditorGUILayout.EnumPopup("On End of List", manager.challengeListSetting);

            showChallengeTable = EditorGUILayout.Foldout(showChallengeTable, "Challenge Timeline");
            if (showChallengeTable) {
                //EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Challenge")) {
                    manager.Challenges.Add(new GameManager.TaskFrame());
                }

                if (GUILayout.Button("Remove Challenge")) {
                    manager.Challenges.RemoveAt(manager.Challenges.Count - 1);
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("T", GUILayout.MaxWidth((Mathf.Log10(manager.Challenges.Count) + 1) * 10));
                EditorGUILayout.LabelField("IC", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("IP", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("OC", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("OP", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("FB", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("RK", GUILayout.MaxWidth(cellWdith));
                EditorGUILayout.LabelField("HL", GUILayout.MaxWidth(cellWdith));

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < manager.Challenges.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i + ":", GUILayout.MaxWidth((Mathf.Log10(manager.Challenges.Count) + 1) * 10));
                    manager.Challenges[i].inboundCargo = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].inboundCargo, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].inboundPassenger = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].inboundPassenger, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].outboundCargo = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].outboundCargo, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].outboundPassenger = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].outboundPassenger, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].flybys = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].flybys, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].rockets = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].rockets, GUILayout.MaxWidth(cellWdith)), 0, 10);
                    manager.Challenges[i].haulers = Mathf.Clamp(EditorGUILayout.IntField(manager.Challenges[i].haulers, GUILayout.MaxWidth(cellWdith)), 0, 10);


                    EditorGUILayout.EndHorizontal();
                }
                //EditorGUI.indentLevel --;
            }
            //EditorGUI.indentLevel--;
        }
        EditorGUILayout.LabelField("Default Inspector", EditorStyles.boldLabel);
        DrawDefaultInspector();


        /*
                if(GUILayout.Button("Sort Task Specs")) {
                    manager.taskSpecs.Sort((a, b) => a.appearanceTime.CompareTo(b.appearanceTime));
                }

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

                if(GUILayout.Button("Randomize Locations")) {
                    manager.RandomizeTaskLocations();
                }
        */
        EditorGUILayout.EndVertical();
    }

    //public void OnSceneGUI() {
    //    GameManager manager = (GameManager)target;

    //    for(int i = 0; i < manager.taskSpecs.Count; i++) {
    //        GameManager.TaskSpec spec = manager.taskSpecs[i];
    //        if(spec.task.taskType == Task.TaskType.Departure) {
    //            continue;
    //        }
    //        spec.entranceLocation = Handles.PositionHandle(spec.entranceLocation, Quaternion.identity);
    //        GUI.color = spec.task.destination switch {
    //            Task.Destination.North => Color.blue,
    //            Task.Destination.South => Color.yellow,
    //            Task.Destination.East => Color.magenta,
    //            Task.Destination.West => Color.cyan,
    //            Task.Destination.Up => Color.red,
    //            Task.Destination.Local => Color.green,
    //            _ => Color.black
    //        };
    //        Handles.Label(spec.entranceLocation, string.Format("{0}:{1}-{2}", i, spec.appearanceTime, spec.task.cargoType));

    //    }
    //}

}
