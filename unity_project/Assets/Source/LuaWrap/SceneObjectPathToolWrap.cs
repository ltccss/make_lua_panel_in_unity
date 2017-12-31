using System;
using UnityEngine;
using LuaInterface;

public class SceneObjectPathToolWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("GetGameObjectPath", GetGameObjectPath),
			new LuaMethod("GetObjectRelativePath", GetObjectRelativePath),
			new LuaMethod("FindEx", FindEx),
			new LuaMethod("New", _CreateSceneObjectPathTool),
			new LuaMethod("GetClassType", GetClassType),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "SceneObjectPathTool", typeof(SceneObjectPathTool), regs, fields, typeof(object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateSceneObjectPathTool(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			SceneObjectPathTool obj = new SceneObjectPathTool();
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: SceneObjectPathTool.New");
		}

		return 0;
	}

	static Type classType = typeof(SceneObjectPathTool);

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, classType);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetGameObjectPath(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 1, typeof(GameObject));
		string o = SceneObjectPathTool.GetGameObjectPath(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetObjectRelativePath(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 1, typeof(GameObject));
		GameObject arg1 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		bool arg2 = LuaScriptMgr.GetBoolean(L, 3);
		string o = SceneObjectPathTool.GetObjectRelativePath(arg0,arg1,arg2);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int FindEx(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Transform arg0 = (Transform)LuaScriptMgr.GetUnityObject(L, 1, typeof(Transform));
		string arg1 = LuaScriptMgr.GetLuaString(L, 2);
		Transform o = SceneObjectPathTool.FindEx(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

