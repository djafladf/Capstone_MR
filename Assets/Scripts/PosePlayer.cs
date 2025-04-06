using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public class PosePlayer : MonoBehaviour
{
    public static PosePlayer pp;

    private void Awake()
    {
        if (PosePlayer.pp == null) PosePlayer.pp = this;
        else Destroy(gameObject);
    }


    private PoseData_User poseData;
    private Dictionary<int, Transform> jointMap;
    private Dictionary<int, Quaternion> initialRotations;
    [SerializeField] private int currentFrame = 0;


    void Start()
    {
        jointMap = new Dictionary<int, Transform>
        {
            {11, FindBone("mixamorig:LeftShoulder")},
            {12, FindBone("mixamorig:RightShoulder")},
            {13, FindBone("mixamorig:LeftArm")},
            {14, FindBone("mixamorig:RightArm")},
            {15, FindBone("mixamorig:LeftForeArm")},
            {16, FindBone("mixamorig:RightForeArm")},
            {17, FindBone("mixamorig:LeftHand")},
            {18, FindBone("mixamorig:RightHand")},
            {19, FindBone("mixamorig:LeftHandIndex1")},
            {20, FindBone("mixamorig:RightHandIndex1")},
            {21, FindBone("mixamorig:LeftHandPinky1")},
            {22, FindBone("mixamorig:RightHandPinky1")},
            {23, FindBone("mixamorig:LeftUpLeg")},
            {24, FindBone("mixamorig:RightUpLeg")},
            {25, FindBone("mixamorig:LeftLeg")},
            {26, FindBone("mixamorig:RightLeg")},
            {27, FindBone("mixamorig:LeftFoot")},
            {28, FindBone("mixamorig:RightFoot")},
            {29, FindBone("mixamorig:LeftToeBase")},
            {30, FindBone("mixamorig:RightToeBase")},
            {31, FindBone("mixamorig:LeftFoot")},
            {32, FindBone("mixamorig:RightFoot")},
        };

        initialRotations = new Dictionary<int, Quaternion>();

        foreach (var pair in jointMap)
        {
            if (pair.Value != null) initialRotations[pair.Key] = pair.Value.localRotation;
        }
    }

    public void UpdateJson()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "test_user.json");
            poseData = JsonConvert.DeserializeObject<PoseData_User>(File.ReadAllText(path));
            foreach (var j in poseData.landmarks) j.Out();
        }
        catch (Exception e)
        {
            Debug.Log("Load Fail!");
        }
    }

    public void UpdatePose()
    {
        if (poseData == null) { Debug.Log("No Json!"); return; }
        if (currentFrame > poseData.landmarks.Count) { Debug.Log("More than limit!"); return; }
        ApplyPose(poseData.landmarks);
    }

    void ApplyPose(List<landmarks> frame)
    {
        foreach (var kp in frame)
        {
            if (!jointMap.TryGetValue(kp.id, out Transform joint)) continue;
            if (joint == null) continue;

            Quaternion relativeRot = Quaternion.Euler(
                Mathf.Rad2Deg * kp.x,
                Mathf.Rad2Deg * kp.y,
                Mathf.Rad2Deg * kp.z
            );

            if (initialRotations.ContainsKey(kp.id))
            {
                joint.localRotation = initialRotations[kp.id] * relativeRot;
            }
        }
    }

    Transform FindBone(string name)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == name)
                return t;
        }
        return null;
    }
}
