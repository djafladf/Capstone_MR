using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DonutSub : MonoBehaviour
{
    [SerializeField] List<string> Name;
    [SerializeField] List<int> Counts;
    [SerializeField] Color[] Colors;
    [SerializeField] Image im;
    [SerializeField] RectTransform DonutIndexs;
    

    private void Awake()
    {
        
        float l = 1f/Counts.Sum();
        float[] tl = new float[Colors.Length];
        tl[0] = l * Counts[0];
        for (int i = 1; i < Colors.Length; i++) tl[i] = tl[i-1] +  l * Counts[i];
        im.material = new Material(im.material);

        im.material.SetColorArray("_Colors", Colors);
        im.material.SetFloatArray("_Amounts", tl);
        im.material.SetInt("_ColorNum", Colors.Length);

        var cnt = DonutIndexs.GetChild(0);
        cnt.GetComponent<Image>().color = Colors[0];
        cnt.GetChild(0).GetComponent<TMP_Text>().text = Name[0];
        for(int i = 1; i < Colors.Length; i++)
        {
            var tmp = Instantiate(cnt, DonutIndexs);
            tmp.GetComponent<Image>().color = Colors[i];
            tmp.GetChild(0).GetComponent<TMP_Text>().text = Name[i];
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(DonutIndexs);

    }

    private void OnApplicationQuit()
    {
        Destroy(im.material);
    }
}
