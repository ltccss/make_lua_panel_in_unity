﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
[MLPSerializableClass][Serializable]
public class CustomData
{
    public float x;
    public float y;
    public float z;

    public Vector3 a;
    public Color ss;
}
 * **/
public class MyContent1 :MakeLuaPanel//: MonoBehaviour 
{
    [MLPException()]
    public GameObject igonreMePls;
    public UnityEngine.UI.Text text1;
    public UnityEngine.UI.Text text2;
    public List<RectTransform> rectTransformList = new List<RectTransform>();
    [MLPAnnotation("i am daddy not mami")]
    public Transform[] daddyArray;

    //public List<CustomData> dataList;

    public int[][] arr;

    //[ContextMenu("aaa")]
    public void Test()
    {
        //var arr = this.aaa.gameObject.GetComponents<Component>();
        //for (int i = 0; i < arr.Length; i++)
        //{
        //    Debug.LogWarning(arr[i].GetType());
        //}
    }

    void f()
    {
        //you can write some temple code here
        //sometimes you may forget the full name of something in C# But the IDE knows

        this.text1.text = "";
        //this.text1.transform
        //rectTransformList[0].sizeDelta ;
    }

}
