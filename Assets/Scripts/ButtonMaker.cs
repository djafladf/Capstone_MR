using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BB))]
public class ButtonMaker : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical(); 
        GUILayout.FlexibleSpace();

        /*GUILayout.Space(20);
        if (GUILayout.Button("Refresh Pose_User", GUILayout.Height(30)))
        {
            PosePlayer.pp.UpdateJson(); PosePlayer.pp.UpdatePose();
        }*/
        GUILayout.Space(20);
        if (GUILayout.Button("Refresh Pose_Model", GUILayout.Height(30)))
        {
            PoseModel.pm.UpdateJson(); PoseModel.pm.UpdatePose();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Refresh All", GUILayout.Height(30)))
        {
            //PosePlayer.pp.UpdateJson(); PosePlayer.pp.UpdatePose();
            PoseModel.pm.UpdateJson(); PoseModel.pm.UpdatePose();
        }

        GUILayout.FlexibleSpace();  
        EditorGUILayout.EndVertical();  

    }
}
