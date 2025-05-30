using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseComparer_saved : MonoBehaviour
{
    [SerializeField] private PosePlayerGuide userPose;
    [SerializeField] private PosePlayerGuide guidePose;
    [SerializeField] private Transform rootObject;
    [SerializeField] private float angleThreshold = 20f;
    [SerializeField] private float compareInterval = 0.05f;

    private Dictionary<int, string> jointToPart = new Dictionary<int, string>
    {
        {23, "LeftUpLeg"},
        {24, "RightUpLeg"},
        {25, "LeftLeg"},
        {26, "RightLeg"}
    };

    private Dictionary<string, Material> partMaterials = new Dictionary<string, Material>();
    private Dictionary<string, Color> originalColors = new Dictionary<string, Color>();

    private List<IdealFrame> userFrames;  
    private List<IdealFrame> guideFrames;
    private int frameIndex = 0;

    void Start()
    {
        foreach (var pair in jointToPart)
        {
            string partName = pair.Value;
            Transform part = rootObject.Find(partName);

            if (part != null)
            {
                Renderer rend = part.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = rend.material;
                    partMaterials[partName] = mat;

                    if (mat.HasProperty("_BaseColor"))
                        originalColors[partName] = mat.GetColor("_BaseColor");
                    else
                        originalColors[partName] = mat.color;
                }
            }
        }

        userFrames = userPose.GetIdealFrames();
        guideFrames = guidePose.GetIdealFrames();

        if (userFrames == null || guideFrames == null)
        {
            Debug.LogError("[PoseComparer_saved] JSON 데이터가 없습니다.");
            return;
        }

        StartCoroutine(CompareRoutine());
    }

    IEnumerator CompareRoutine()
    {
        var wait = new WaitForSeconds(compareInterval);
        while (frameIndex < userFrames.Count && frameIndex < guideFrames.Count)
        {
            CompareFrame(userFrames[frameIndex], guideFrames[frameIndex]);
            frameIndex++;
            yield return wait;
        }
    }

    void CompareFrame(IdealFrame userFrame, IdealFrame guideFrame)
    {
        Dictionary<int, Vector3> userDirs = GetJointDirections(userFrame.pose);
        Dictionary<int, Vector3> guideDirs = GetJointDirections(guideFrame.pose);

        foreach (var pair in jointToPart)
        {
            int jointId = pair.Key;
            string partName = pair.Value;

            if (!userDirs.ContainsKey(jointId) || !guideDirs.ContainsKey(jointId))
                continue;

            float angle = Vector3.Angle(userDirs[jointId], guideDirs[jointId]);

            if (partMaterials.TryGetValue(partName, out Material mat))
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    if (angle < 10f)
                        mat.SetColor("_BaseColor", originalColors[partName]);
                    else if (angle < 20f)
                        mat.SetColor("_BaseColor", Color.yellow);
                    else if (angle < 40f)
                        mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f));
                    else
                        mat.SetColor("_BaseColor", Color.red);
                }
                else
                {
                    if (angle < 10f)
                        mat.color = originalColors[partName];
                    else if (angle < 20f)
                        mat.color = Color.yellow;
                    else if (angle < 40f)
                        mat.color = new Color(1f, 0.5f, 0f);
                    else
                        mat.color = Color.red;
                }
            }
        }
    }

    Dictionary<int, Vector3> GetJointDirections(List<Joint> joints)
    {
        Dictionary<int, Vector3> result = new Dictionary<int, Vector3>();
        Dictionary<int, Vector3> posMap = new Dictionary<int, Vector3>();

        foreach (var joint in joints)
            posMap[joint.index] = new Vector3(joint.x, joint.y, joint.z);

        for (int i = 23; i <= 26; i++)
        {
            if (posMap.ContainsKey(i) && posMap.ContainsKey(i + 2))
            {
                Vector3 from = posMap[i];
                Vector3 to = posMap[i + 2];
                Vector3 dir = (to - from).normalized;
                result[i] = dir;
            }
        }

        return result;
    }
}
