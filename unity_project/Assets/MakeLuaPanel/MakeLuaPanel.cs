using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using System;
using System.Runtime.Serialization;

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
#endif

/// <summary>
/// 使用此类可将c#所写的面板上所绑定的场景对象转化为lua面板的形式,
/// 用法：
/// 在c#的对应的继承MonoBehaviour的类中，将该类由继承自MonoBehaviour改为继承此类,
/// 当在编辑器中绑定完场景对象时，右键脚本运行Make Lua Panel File，
/// 当指定的lua文件存在时，将检查是否存在对应方法{luaFunctionName}，如果方法存在，则更新该方法，否则在文件末尾添加该方法
/// Todo:当一些新的c#脚本随prefab被打包成bundle后，在旧的运行环境下读取此bundle可能会出现missing script的警告，
/// 如果在生成prefab的时候就删除此脚本，因为绑定关系也没了，不方便后续更新，所以希望是在生成bundle的时候去除此脚本
/// Using this script to convert the references of GameObject or Component which was combined in C# Class attached on some GameObject to a lua file
/// Usage:
/// Replace the base class "MonoBehaviour" with "MakeLuaPanel" in your C# Script
/// When you have combined all GameObject to the C# Class on inspector, right click the C# Class title on inspector, and then click "Make Lua Panel File"
/// If the target lua file exists, this script will check whether the certain lua function{luaFunctionName} exists, if this function exists,update it ,or it will be append to the end
/// </summary>
public class MakeLuaPanel : MonoBehaviour 
{
    /// <summary>
    /// 指定目标lua文件的类名，如果不填则默认为C#类名+"Panel"
    /// the target lua class name, default value is {C#ClassName} + "Panel" if empty
    /// </summary>
    [Header("Default:{C#ClassName}+\"Panel\"")]
    public string luaClassName;
    /// <summary>
    /// default value is "_FindComponent"
    /// NOT recommended to change this value and better to leave it there
    /// the true lua function that assign those objects which combined in C# to the certain lua object
    /// </summary>
    [Header("Default:_FindComponent")]
    public string luaCombineFunctionName;
    /// <summary>
    /// default value is "_ReleaseComponent"
    /// similar to luaCombineFunctionName, but it plays the role as releasing those objects instead of assigning them
    /// NOT recommended to change the value either
    /// </summary>
    [Header("Default:_ReleaseComponent")]
    public string luaReleaseFunctionName;
    /// <summary>
    /// default value is "self:" if empty
    /// sometimes you write a lua file begin with "MyLuaClass={};local this = MyLuaClass", 
    /// and then write the method content using variables like "this.ABC" instead of "self.ABC"
    /// in this case set "selfAlias" to "this" but not "self" for the generated content
    /// </summary>
    [Header("Default:self")]
    public string selfAlias = "";
    /// <summary>
    /// 指定目标lua文件，如果不填默认为Assets/LuaScripts/View/{luaClassName}.lua
    /// the target lua file that this script will generate, the default value is Assets/LuaScripts/View/{luaClassName}.lua if empty
    /// </summary>
    [Header("Default:Assets/LuaScripts/View/{luaClassName}.lua")]
    [MLPDivisionBelow()]
    public UnityEngine.Object targetLuaFile;
    #if UNITY_EDITOR
    static Encoding utf8WithoutBom = new System.Text.UTF8Encoding(false);

    enum FieldType
    {
        Unknown,
        GameObject,
        Component,
        List,
        Array,
    }

