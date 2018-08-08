using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJson;

public class MLPPrefabTool
{
    static string tempFolderName = "TempPrefab";
    static string jsonFileName = "PrefabList.json";
    public static void BackupAndRemoveMLPComponents()
    {
        //1.将对应的prefab拷贝到临时文件夹,
        //注意：需要连着相对于Assets/的文件目录结构一起拷贝，方便手动还原
        //2.记录拷贝的prefab文件的路径，存放于一份json文件中
        //3.删除工程内prefab上的MLP脚本

        string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/";
        string tempFolderPath = projectPath + tempFolderName + "/";
        Debug.LogWarning(tempFolderPath);

        string jsonFilePath = tempFolderPath + "/" + jsonFileName;

        if (File.Exists(jsonFilePath))
        {
            //RestoreMLPComponents成功后会把json文件清除，
            //如果当前存在json文件，说明临时目录里的prefab尚未被restore
            //这个时候再次BackupAndRemoveMLPComponents可能会出问题
            //因此给予警告提示
            var result = EditorUtility.DisplayDialog("警告", "上一次存放的临时Prefabs可能尚未被还原，再次备份可能覆盖掉之前备份的Prefabs", "继续", "取消");
            if (!result)
            {
                return;
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        var paths = AssetDatabase.GetAllAssetPaths();

        var json = new JsonObject();
        json["time"] = DateTime.Now.ToString();
        var jsonArray = new JsonArray();
        json["prefabs"] = jsonArray;

        if (paths != null && paths.Length > 0)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (go != null)
                {
                    var arr = go.GetComponentsInChildren<MakeLuaPanel>(true);
                    if (arr != null && arr.Length > 0)
                    {
                        var prefabJson = new JsonObject();
                        prefabJson["name"] = go.name;
                        prefabJson["path"] = paths[i];
                        jsonArray.Add(prefabJson);

                        CopyPrefab(projectPath + paths[i], tempFolderPath + paths[i]);
                    }
                }
            }

        }

        AssetDatabase.Refresh();

        if (paths != null && paths.Length > 0)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (go != null)
                {
                    var arr = go.GetComponentsInChildren<MakeLuaPanel>(true);
                    if (arr != null && arr.Length > 0)
                    {

                        for (int j = 0; j < arr.Length; j++)
                        {
                            UnityEngine.Object.DestroyImmediate(arr[j], true);
                        }
                        EditorUtility.SetDirty(go);
                    }
                }
            }

        }

        AssetDatabase.SaveAssets();


        File.WriteAllText(jsonFilePath, json.ToString());

        Debug.LogWarning("BackupAndRemoveMLPComponents : done");
    }

    static void CopyPrefab(string fromPath, string toPath)
    {
        //check if directory exists
        var toDirectory = Path.GetDirectoryName(toPath);
        if (!Directory.Exists(toDirectory))
        {
            Directory.CreateDirectory(toDirectory);
        }

        Debug.LogWarning(string.Format(">> copy from {0} to {1}", fromPath, toPath));
       
        if (File.Exists(toPath))
        {
            File.Delete(toPath);
        }
        File.Copy(fromPath, toPath);

        var metaFromPath = fromPath.Substring(0, fromPath.Length - 7) + ".prefab.meta";
        var metaToPath = toPath.Substring(0, toPath.Length - 7) + ".prefab.meta";

        if (File.Exists(metaToPath))
        {
            File.Delete(metaToPath);
        }
        File.Copy(metaFromPath, metaToPath);
    }

    static void RemovePrefab(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }


    public static void RestoreMLPComponents()
    {
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/";
        string tempFolderPath = projectPath + tempFolderName + "/";
        Debug.LogWarning(tempFolderPath);

        string jsonFilePath = tempFolderPath + "/" + jsonFileName;

        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError(">>RestoreMLPComponents : No jsonfile found");
            return;
        }
        var jsonText = File.ReadAllText(jsonFilePath);
        var json = SimpleJson.SimpleJson.DeserializeObject(jsonText) as JsonObject;
        var jsonArray = json["prefabs"] as JsonArray;

        for (int i = 0; i < jsonArray.Count; i++)
        {
            var prefabJson = jsonArray[i] as JsonObject;
            var prefabPath = Convert.ToString(prefabJson["path"]);
            RemovePrefab(projectPath + prefabPath);
        }

        AssetDatabase.Refresh();

        for (int i = 0; i < jsonArray.Count; i++)
        {
            var prefabJson = jsonArray[i] as JsonObject;
            var prefabPath = Convert.ToString(prefabJson["path"]);
            CopyPrefab(tempFolderPath + prefabPath, projectPath + prefabPath);
            //var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            //EditorUtility.SetDirty(go);
            //AssetDatabase.ImportAsset(prefabPath);
        }

        AssetDatabase.Refresh();

        File.Delete(jsonFilePath);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        Debug.LogWarning("RestoreMLPComponents : done");
    }

    public static void ClearTempPrefabs()
    {
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/";
        string tempFolderPath = projectPath + tempFolderName + "/";

        Directory.Delete(tempFolderPath);

        Debug.LogWarning("ClearTempPrefabs : done");
    }
}
