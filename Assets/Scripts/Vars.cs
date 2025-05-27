using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Users
{
    [SerializeField]
    public List<UserData> User;
    public Users()
    {
        User = new List<UserData>();
    }
}


[System.Serializable]
public class UserData
{
    public bool gender; // 0 : Man, 1 : Woman
    public string id, pw, name;
    public int age, height, weight;
    public UserData() { }
    public UserData(string id, string pw, string name, int age, int height, int weight, bool gender)
    {
        this.id = id;
        this.pw = pw;
        this.name = name;
        this.age = age;
        this.height = height;
        this.weight = weight;
        this.gender = gender;
    }
}

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
    public string deviceId;
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

