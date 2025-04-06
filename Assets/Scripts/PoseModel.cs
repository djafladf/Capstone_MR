using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class PoseModel : MonoBehaviour
{
    public static PoseModel pm;
    private PoseData_Model poseData = null;

    private void Awake()
    {
        if (PoseModel.pm == null) PoseModel.pm = this;
        else Destroy(gameObject);
    }

    private Dictionary<string, Transform> jointMap;
    private Dictionary<string, Quaternion> initialRotations;
    [SerializeField] private int currentFrame = 0;

    float Mx = 0, My = 0, Mz = 0, mx = 1000, my = 1000, mz = 1000;

    [SerializeField] Transform canvas;
    [SerializeField] GameObject txt;
    [SerializeField] Gradient Grad;
    Dictionary<string, Transform> Objects = new Dictionary<string, Transform>();

    void Start()
    {
        jointMap = new Dictionary<string, Transform>
        {
            {"Nose", FindBone("mixamorig:Head")},
            {"Left Shoulder", FindBone("mixamorig:LeftArm")},
            {"Right Shoulder", FindBone("mixamorig:RightArm")},
            {"Left Elbow", FindBone("mixamorig:LeftForeArm")},
            {"Right Elbow", FindBone("mixamorig:RightForeArm")},
            {"Left Wrist", FindBone("mixamorig:LeftHand")},
            {"Right Wrist", FindBone("mixamorig:RightHand")},
            //{"Left Palm", FindBone("mixamorig:LeftHand")},
            //{"Right Palm", FindBone("mixamorig:RightHand")},
            {"Left Hip", FindBone("mixamorig:LeftUpLeg")},
            {"Right Hip", FindBone("mixamorig:RightUpLeg")},
            {"Left Knee", FindBone("mixamorig:LeftLeg")},
            {"Right Knee", FindBone("mixamorig:RightLeg")},
            {"Left Ankle", FindBone("mixamorig:LeftFoot")},
            {"Right Ankle", FindBone("mixamorig:RightFoot")},
            {"Left Foot", FindBone("mixamorig:LeftToeBase")},
            {"Right Foot", FindBone("mixamorig:RightToeBase")},
            {"Neck", FindBone("mixamorig:Neck")},
            {"Back", FindBone("mixamorig:Spine1")},
            {"Waist", FindBone("mixamorig:Spine")}
        };

        initialRotations = new Dictionary<string, Quaternion>();

        foreach (var pair in jointMap)
        {
            initialRotations[pair.Key] = pair.Value.localRotation;
            GameObject cnt = Instantiate(txt, canvas); cnt.GetComponent<TMP_Text>().text = pair.Key;
            Objects[pair.Key] = cnt.transform;
        }
        Destroy(txt.gameObject);
    }

    public void UpdateJson()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "test_model.json");

            poseData = JsonConvert.DeserializeObject<PoseData_Model>(File.ReadAllText(path));


            // Regulation
            foreach (var a in poseData.frames)
            {
                foreach (var b in a.pts.Values)
                {
                    Mx = Mathf.Max(Mx, b.x); mx = Mathf.Min(mx, b.x);
                    My = Mathf.Max(My, b.y); my = Mathf.Min(my, b.y);
                    Mz = Mathf.Max(Mz, b.z); mz = Mathf.Min(mz, b.z);
                }
            }
            Mx = 1 / (Mx - mx);
            My = 1 / (My - my);
            Mz = 1 / (Mz - mz);
        }
        catch (Exception e)
        {
            Debug.Log("Load Fail!");
        }
    }

    public void UpdatePose()
    {
        if (poseData == null) { Debug.Log("No Json!"); return; }
        if (currentFrame > poseData.frames.Count) { Debug.Log("More than limit!"); return; }
        ApplyPose(poseData.frames[currentFrame]);
    }

    [SerializeField] Vector3 TestPos;
    void ApplyPose(Frame_Model frame)
    {
        foreach (var j in frame.pts)
        {
            if (!jointMap.ContainsKey(j.Key)) continue;
            Vector3 xyz = new Vector3(
                (j.Value.x - mx) * Mx,
                (j.Value.y - my) * My,
                (j.Value.z - mz) * Mz * 2f - 1f);
            Quaternion relativeRot = Quaternion.FromToRotation(TestPos, xyz);
            jointMap[j.Key].localRotation = initialRotations[j.Key] * relativeRot;
            //Objects[j.Key].localPosition = xyz;
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
