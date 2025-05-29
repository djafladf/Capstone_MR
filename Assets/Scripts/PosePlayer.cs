using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Unity.VisualScripting;

public class PosePlayer : MonoBehaviour
{
    public string DeviceId;
    public Dictionary<int, Transform> jointMap;
    private Dictionary<int, Quaternion> initialRotations;
    private Dictionary<int, Vector3> landmarkPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> LastValue = new Dictionary<int, Vector3>();

    Animator anim;
    Coroutine Posecor = null;

    [SerializeField] private Vector3 Agle;
    [SerializeField] private Vector3 Corr_Position = Vector3.one;
    [SerializeField] private TextAsset idealPoseJson;
    [SerializeField] private TextMeshProUGUI scoreText;

    private Quaternion corr = Quaternion.Euler(Vector3.zero);
    private Vector3 StartPos;
    [SerializeField] private float Threshold = 1.5f;
    [SerializeField] private float GapVar = -1.8f;
    private int frameIndex = 0;
    private float liveScore = 100f;

    private List<IdealFrame> idealPose;

    float yLF, yRF;

    private void OnValidate()
    {
        SpineCorrRotation = Quaternion.Euler(SpineCorrTarget);
    }

    void Start()
    {
        if (name.Contains("Device")) { Tongsin.inst.pp.Add(this); Debug.Log($"Register {name}"); }
        anim = GetComponent<Animator>();
        DeviceId = name;

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
        yLF = jointMap[27].position.y; yRF = jointMap[28].position.y;

        foreach (var joint in jointMap.Keys) LastValue[joint] = Vector3.right;
        LastValue[1] = Vector3.right;

        StartPos = transform.position;
        corr = Quaternion.Euler(Agle);
        SpineCorrRotation = Quaternion.Euler(SpineCorrTarget);
        LoadPoseJson();
    }

    void LoadPoseJson()
    {
        idealPoseJson = Resources.Load<TextAsset>("ideal_pose");

        if (idealPoseJson == null)
        {
            Debug.LogError("[PosePlayer] Resources 폴더에 'ideal_pose.json'이 없습니다.");
            return;
        }

        try
        {
            idealPose = JsonConvert.DeserializeObject<List<IdealFrame>>(idealPoseJson.text);
            Debug.Log("[PosePlayer] ideal_pose.json 정상 로딩 완료. 프레임 수: " + idealPose.Count);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[PosePlayer] JSON 파싱 실패: " + e.Message);
        }
    }

    public void UpdatePose()
    {
        if (Posecor == null) Posecor = StartCoroutine(ApplyPose());
    }

    IEnumerator ApplyPose()
    {
        var WFS = new WaitForSeconds(0.05f);
        print($"Start Pose of {name}");
        var user = Tongsin.inst.poseData[DeviceId];
        foreach (var kp in user)
        {
            Vector3 pos = new Vector3(kp.x * Corr_Position.x, kp.y * Corr_Position.y, kp.z * Corr_Position.z);
            landmarkPositions[kp.id] = pos;
        }
        PoseSub(true);

        while (true)
        {
            yield return WFS;

            if (!Tongsin.inst.poseData.ContainsKey(DeviceId)) continue;

            user = Tongsin.inst.poseData[DeviceId];

            foreach (var kp in user)
            {
                Vector3 pos = new Vector3(kp.x * Corr_Position.x, kp.y * Corr_Position.y, kp.z * Corr_Position.z);
                landmarkPositions[kp.id] = pos;
            }

            PoseSub();

            if (frameIndex >= idealPose.Count) frameIndex = 0;

            var ideal = idealPose[frameIndex].pose;
            float error = CalculatePoseDistance(landmarkPositions, ideal);

            if (error < 0.5f)
                liveScore += 10f * Time.deltaTime;
            else
                liveScore -= error * 5f * Time.deltaTime;

            liveScore = Mathf.Clamp(liveScore, 0f, 100f);

            if (scoreText != null)
            {
                scoreText.text = $"{liveScore:F1}";

                if (liveScore >= 50f)
                    scoreText.color = Color.green;
                else if (liveScore >= 20f)
                    scoreText.color = new Color(1f, 0.5f, 0f); // 주황
                else
                    scoreText.color = Color.red;
            }

            frameIndex++;
        }
    }

