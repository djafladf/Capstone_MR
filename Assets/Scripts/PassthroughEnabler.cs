using UnityEngine;

public class PassthroughEnabler : MonoBehaviour
{
    void Start()
    {
        var layer = FindObjectOfType<OVRPassthroughLayer>();
        if (layer != null)
        {
            layer.enabled = true;
            Debug.Log("Passthrough enabled.");
        }
        else
        {
            Debug.LogWarning("OVRPassthroughLayer not found.");
        }
    }
}
