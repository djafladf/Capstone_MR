using System.Collections.Generic;
using UnityEngine;

public class PoseComparer : MonoBehaviour
{
    [SerializeField] private PosePlayer userPose;
    [SerializeField] private PosePlayerGuide guidePose;
    [SerializeField] private Transform rootObject;
    [SerializeField] private float angleThreshold = 20f;

    private Dictionary<int, string> jointToPart = new Dictionary<int, string>
    {
        {23, "LeftUpLeg"},
        {24, "RightUpLeg"},
        {25, "LeftLeg"},
        {26, "RightLeg"}
    };

    private Dictionary<string, Material> partMaterials = new Dictionary<string, Material>();
    private Dictionary<string, Color> originalColors = new Dictionary<string, Color>();

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
    }

    void Update()
    {
        foreach (var pair in jointToPart)
        {
            int jointId = pair.Key;
            string partName = pair.Value;

            if (!userPose.jointMap.ContainsKey(jointId) || !guidePose.jointMap.ContainsKey(jointId))
                continue;

            Quaternion userRot = userPose.jointMap[jointId].rotation;
            Quaternion guideRot = guidePose.jointMap[jointId].rotation;

            float angle = Quaternion.Angle(userRot, guideRot);

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
}
