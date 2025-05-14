using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

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

	String r = "Right";
	String l = "Left";

	void Start()
	{
		jointMap = new Dictionary<int, Transform>
		{
			{0,  FindBone("mixamorig:Head")},

			{11, FindBone($"mixamorig:{l}Arm")},        // Left Shoulder
			{12, FindBone($"mixamorig:{r}Arm")},       // Right Shoulder
			{13, FindBone($"mixamorig:{l}ForeArm")},    // Left Elbow
			{14, FindBone($"mixamorig:{r}ForeArm")},   // Right Elbow
			{15, FindBone($"mixamorig:{l}Hand")},       // Left Wrist
			{16, FindBone($"mixamorig:{r}Hand")},      // Right Wrist

			{23, FindBone($"mixamorig:{l}UpLeg")},
			{24, FindBone($"mixamorig:{r}UpLeg")},
			{25, FindBone($"mixamorig:{l}Leg")},
			{26, FindBone($"mixamorig:{r}Leg")},
			{27, FindBone($"mixamorig:{l}Foot")},
			{28, FindBone($"mixamorig:{r}Foot")},
		};
	}

	public void UpdatePose(string text)
	{
		poseData = JsonConvert.DeserializeObject<PoseData_User>(text);

		ApplyPose(poseData.landmarks);
	}

    [SerializeField] private Vector3 Agle;

    private void OnValidate()
    {
		corr = Quaternion.Euler(Agle);
    }


    Quaternion corr = Quaternion.Euler(Vector3.zero);
	void ApplyPose(List<landmarks> frame)
	{
		// 필요한 관절 위치 저장
		Dictionary<int, Vector3> landmarkPositions = new Dictionary<int, Vector3>();
		foreach (var kp in frame)
		{
			landmarkPositions[kp.id] = new Vector3(kp.x, kp.y, kp.z);
		}

        // ------------------------------
        // 상완 회전: 어깨 → 팔꿈치
        // ------------------------------
        // 왼팔 상완 (11 → 13)
        if (true)
		{
			Vector3 from = landmarkPositions[11];
			Vector3 to = landmarkPositions[13];
			Vector3 direction = (to - from).normalized;

			Quaternion rotation = Quaternion.LookRotation(direction);
			rotation *= corr; // 모델 회전축 보정
			jointMap[12].rotation = rotation;
		}

		

		// 오른팔 상완 (12 → 14)
		if (true)
		{
			Vector3 from = landmarkPositions[12];
			Vector3 to = landmarkPositions[14];
			Vector3 direction = (to - from).normalized;

			Quaternion rotation = Quaternion.LookRotation(direction);
			rotation *= corr;
            jointMap[11].rotation = rotation;
        }

		// ------------------------------
		// 전완 회전: 팔꿈치 → 손목
		// ------------------------------

		// 왼팔 전완 (13 → 15)
		if (true)
		{
			Vector3 from = landmarkPositions[13];
			Vector3 to = landmarkPositions[15];
			Vector3 direction = (to - from).normalized;

			Quaternion rotation = Quaternion.LookRotation(direction);
            rotation *= corr;
            jointMap[14].rotation = rotation;

        }

		// 오른팔 전완 (14 → 16)
		if (true)
		{
			Vector3 from = landmarkPositions[14];
			Vector3 to = landmarkPositions[16];
			Vector3 direction = (to - from).normalized;

			Quaternion rotation = Quaternion.LookRotation(direction);
            rotation *= corr;
            jointMap[13].rotation = rotation;
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
