using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Tongsin))]
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
        if (GUILayout.Button("Set Leg Gap", GUILayout.Height(30)))
        {
            if(Tongsin.inst != null) Tongsin.inst.MakeGapOfLeg("Device1");
        }
        GUILayout.FlexibleSpace();  
        EditorGUILayout.EndVertical();  

    }
}
#endif
