using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PosePlayer : MonoBehaviour
{
	private Dictionary<int, Transform> jointMap;
	private Dictionary<int, Quaternion> initialRotations;

	Animator anim;

	void Start()
	{
		
		anim = GetComponent<Animator>();
        jointMap = new Dictionary<int, Transform>
		{
			//{0,  FindBone("mixamorig:Head")},

			{11, anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)},        // Left Shoulder
			{12, anim.GetBoneTransform(HumanBodyBones.RightUpperArm)},       // Right Shoulder
			{13, anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)},    // Left Elbow
			{14, anim.GetBoneTransform(HumanBodyBones.RightLowerArm)},   // Right Elbow
			{15, anim.GetBoneTransform(HumanBodyBones.LeftHand)},       // Left Wrist
			{16, anim.GetBoneTransform(HumanBodyBones.RightHand)},      // Right Wrist

			{17, anim.GetBoneTransform(HumanBodyBones.Spine) },
			//{18, FindBone($"mixamorig:Hips") },

			{23, anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)},
			{24, anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)},
			{25, anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)},
			{26, anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)},
			{27, anim.GetBoneTransform(HumanBodyBones.LeftFoot)},
			{28,  anim.GetBoneTransform(HumanBodyBones.RightFoot)},
		};
		Vector3 sub = Vector3.Cross((jointMap[12].position - jointMap[11].position).normalized, Vector3.up);
		StartPos = transform.position;
    }

	public void UpdatePose()
	{
		if(Posecor == null) Posecor = StartCoroutine(ApplyPose());
	}

	Vector3 StartPos;
    [SerializeField] private Vector3 Agle;
	[SerializeField] private Vector3 Corr_Position;
    private void OnValidate()
    {
		corr = Quaternion.Euler(Agle);
    }

	Coroutine Posecor = null;
    Quaternion corr = Quaternion.Euler(Vector3.zero);

	[SerializeField] float GapVar;
	IEnumerator ApplyPose()
	{
		print($"{name}'s PoseStart!");
		var WFS = new WaitForSeconds(0.1f);
		while (true)
		{
			yield return WFS;
			Dictionary<int, Vector3> landmarkPositions = new Dictionary<int, Vector3>();
			foreach (var kp in Tongsin.inst.poseData.landmarks)
			{
				
				landmarkPositions[kp.id] = new Vector3(kp.x * Corr_Position.x, kp.y * Corr_Position.y, kp.z * Corr_Position.z);
			}
            Vector3 p1 = landmarkPositions[11], p2 = landmarkPositions[24], p3 = landmarkPositions[12], p4 = landmarkPositions[23];

            float midsub = 1.0f / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
            float sub1 = (p1.x * p2.y - p1.y * p2.x), sub2 = (p3.x * p4.y - p3.y * p4.x);

            landmarkPositions[17] = new Vector3((sub1 * (p3.x - p4.x) - (p1.x - p2.x) * sub2) * midsub,
                (sub1 * (p3.y - p4.y) - (p1.y - p2.y) * sub2),
                (p1.z + p2.z + p3.z + p4.z) * 0.25f);

			
            Vector3 from, to, direction;
			Quaternion rotation;

			// 허리
			from = landmarkPositions[17];
			to = (landmarkPositions[11] + landmarkPositions[12]) * 0.5f;
			direction = (from - to).normalized;
			rotation = Quaternion.LookRotation(direction) * corr;
			//jointMap[17].rotation = rotation;


			// 상반 ( 허리 제외 )
			for (int i = 11; i <= 14; i++)
			{
				from = landmarkPositions[i];
				to = landmarkPositions[i + 2];
				direction = (to - from).normalized;
				rotation = Quaternion.LookRotation(direction) * corr;
				jointMap[i].rotation = rotation;
			}


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


			
			if(Tongsin.inst.GapOfLeg != -1)
			{
				float Gap = Tongsin.inst.GapOfLeg - Tongsin.inst.CurGap;
				transform.position = new Vector3(StartPos.x, StartPos.y - (Gap * GapVar * transform.localScale.y), StartPos.z);
            }
		}
    }
}