    float CalculatePoseDistance(Dictionary<int, Vector3> user, List<Joint> ideal)
    {
        float total = 0f;
        int count = 0;

        foreach (var joint in ideal)
        {
            if (!user.ContainsKey(joint.index)) continue;
            Vector3 a = user[joint.index];
            Vector3 b = new Vector3(joint.x, joint.y, joint.z);
            total += Vector3.Distance(a, b);
            count++;
        }

        return count > 0 ? total / count : 0f;
    }

    public float GetLiveScore()
    {
        return liveScore;
    }

    Vector3 SpineTarget = Vector3.right;
    [SerializeField] Vector3 SpineCorrTarget = new Vector3(-1, 2, 1);
    Quaternion SpineCorrRotation;

    float LastHeightGap = 100000;

    Vector3 from, to, dir;
    Quaternion rotation;

    [SerializeField] private bool LockGround = true;
    [SerializeField] private float GroundY = 0f;

    void PoseSub(bool OnInit = false)
    {
        print("!");
        // 허리 회전
        dir = (landmarkPositions[11] - landmarkPositions[12]).normalized;
        rotation = Quaternion.FromToRotation(dir, SpineTarget) * SpineCorrRotation;
        jointMap[17].rotation = rotation;

        for (int i = 11; i <= 14; i++)
        {
            from = landmarkPositions[i];
            to = landmarkPositions[i + 2];
            dir = (to - from).normalized;
            float angleGap = Vector3.Angle(LastValue[i], dir);
            if (angleGap >= Threshold || OnInit)
            {
                rotation = Quaternion.LookRotation(dir) * corr;
                jointMap[i].rotation = rotation;
                LastValue[i] = dir;
            }
        }

        for (int i = 23; i <= 26; i++)
        {
            from = landmarkPositions[i];
            to = landmarkPositions[i + 2];
            dir = (to - from);
            float angleGap = Vector3.Angle(LastValue[i], dir);
            Debug.DrawRay(jointMap[i].position, dir, Color.red, 5f);
            if (angleGap >= Threshold || OnInit)
            {
                rotation = Quaternion.LookRotation(dir) * corr;
                jointMap[i].rotation = rotation;
                LastValue[i] = dir;
            }
        }

        // 종아리 부분은 다르게 처리
        from = landmarkPositions[28];
        to = landmarkPositions[32];
        dir = (to - from).normalized;
        float ag1 = Vector3.Angle(LastValue[28], dir);
        if (ag1 >= Threshold || OnInit)
        {
            jointMap[28].rotation = Quaternion.LookRotation(dir) * corr;
            LastValue[28] = dir;
        }

        from = landmarkPositions[27];
        to = landmarkPositions[31];
        float ag2 = Vector3.Angle(LastValue[27], dir);
        if (ag2 >= Threshold || OnInit)
        {
            jointMap[27].rotation = Quaternion.LookRotation(dir) * corr;
            LastValue[27] = dir;
        }

        /*float cLF = jointMap[27].position.y, cRF = jointMap[28].position.y;

        if (cLF < cRF)
        {
            Vector3 ttmp = transform.position;
            ttmp.y -= (cLF - yLF);
            transform.position = ttmp;
        }
        else
        {
            Vector3 ttmp = transform.position;
            ttmp.y -= (cRF - yRF);
            transform.position = ttmp;
        }*/

        if (LockGround)
        {
            transform.position = new Vector3(transform.position.x, GroundY, transform.position.z);
        }
    }
}

[System.Serializable]
public class IdealFrame
{
    public int frame;
    public List<Joint> pose;
}

[System.Serializable]
public class Joint
{
    public int index;
    public float x;
    public float y;
    public float z;
}
