using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class PosePlayerGuide : MonoBehaviour
{
    public Dictionary<int, Transform> jointMap;
    private Dictionary<int, Quaternion> initialRotations;
    public Dictionary<int, Vector3> LastValue = new Dictionary<int, Vector3>();
    private List<IdealFrame> idealPose;

    [SerializeField] float Gaap;
    private int frameIndex = 0;
    [SerializeField] private float Threshold = 0;
    private Quaternion corr;

    [SerializeField] private TextAsset idealPoseJson;
    [SerializeField] private Vector3 Agle;
    [SerializeField] private Vector3 Corr_Position = Vector3.one;

    private void OnValidate()
    {
        corr = Quaternion.Euler(Agle);
    }

    void Start()
    {
        Animator anim = GetComponent<Animator>();
        corr = Quaternion.Euler(Agle);

        jointMap = new Dictionary<int, Transform>
        {
            {11, anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)},
            {12, anim.GetBoneTransform(HumanBodyBones.RightUpperArm)},
            {13, anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)},
            {14, anim.GetBoneTransform(HumanBodyBones.RightLowerArm)},
            {15, anim.GetBoneTransform(HumanBodyBones.LeftHand)},
            {16, anim.GetBoneTransform(HumanBodyBones.RightHand)},
            {17, anim.GetBoneTransform(HumanBodyBones.Spine)},
            {23, anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)},
            {24, anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)},
            {25, anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)},
            {26, anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)},
            {27, anim.GetBoneTransform(HumanBodyBones.LeftFoot)},
            {28, anim.GetBoneTransform(HumanBodyBones.RightFoot)},
        };

        print($"{name} : {jointMap[11].position - jointMap[27].position}");

        StartPos = transform.position;
        LoadPoseJson();
        StartCoroutine(PlayIdealPose());
    }

    void LoadPoseJson()
    {
        idealPoseJson = Resources.Load<TextAsset>("ideal_pose");

        if (idealPoseJson == null)
        {
            Debug.LogError("[Guide] ideal_pose.json을 Resources 폴더에 넣어주세요.");
            return;
        }

        idealPose = JsonConvert.DeserializeObject<List<IdealFrame>>(idealPoseJson.text);
        Debug.Log("[Guide] ideal_pose.json 로드 완료. 프레임 수: " + idealPose.Count);
    }

    IEnumerator PlayIdealPose()
    {
        var WFS = new WaitForSeconds(0.05f);
        var frame = idealPose[0];
        ApplyPose(frame.pose,true);
        while (true)
        {
            if (idealPose == null || idealPose.Count == 0) yield break;
            frame = idealPose[frameIndex % idealPose.Count];

            ApplyPose(frame.pose);

            frameIndex++;
            yield return WFS;
        }
    }

    [SerializeField] bool Test = false;
    void ApplyPose(List<Joint> joints,bool I = false)
    {
        Dictionary<int, Vector3> landmarks = new Dictionary<int, Vector3>();

        foreach (var joint in joints)
        {
            Vector3 pos = new Vector3(joint.x * Corr_Position.x, joint.y * Corr_Position.y, joint.z * Corr_Position.z);
            if (Test) pos += new Vector3(Random.Range(-0.001f, 0.001f), Random.Range(-0.001f, 0.001f), Random.Range(-0.05f, 0.001f));
            landmarks[joint.index] = pos;
        }

        ApplyRotation(landmarks,I);
    }

    float GapOfLeg = 100000, CurGap;
    Vector3 StartPos;

    void ApplyRotation(Dictionary<int, Vector3> pos,bool OnInit = false)
    {
        Vector3 from, to, direction;
        Quaternion rotation;

        for (int i = 11; i <= 14; i++)
        {
            if (!pos.ContainsKey(i) || !pos.ContainsKey(i + 2)) continue;
            from = pos[i];
            to = pos[i + 2];
            direction = (to - from).normalized;

            float angleGap = Vector3.Angle(LastValue.ContainsKey(i) ? LastValue[i] : direction, direction);
            if (angleGap >= Threshold)
            {
                rotation = Quaternion.LookRotation(direction) * corr;
                if (jointMap.ContainsKey(i)) jointMap[i].rotation = rotation;
                LastValue[i] = direction;
            }
        }

        for (int i = 23; i <= 26; i++)
        {
            if (!pos.ContainsKey(i) || !pos.ContainsKey(i + 2)) continue;
            from = pos[i];
            to = pos[i + 2];
            direction = (to - from).normalized;

            float angleGap = Vector3.Angle(LastValue.ContainsKey(i) ? LastValue[i] : direction, direction);
            if (angleGap >= Threshold)
            {
                rotation = Quaternion.LookRotation(direction) * corr;
                if (jointMap.ContainsKey(i)) jointMap[i].rotation = rotation;
                LastValue[i] = direction;
            }
        }

        for (int i = 27; i <= 28; i++)
        {
            if (!pos.ContainsKey(i) || !pos.ContainsKey(i + 2) || !pos.ContainsKey(i + 4)) continue;
            from = pos[i];
            to = 0.5f * (pos[i + 2] + pos[i + 4]);
            direction = (to - from).normalized;

            float ag = Vector3.Angle(LastValue.ContainsKey(i) ? LastValue[i] : direction, direction);
            if (ag >= Threshold)
            {
                rotation = Quaternion.LookRotation(direction) * corr;
                if (jointMap.ContainsKey(i)) jointMap[i].rotation = rotation;
                LastValue[i] = direction;
            }
        }

        // 종아리 부분은 다르게 처리
        from = pos[28];
        to = 0.5f * (pos[30] + pos[32]);
        direction = (to - from).normalized;
        float ag1 = Vector3.Angle(LastValue[28], direction);
        if (ag1 >= Threshold || OnInit)
        {
            jointMap[28].rotation = Quaternion.LookRotation(direction) * corr;
            LastValue[28] = direction;
        }

        from = pos[27];
        to = 0.5f * (pos[29] + pos[31]);
        direction = (to - from).normalized;
        float ag2 = Vector3.Angle(LastValue[27], direction);
        if (ag2 >= Threshold || OnInit)
        {
            jointMap[27].rotation = Quaternion.LookRotation(direction) * corr;
            LastValue[27] = direction;
        }

        CurGap = -0.5f * ((pos[23].y - pos[27].y) + (pos[24].y - pos[28].y));
        if (OnInit) GapOfLeg = CurGap;
        float Gap = GapOfLeg - CurGap;
        transform.position = new Vector3(StartPos.x, StartPos.y - (Gap * Gaap * transform.localScale.y), StartPos.z);
    }
}