    [ContextMenu("Make Lua Panel File")]
    public void MakeLuaPanelFile()
    {

        string className = this.GetType().Name;
        string luaClassName = string.IsNullOrEmpty(this.luaClassName)? (className + "Panel") : this.luaClassName;
        string targetLuaFileName = luaClassName + ".lua";
        string luaCombineFunctionName = string.IsNullOrEmpty(this.luaCombineFunctionName)? "_FindComponent" : this.luaCombineFunctionName;
        string luaReleaseFunctionName = string.IsNullOrEmpty(this.luaReleaseFunctionName) ? "_ReleaseComponent" : this.luaReleaseFunctionName;
        string selfAlias = string.IsNullOrEmpty(this.selfAlias)? "self" : this.selfAlias;
        // todo : add option to determine if the created function is defined as a.Foo or a:Foo, do nothing with the existed function

        string outpath = Application.dataPath + "/LuaScripts/View" + "/" + targetLuaFileName;

        if (targetLuaFile != null)
        {
            var spath = AssetDatabase.GetAssetPath(targetLuaFile);
            Debug.LogWarning(">> spath " + spath);
            if (spath.Substring(spath.Length - 4, 4) != ".lua")
            {
                Debug.LogError("MakeLuaPanelFile : targetLuaFile is not lua file");
                return;
            }
            else
            {
                outpath = Application.dataPath + spath.Substring(6, spath.Length - 6);
            }
        }

        var memberList = new List<MemberInfo>();

        var members = this.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
        for (int i = 0; i < members.Length; i++)
        {
            if (members[i].MemberType == MemberTypes.Field && (members[i].DeclaringType != typeof(MakeLuaPanel)) && !Attribute.IsDefined(members[i], typeof(MLPException)))
            {
                memberList.Add(members[i]);
            }
        }

        var members2 = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        for (int i = 0; i < members2.Length; i++)
        {
            if (Attribute.IsDefined(members2[i], typeof(SerializeField)) && (members2[i].DeclaringType != typeof(MakeLuaPanel)) && !Attribute.IsDefined(members[i], typeof(MLPException)))
            {
                memberList.Add(members2[i]);
            }
        }

        Debug.Log("MakeLuaPanelFile : Write lua file to : " + outpath);

        string templatePath = string.Format(Application.dataPath + "/MakeLuaPanel/MakeLuaPanelTemplate.lua");
        
        if (!File.Exists(outpath))
        {
            //如果文件不存在，则使用模版文件创建一份，再替换对应类名关键字
            CreateLuaFileFromTemplate(luaClassName, outpath, templatePath);

        }


        //如果有对应方法，则更新对应方法，如果没有，则添加对应方法
        string fileText = null;
        using (StreamReader reader = new StreamReader(outpath))
        {
            fileText = reader.ReadToEnd();
        }
        if (fileText != null)
        {
            if (CheckIfHasLuaFunction(luaClassName, luaCombineFunctionName, fileText, "--make_lua_panel_tag1_do_not_delete"))
            {
                int startIndex = -1;
                int length = 0;

                GetLuaFunctionTextPosition(luaClassName, luaCombineFunctionName, fileText, "--make_lua_panel_tag1_do_not_delete", "--make_lua_panel_tag2_do_not_delete", out startIndex, out length);

                fileText = fileText.Substring(0, startIndex)
                    + GenerateFindComponentContext(memberList, this, luaClassName, luaCombineFunctionName, selfAlias, false) 
                    + (((startIndex + length) == fileText.Length)? "" : fileText.Substring(startIndex + length, fileText.Length - (startIndex + length)));

            }
            else
            {
                fileText += "\n" + "--this function is auto generated by script, and should NOT be modified manually.";
                fileText += "\n" + GenerateFindComponentContext(memberList, this, luaClassName, luaCombineFunctionName, selfAlias);
            }

            if (CheckIfHasLuaFunction(luaClassName, luaReleaseFunctionName, fileText, "--make_lua_panel_tag3_do_not_delete"))
            {
                int startIndex = -1;
                int length = 0;

                GetLuaFunctionTextPosition(luaClassName, luaReleaseFunctionName, fileText, "--make_lua_panel_tag3_do_not_delete", "--make_lua_panel_tag4_do_not_delete", out startIndex, out length);

                fileText = fileText.Substring(0, startIndex)
                    + GenerateReleaseComponentContext(memberList, this, luaClassName, luaReleaseFunctionName, selfAlias, false)
                    + (((startIndex + length) == fileText.Length) ? "" : fileText.Substring(startIndex + length, fileText.Length - (startIndex + length)));
            }
            else
            {
                fileText += "\n" + "--this function is auto generated by script, and should NOT be modified manually.";
                fileText += "\n" + GenerateReleaseComponentContext(memberList, this, luaClassName, luaReleaseFunctionName, selfAlias);
            }
        }


        using (StreamWriter writer = new StreamWriter(outpath, false, utf8WithoutBom))
        {
            writer.Write(fileText);                 
        }

        AssetDatabase.Refresh();

        if (this.targetLuaFile == null)
        {
            var targetPath = "Assets" + outpath.Replace(Application.dataPath, "");
            targetLuaFile = AssetDatabase.LoadAssetAtPath(targetPath, typeof(UnityEngine.Object));

        }

        Debug.Log("MakeLuaPanelFile : Done");

    }

    static private void CreateLuaFileFromTemplate(string luaClassName, string targetPath, string templatePath)
    {
        string fileText;
        using (StreamReader reader = new StreamReader(templatePath))
        {
            fileText = reader.ReadToEnd();
        }

        if (fileText == null)
        {
            Debug.LogError("cannot find template file form " + templatePath);
            return;
        }

        fileText = fileText.Replace("MakeLuaPanelTemplate", luaClassName);

        string targetDir = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        using (StreamWriter writer = new StreamWriter(targetPath, false, utf8WithoutBom))
        {
            writer.Write(fileText);
        }
    }

