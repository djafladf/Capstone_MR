using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class PosePlayer : MonoBehaviour
{
    public string DeviceId;
    private Dictionary<int, Transform> jointMap;
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
    private float Threshold = 0;
    private float GapVar = -2.3f;
    private int frameIndex = 0;
    private float liveScore = 100f;

    private List<IdealFrame> idealPose; // 수정된 구조

    void Start()
    {
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

        StartPos = transform.position;
        corr = Quaternion.Euler(Agle);
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

        while (true)
        {
            yield return WFS;

            if (!Tongsin.inst.poseData.ContainsKey(DeviceId)) continue;

            var user = Tongsin.inst.poseData[DeviceId];

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
                scoreText.text = $"Score: {liveScore:F1}";

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

    void PoseSub()
    {
        Vector3 from, to, direction;
        Quaternion rotation;

        Vector3 p1 = landmarkPositions[11], p2 = landmarkPositions[24], p3 = landmarkPositions[12], p4 = landmarkPositions[23];
        float midsub = 1.0f / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
        float sub1 = (p1.x * p2.y - p1.y * p2.x), sub2 = (p3.x * p4.y - p3.y * p4.x);

        landmarkPositions[17] = new Vector3((sub1 * (p3.x - p4.x) - (p1.x - p2.x) * sub2) * midsub,
            (sub1 * (p3.y - p4.y) - (p1.y - p2.y) * sub2),
            (p1.z + p2.z + p3.z + p4.z) * 0.25f);

        for (int i = 11; i <= 14; i++)
        {
            from = landmarkPositions[i];
            to = landmarkPositions[i + 2];
            direction = (to - from).normalized;
            float angleGap = Vector3.Angle(LastValue.ContainsKey(i) ? LastValue[i] : direction, direction);
            if (angleGap >= Threshold)
            {
                rotation = Quaternion.LookRotation(direction) * corr;
                jointMap[i].rotation = rotation;
                LastValue[i] = direction;
            }
        }

        for (int i = 23; i <= 26; i++)
        {
            from = landmarkPositions[i];
            to = landmarkPositions[i + 2];
            direction = (to - from).normalized;
            float angleGap = Vector3.Angle(LastValue.ContainsKey(i) ? LastValue[i] : direction, direction);
            if (angleGap >= Threshold)
            {
                rotation = Quaternion.LookRotation(direction) * corr;
                jointMap[i].rotation = rotation;
                LastValue[i] = direction;
            }
        }

        from = landmarkPositions[28];
        to = 0.5f * (landmarkPositions[30] + landmarkPositions[32]);
        direction = (to - from).normalized;
        float ag1 = Vector3.Angle(LastValue.ContainsKey(28) ? LastValue[28] : direction, direction);
        if (ag1 >= Threshold)
        {
            jointMap[28].rotation = Quaternion.LookRotation(direction) * corr;
            LastValue[28] = direction;
        }

        from = landmarkPositions[27];
        to = 0.5f * (landmarkPositions[29] + landmarkPositions[31]);
        direction = (to - from).normalized;
        float ag2 = Vector3.Angle(LastValue.ContainsKey(27) ? LastValue[27] : direction, direction);
        if (ag2 >= Threshold)
        {
            jointMap[27].rotation = Quaternion.LookRotation(direction) * corr;
            LastValue[27] = direction;
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
