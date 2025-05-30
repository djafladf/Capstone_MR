using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTest : MonoBehaviour
{
    public void SceneChange_Single()
    {
        SceneManager.LoadScene("SingleGame");
    }
    public void SceneChange_Multi()
    {
        SceneManager.LoadScene("MultiGame");
    }
}
