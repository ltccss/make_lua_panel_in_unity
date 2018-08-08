using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MakeLuaPanelMenu
{
    [MenuItem("MakeLuaPanel/Get Selected Object Path")]
    static void GetGameObjectPath()
    {
        var selected = Selection.activeGameObject;

        string path = SceneObjectPathTool.GetGameObjectPath(selected);

        Debug.LogWarning(path);
    }

    [MenuItem("MakeLuaPanel/Backup And Remove MLPComponents")]
    static void BackupAndRemoveMLPComponents()
    {
        MLPPrefabTool.BackupAndRemoveMLPComponents();
    }

    [MenuItem("MakeLuaPanel/Restore MLPComponents")]
    static void RestoreMLPConponents()
    {
        MLPPrefabTool.RestoreMLPComponents();
    }
    [MenuItem("MakeLuaPanel/ClearTempPrefabs")]
    static void ClearTempPrefabs()
    {
        MLPPrefabTool.ClearTempPrefabs();
    }
    [MenuItem("MakeLuaPanel/RefreshAsset")]
    static void RefreshAsset()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.Default | ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);
    }

}
