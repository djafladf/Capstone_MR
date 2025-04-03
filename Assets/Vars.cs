using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PoseData
{
    [JsonProperty("frames")]
    public List<Frame> frames;
}

[Serializable]
public class Frame
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