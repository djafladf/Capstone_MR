using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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

			{17, FindBone($"mixamorig:Spine") },

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

		if(Posecor == null) Posecor = StartCoroutine(ApplyPose());
	}

    [SerializeField] private Vector3 Agle;
	[SerializeField] private Vector3 Corr_Position;
    private void OnValidate()
    {
		corr = Quaternion.Euler(Agle);
    }

	Coroutine Posecor = null;
    Quaternion corr = Quaternion.Euler(Vector3.zero);

	IEnumerator ApplyPose()
	{
		var WFS = new WaitForSeconds(0.1f);
		while (true)
		{
			yield return WFS;
			Dictionary<int, Vector3> landmarkPositions = new Dictionary<int, Vector3>();
			foreach (var kp in poseData.landmarks)
			{
				landmarkPositions[kp.id] = new Vector3(kp.x * Corr_Position.x, kp.y * Corr_Position.y, kp.z * Corr_Position.z);
			}
			Vector3 from, to, direction;
			Quaternion rotation;
			// 필요한 관절 위치 저장
			/*from = landmarkPositions[17];
			to = (landmarkPositions[11] + landmarkPositions[12]) * 0.5f;
			direction = (from - to).normalized;
			rotation = Quaternion.LookRotation(direction) * corr;
			jointMap[17].rotation = rotation;*/

			// 상반 ( 허리 제외 )
			for (int i = 11; i <= 14; i++)
			{
				from = landmarkPositions[i];
				to = landmarkPositions[i + 2];
				direction = (to - from).normalized;
				rotation = Quaternion.LookRotation(direction) * corr;
				jointMap[i].rotation = rotation;
			}
			// 허리



			// 하반
			for (int i = 23; i <= 28; i++)
			{
				from = landmarkPositions[i];
				to = landmarkPositions[i + 2];
				direction = (to - from).normalized;
				rotation = Quaternion.LookRotation(direction);
				rotation *= corr;
				jointMap[i].rotation = rotation;
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
