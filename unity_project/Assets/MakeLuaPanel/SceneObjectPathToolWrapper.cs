using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SceneObjectPathToolWrapper
{
    [MenuItem("MakeLuaPanel/Get Selected Object Path")]
    static void GetGameObjectPath()
    {
        var selected = Selection.activeGameObject;

        string path = SceneObjectPathTool.GetGameObjectPath(selected);

        Debug.LogWarning(path);
    }

}
