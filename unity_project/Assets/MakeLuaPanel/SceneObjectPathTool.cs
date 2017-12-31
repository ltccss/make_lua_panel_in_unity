using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SceneObjectPathTool 
{

    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + ProcessObjectName(obj);


        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + ProcessObjectName(obj) + path;
        }
        return path;
    }

    /// <summary>
    /// trimSlash : 是否修剪掉开头的斜杠
    /// </summary>
    /// <returns></returns>
    public static string GetObjectRelativePath(GameObject targetObject, GameObject relativeObject, bool trimSlash = true)
    {
        //目标物体的transform路径，从子到父
        List<Transform> targetTransformPathList = new List<Transform>();

        //相对物体的transform路径，从子到父
        List<Transform> relativeTransformPathList = new List<Transform>();

        Transform targetTrans = targetObject.transform;
        Transform relativeTrans = relativeObject.transform;

        do
        {
            targetTransformPathList.Add(targetTrans);
            targetTrans = targetTrans.parent;

        }
        while(targetTrans != null);

        do
        {
            relativeTransformPathList.Add(relativeTrans);
            relativeTrans = relativeTrans.parent;
        }
        while (relativeTrans != null);


        int targetDepth = -1;
        int relativeDepth = -1;
        bool isFind = false;
        for (int i = 0; i < relativeTransformPathList.Count; i++)
        {
            for (int j = 0; j < targetTransformPathList.Count; j++)
            {
                if (relativeTransformPathList[i] == targetTransformPathList[j])
                {
                    targetDepth = j;
                    relativeDepth = i;
                    isFind = true;
                    break;
                }
            }
            if (isFind)
            {
                break;
            }
        }


        string relativePath = "";

        //先从relativeTransform开始向上访问
        for (int i = 0; i < relativeDepth; i++)
        {
            relativePath = "/" + ".." + relativePath;
        }

        string targetPath = "";
        for (int i = 0; i < targetDepth; i++)
        {
            targetPath = "/" + ProcessObjectName(targetTransformPathList[i].gameObject) + targetPath;
        }

        string path = relativePath + targetPath;

        if (trimSlash && path.Length > 0 && path[0] == '/')
        {
            path = path.Substring(1, path.Length - 1);   
        }

        //Debug.LogWarning("path : " + path);
        return path;
    }

    /// <summary>
    /// 处理下同名物体的情况
    /// </summary>
    static string ProcessObjectName(GameObject obj)
    {
        if (obj.transform.parent != null)
        {
            int repeatCount = 0;
            int index = 0;
            foreach (Transform trans in obj.transform.parent)
            {
                if (trans.gameObject.name == obj.name)
                {
                    repeatCount++;
                    if (trans.gameObject == obj)
                    {
                        index = repeatCount - 1;

                    }
                }
            }
            if (index > 0 || repeatCount > 1)
            {
                return string.Format("{0}[{1}]", obj.name, index);
            }
            else
            {
                return obj.name;
            }
        }
        else
        {
            return obj.name;
        }
    }

    /// <summary>
    /// 对应的寻找方法
    /// </summary>
    public static Transform FindEx(Transform root, string path)
    {
        if (root == null)
        {
            throw new NullReferenceException("root cannot be null");
        }
        if (path == string.Empty)
        {
            return root;
        }
        if (path != null)
        {
            char[] separator = new char[] { '/' };
            string[] strArray = path.Split(separator);
            if (root == null)
            {
                return null;
            }
            Transform parent = root;
            for (int i = 0; (i < strArray.Length) && (parent != null); i++)
            {
                string childName = strArray[i];
                if (childName.Length > 0)
                {
                    if (childName == "..")
                    {
                        parent = parent.parent;
                        if (parent == null)
                        {
                            return null;
                        }
                    }
                    else if (childName[childName.Length - 1] != ']')
                    {
                        parent = FindChildTransform(parent, childName, 0);
                    }
                    else
                    {
                        int length = childName.LastIndexOf("[", (int)(childName.Length - 2));
                        string str2 = childName.Substring(length + 1, (childName.Length - 2) - length);
                        parent = FindChildTransform(parent, childName.Substring(0, length), Convert.ToInt32(str2));
                    }
                }
                else if (i > 0)
                {
                    Debug.LogWarning("FindEx : path is not right - " + path);
                }
            }
            if (parent != null)
            {
                return parent;
            }
        }
        return null;
    }
    private static Transform FindChildTransform(Transform parent, string childName, int childIndex)
    {
        Transform child = null;
        if (parent != null)
        {
            int num = -1;
            for (int i = 0; i < parent.childCount; i++)
            {
                child = parent.GetChild(i);
                if ((child.name.Length == childName.Length) && (child.name == childName))
                {
                    num++;
                    if (num == childIndex)
                    {
                        return child;
                    }
                }
            }
        }
        return null;
    }



}
