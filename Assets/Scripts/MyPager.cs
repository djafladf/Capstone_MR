using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPager : MonoBehaviour
{
    [SerializeField] List<GameObject> Pages;
    int CurOpen = 0;

    public void OpenPage(int PageNum)
    {
        Pages[CurOpen].SetActive(false); CurOpen = PageNum; Pages[CurOpen].SetActive(true);
    }
}
