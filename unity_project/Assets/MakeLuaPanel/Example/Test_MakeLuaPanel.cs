using UnityEngine;
using System.Collections;
using LuaInterface;
using System.IO;

public class Test_MakeLuaPanel : MonoBehaviour
{
    public GameObject myContent1;
    private LuaScriptMgr luaMgr = null;
    // Use this for initialization
    void Start()
    {
        string fileText = null;
        using (StreamReader reader = new StreamReader(Application.dataPath + "/LuaScripts/View/MyContent1Panel.lua"))
        {
            fileText = reader.ReadToEnd();
        }



        luaMgr = new LuaScriptMgr();
        luaMgr.Start();
        luaMgr.DoString(fileText);

        SceneObjectPathToolWrap.Register(luaMgr.lua.L);
        UnityEngine_UI_ImageWrap.Register(luaMgr.lua.L);
        UnityEngine_UI_TextWrap.Register(luaMgr.lua.L);
        RectTransformWrap.Register(luaMgr.lua.L);

        luaMgr.CallLuaFunction("MyContent1Panel.Test", this.myContent1);
    }
    void Update()
    {
        if (luaMgr != null)
        {
            luaMgr.Update();
        }
    }
    void LateUpdate()
    {
        if (luaMgr != null)
        {
            luaMgr.LateUpate();
        }
    }

    void FixedUpdate()
    {
        if (luaMgr != null)
        {
            luaMgr.FixedUpdate();
        }
    }

}
