# 功能介绍

![img](https://github.com/ltccss/make_lua_panel_in_unity/raw/master/doc/usage.gif)


# 特性
将附着于GameObject上的C#脚本中的成员和场景物体的绑定关系转化为对应的lua脚本，
可以创建新的lua脚本，也可以<b>更新</b>已经存在的lua脚本上对应的方法
目前支持转化的c#类型有：
* GameObject
* Component以及各子类
* List&lt;T&gt;, 其中T为GameObject 或者 Component以及各子类
* Array ,比如GameObject[] 或者Component[] (包括Component的子类)

# 使用
* 主要的文件都在/unity_project/Assets/MakeLuaPanel目录下面，其中Example目录下为示例相关
* 只需要把那些继承自MonoBehaviour的类改为继承自MakeLuaPanel，就可以在面板上右键"Make Lua Panel File"生成lua脚本了
* 提供MLPException和MLPAnnotation两个Attribute，可以用来排除某些绑定关系或者将注释一并生成到lua脚本中，具体用法参见示例

# 工程相关
* 这个项目中的unity工程基于萌♂萌哥的 [topameng/CsToLua](https://github.com/topameng/CsToLua)
* 这个项目当前使用的unity版本是5.6.4p4

# 注意事项
因为很多时候用lua涉及到热更，而通常来说unity里面C#脚本是无法打包进bundle里进行热更的，

当你使用这个工具制作一个新的界面时，一般流程是先写一个C#的脚本，在C#脚本里声明你需要的场景里的对象，然后把脚本附加到某GameObject上，然后拖拖拖，把需要的东西全拖到脚本对应的面板里（其实就是正常的纯C#开发步骤）

然后通过MakeLuaPanel生成对应的Lua Panel脚本，之后你可以引用这个LuaPanel，或者直接把其他逻辑写这个脚本里

所以C#脚本避免不了，但是这个脚本其实在最终运行时是用不到的（因为所有绑定关系都在生成的Lua代码里实现了）

有一个问题就是，当你把附着这些C#脚本的prefab打入bundle后，在一些旧环境下运行时，

因为旧环境中没有这些C#脚本，所以在prefab被加载时可能会有一堆“script missing”的警告

如果你家老板不介意这个，那么下面可以忽略

<s>如果你家老板介意，而你又肛不过他的时候</s>

推荐的做法是为出（热更）包的工程单独开一个末端分支（也就是该分支不再合至其他分支），然后可以自己写一些简单的工具去遍历、删除所有不需要的东西，
等出完包后使用版本工具直接回退节点

目前这个项目也自带 [删除所有Prefab上附着的MakeLuaPanel脚本并备份相关的Prefab] 以及 [使用备份恢复那些被移除MakeLuaPanel脚本的Prefab]的功能（在顶部MakeLuaPanel菜单中），但是这些尚未经过长时间大规模使用验证，我也不造会有什么问题XD

不建议的做法是，在生成了lua脚本后直接删除继承了MakeLuaPanel的Component，
因为C#的脚本除了用来生成lua绑定代码外，还有个作用是，在unity里，它是可以维持住和它所绑定的场景物体的关系的，
就比如，如果你修改了一个场景中的GameObject的名字，如果它被某C#的脚本绑定，脚本面板中可以观察到此物体的更新，而不用重新去拖一遍

如果生成了lua脚本后直接删除掉继承了MakeLuaPanel的Component脚本话，因为没有C#脚本了，后续的界面修改每次都得重新添加C#脚本，然后一个个拖物体到C#里面，不方便
