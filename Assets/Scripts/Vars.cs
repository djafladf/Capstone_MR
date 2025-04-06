using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PoseData_Model
{
    [JsonProperty("frames")]
    public List<Frame_Model> frames;
}

[Serializable]
public class Frame_Model
{
    [JsonProperty("pts")]
    public Dictionary<string, Point> pts;
}

[Serializable]
public class Point
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class PoseData_User
{
    public List<landmarks> landmarks;
}

[System.Serializable]
public class landmarks
{
    public int id;
    public float x;
    public float y;
    public float z;

    public void Out()
    {
        Debug.Log($"{id} : {x},{y},{z}");
    }
}