    static private string GenerateReleaseComponentContext(List<MemberInfo> memberInfoList, MakeLuaPanel panel, string luaClassName, string luaReleaseFunctionName, string selfAlias, bool needWrap = true)
    {
        string result = "";

        if (needWrap)
        {
            result += string.Format("function {0}:{1}()", luaClassName, luaReleaseFunctionName);
            result += "\n";
        }

        result += "--make_lua_panel_tag3_do_not_delete\n";

        for (int i = 0; i < memberInfoList.Count; i++)
        {
            var fieldType = GetFieldType(memberInfoList[i]);

            if (fieldType != FieldType.Unknown)
            {
                FieldInfo fd = memberInfoList[i] as FieldInfo;

                result += string.Format(MakeTab() + "{0}.{1} = nil;\n", selfAlias, fd.Name); ;
            }
        }

        result += "--make_lua_panel_tag4_do_not_delete";
        if (needWrap)
        {
            result += "\nend\n";
        }

        return result;
    }

    /// <summary>
    /// 生成FindComponent方法的文本内容，
    /// needWrap:是否包括函数头和尾
    /// </summary>
    static private string GenerateFindComponentContext(List<MemberInfo> memberInfoList, MakeLuaPanel panel, string luaClassName, string luaCombineFunctionName, string selfAlias, bool needWrap = true)
    {
        string result = "";

        if (needWrap)
        {
            result += string.Format("function {0}:{1}(pGameObject)", luaClassName, luaCombineFunctionName);
            result += "\n";
        }

        result += "--make_lua_panel_tag1_do_not_delete\n";

        result += MakeTab() + @"local tTransform = pGameObject.transform;";
        result += "\n";

        List<MemberInfo> unprocessedList = new List<MemberInfo>();

        for (int i = 0; i < memberInfoList.Count; i++)
        {
            //目前只处理GameObject、Component、Array(GameObject[]/Component[])、List<GameObject/Component>类型的字段
            //todo : 当发现对应的Component为MakeLuaPanel时，将对应的lua代码转化为对应的luaPanel，并在开头加入对应Require "xxx"
            var fieldType = GetFieldType(memberInfoList[i]);

            MLPAnnotation.AddTypeEnum addType = MLPAnnotation.AddTypeEnum.Above;
            string annotation = "";
            if (Attribute.IsDefined(memberInfoList[i], typeof(MLPAnnotation)))
            {
                var annotationAttribute = Attribute.GetCustomAttribute(memberInfoList[i], typeof(MLPAnnotation)) as MLPAnnotation;
                annotation = annotationAttribute.Annotion;
                addType = annotationAttribute.AnnotationAddType;
            }

            string code = "";

            if (fieldType == FieldType.GameObject)
            {
                FieldInfo fd = memberInfoList[i] as FieldInfo;
                var go = fd.GetValue(panel) as GameObject;

                if (go != null)
                {
                    code = string.Format(MakeTab() + "{2}.{0} = SceneObjectPathTool.FindEx(tTransform, \"{1}\").gameObject;\n", fd.Name, SceneObjectPathTool.GetObjectRelativePath(go, panel.gameObject), selfAlias);
                }
                else
                {
                    Debug.LogError(string.Format("MakeLuaPanel: field'{0}' is NULL", fd.Name));
                }


            }
            else if (fieldType == FieldType.Component)
            {
                FieldInfo fd = memberInfoList[i] as FieldInfo;
                var comp = fd.GetValue(panel) as Component;

                string fdType = StripNameSpace(fd.FieldType.ToString());

                if (comp != null)
                {
                    if (fdType == "Transform")
                    {
                        code = string.Format(MakeTab() + "{2}.{0} = SceneObjectPathTool.FindEx(tTransform, \"{1}\");\n", fd.Name, SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject), selfAlias);

                    }
                    else
                    {
                        code = string.Format(MakeTab() + "{3}.{0} = SceneObjectPathTool.FindEx(tTransform, \"{1}\").gameObject:GetComponent(\"{2}\");\n", fd.Name, SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject), fdType, selfAlias);

                    }
                }
                else
                {
                    Debug.LogError(string.Format("MakeLuaPanel: field'{0}' is NULL", fd.Name));
                }

            }
            else if (fieldType == FieldType.List)
            {
                FieldInfo fd = memberInfoList[i] as FieldInfo;
                var elementGenericTypeArray = fd.FieldType.GetGenericArguments();
                if (elementGenericTypeArray != null && elementGenericTypeArray.Length == 1)
                {
                    var elementGenericType = elementGenericTypeArray[0];

                    var elementFieldType = GetFieldType(elementGenericType);

                    var listObj = fd.GetValue(panel) as IList;

                    if (elementFieldType == FieldType.GameObject)
                    {
                        code += string.Format(MakeTab() + "{1}.{0} = {{", fd.Name, selfAlias);
                        if (listObj.Count > 0)
                        {
                            code += "\n";

                            for (int k = 0; k < listObj.Count; k++)
                            {
                                if (listObj[k] != null)
                                {
                                    code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\").gameObject,\n", SceneObjectPathTool.GetObjectRelativePath(listObj[k] as GameObject, panel.gameObject));
                                }
                                else
                                {
                                    Debug.LogError(string.Format("MakeLuaPanel: field'{0}'[{1}] is NULL", fd.Name, i));
                                }
                            }
                            code += MakeTab() + "};\n";
                        }
                        else
                        {
                            code += "};\n";
                        }
                    }
                    else if (elementFieldType == FieldType.Component)
                    {
                        code += string.Format(MakeTab() + "{1}.{0} = {{", fd.Name, selfAlias);
                        if (listObj.Count > 0)
                        {
                            code += "\n";
                            for (int k = 0; k < listObj.Count; k++)
                            {
                                if (listObj[k] != null)
                                {
                                    var comp = listObj[k] as Component;
                                    string fdType = StripNameSpace(comp.GetType().ToString());

                                    if (fdType == "Transform")
                                    {
                                        code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\"),\n", SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject));
                                    }
                                    else
                                    {
                                        code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\").gameObject:GetComponent(\"{1}\"),\n", SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject), fdType);
                                    }
                                }
                                else
                                {
                                    Debug.LogError(string.Format("MakeLuaPanel: field'{0}'[{1}] is NULL", fd.Name, k));
                                }
                            }
                            code += MakeTab() + "};\n";
                        }
                        else
                        {
                            code += "};\n";
                        }
                    }
                }
            }
            else if (fieldType == FieldType.Array)
            {
                FieldInfo fd = memberInfoList[i] as FieldInfo;
                var elementType = fd.FieldType.GetElementType();
                var elementFieldType = GetFieldType(elementType);
                var array = fd.GetValue(panel) as Array;

                if (elementFieldType == FieldType.GameObject)
                {
                    code += string.Format(MakeTab() + "{1}.{0} = {{", fd.Name, selfAlias);
                    if (array.Length > 0)
                    {
                        code += "\n";

                        for (int k = 0; k < array.Length; k++)
                        {
                            if (array.GetValue(k) != null)
                            {
                                code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\").gameObject,\n", SceneObjectPathTool.GetObjectRelativePath(array.GetValue(k) as GameObject, panel.gameObject));
                            }
                            else
                            {
                                Debug.LogError(string.Format("MakeLuaPanel: field'{0}'[{1}] is NULL", fd.Name, k));
                            }
                        }
                        code += MakeTab() + "};\n";
                    }
                    else
                    {
                        code += "};\n";
                    }
                }
                else if (elementFieldType == FieldType.Component)
                {
                    code += string.Format(MakeTab() + "{1}.{0} = {{", fd.Name, selfAlias);
                    if (array.Length > 0)
                    {
                        code += "\n";
                        for (int k = 0; k < array.Length; k++)
                        {
                            if (array.GetValue(k) != null)
                            {
                                var comp = array.GetValue(k) as Component;
                                string fdType = StripNameSpace(comp.GetType().ToString());

                                if (fdType == "Transform")
                                {
                                    code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\"),\n", SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject));
                                }
                                else
                                {
                                    code += string.Format(MakeTab(2) + "SceneObjectPathTool.FindEx(tTransform, \"{0}\").gameObject:GetComponent(\"{1}\"),\n", SceneObjectPathTool.GetObjectRelativePath(comp.gameObject, panel.gameObject), fdType);
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("MakeLuaPanel: field'{0}'[{1}] is NULL", fd.Name, k));
                            }
                        }
                        code += MakeTab() + "};\n";
                    }
                    else
                    {
                        code += "};\n";
                    }
                }
                                

            }
            else
            {
                unprocessedList.Add(memberInfoList[i]);
            }

            if (!string.IsNullOrEmpty(code))
            {
                result += WrapCodeWithAnnotation(code, annotation, addType);
            }
        }

        if (unprocessedList.Count > 0)
        {
            result += "\n" + MakeTab() + "--the following fields(public or SerializeField) were NOT processed, \n" 
                + MakeTab() + "--you can process them OUT of this function if necessary" + "\n";
        }

        for (int i = 0; i < unprocessedList.Count; i++)
        {
            result += string.Format(MakeTab(2) + "--{1}.{0}\n", unprocessedList[i].Name, selfAlias);
        }

        result += "--make_lua_panel_tag2_do_not_delete";
        if (needWrap)
        {
            result += "\nend\n";
        }
        return result;
    }
    static private FieldType GetFieldType(Type type)
    {
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return FieldType.List;
            }
            else
            {
                return FieldType.Unknown;
            }
        }
        else if (type.IsArray)
        {
            return FieldType.Array;
        }
        while (type != null)
        {
            if (type == typeof(GameObject))
            {
                return FieldType.GameObject;
            }
            else if (type == typeof(Component))
            {
                return FieldType.Component;
            }
            type = type.BaseType;
        }
        return FieldType.Unknown;
    }
    static private FieldType GetFieldType(MemberInfo memberInfo)
    {
        if (memberInfo is FieldInfo)
        {
            Type type = (memberInfo as FieldInfo).FieldType;

            return GetFieldType(type);
        }

        return FieldType.Unknown;
    }

    /// <summary>
    /// 使用空格符生成缩进
    /// </summary>
    static private string MakeTab(int tabCount = 1)
    {
        string result = "";
        for (int i = 0; i < tabCount; i++)
        {
            result += "    ";
        }
        return result;
    }

    static private bool CheckIfHasLuaFunction(string className, string luaFunctionName, string context, string tag)
    {
        //同时匹配ClassName:LuaFunctionName和Class.LuaFunctionName两种情况
        Regex reg = new Regex(string.Format(@"function\s+{0}\s*[:.]\s*{1}[\s\S]+{2}", className, luaFunctionName, tag));

        Match match = reg.Match(context);

        if (match.Success)
        {
            return true;
        }

        return false;
    }


    /// <summary>
    /// 获取lua方法在文本中的位置，不包含方法头和尾
    /// </summary>
    static private void GetLuaFunctionTextPosition(string className, string luaFunctionName, string context, string tag1, string tag2, out int startIndex, out int length)
    {
        startIndex = -1;
        length = 0;

        //Regex reg = new Regex(string.Format(@"function\s+{0}\s*[:.]\s*{1}[\s\S]+make_lua_panel_tag2_do_not_delete\s+end", className, luaFunctionName));
        //Regex reg = new Regex(string.Format(@"--make_lua_panel_tag1_do_not_delete[\s\S]+--make_lua_panel_tag2_do_not_delete", className, luaFunctionName));
        Regex reg = new Regex(string.Format(@"{2}[\s\S]+{3}", className, luaFunctionName, tag1, tag2));

        Match match = reg.Match(context);

        if (match.Success)
        {
            startIndex = match.Index;
            length = match.Length;
        }
    }

    /// <summary>
    /// 剔除类型字符串中的命名空间部分
    /// </summary>
    static private string StripNameSpace(string type)
    {
        int dotIndex = type.LastIndexOf('.');

        if (dotIndex > 0 && (dotIndex < type.Length - 1))
        {
            type = type.Substring(dotIndex + 1, type.Length - dotIndex - 1);
        }

        return type;
    }

    static private string WrapCodeWithAnnotation(string code, string annotation, MLPAnnotation.AddTypeEnum addType)
    {
        if (string.IsNullOrEmpty(annotation))
        {
            return code;
        }

        string result = "";

        if (annotation.Contains("\n"))
        {
            addType = MLPAnnotation.AddTypeEnum.Above;
        }

        if (addType == MLPAnnotation.AddTypeEnum.Above)
        {
            var arr = annotation.Split('\n');
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    result += string.Format(MakeTab() + "--{0}\n", arr[i]);
                }
                //result += string.Format(MakeTab() + "--{0}\n", annotation) + code;
            }
            result += code;
        }
        else
        {
            bool hasLineEnd = false;
            if (code.Length > 0 && code[code.Length - 1] == '\n')
            {
                code = code.Substring(0, code.Length - 1);
                hasLineEnd = true;
            }

            if (hasLineEnd)
            {
                result += code + string.Format("--{0}\n", annotation);
            }
            else
            {
                result += code + string.Format("--{0}", annotation);
            }
        }

        return result;
    }

#endif
}
