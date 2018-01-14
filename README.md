# 功能介绍
假设你有一个C#脚本，并且在界面上绑定了一些对象，就像这样：

![img](https://github.com/ltccss/make_lua_panel_in_unity/raw/master/doc/how_to_use_1.png)


修改一下C#脚本中的类的继承关系（把MonoBehaviour改成提供的MakeLuaPanel）：


![img](https://github.com/ltccss/make_lua_panel_in_unity/raw/master/doc/how_to_use_2.png)

然后：


![img](https://github.com/ltccss/make_lua_panel_in_unity/raw/master/doc/how_to_use_3.png)

你将得到这么一个东西


![img](https://github.com/ltccss/make_lua_panel_in_unity/raw/master/doc/how_to_use_4.png)

emmm，这就是这玩意儿干的事情


# 特性
将c#面板类中的绑定关系转化为对应的lua脚本，
可以创建新的lua脚本，也可以更新已经存在的lua脚本上对应的方法
目前支持转化的c#类型有：
* GameObject
* Component以及各子类
* List<T>, 其中T为GameObject 或者 Component以及各子类
* Array ,比如GameObject[] 或者{Component以及各子类}[]

# 使用
* 主要的文件都在/unity_project/Assets/MakeLuaPanel目录下面，其中Example目录下为示例相关
* 你只需要把那些继承自MonoBehaviour的类改为继承自MakeLuaPanel，就可以在面板上右键"Make Lua Panel File"生成lua脚本了
* 提供MLPException和MLPAnnotation两个Attribute，你可以用来排除某些绑定关系或者将注释一并生成到lua脚本中，具体用法参见示例

# 工程相关
* 这个项目中的unity工程基于萌♂萌哥的 [topameng/CsToLua](https://github.com/topameng/CsToLua)
* 这个项目的作者使用的unity版本是5.6.4p4

# 脑补的工作流程&注意事项
因为很多时候用lua涉及到热更，而unity里面脚本是无法打包进bundle的，

所以当你使用这个工具制作一个新的界面时，一般流程肯定是先写一个C#的面板，用于绑定所有需要绑定的场景对象，

然后通过MakeLuaPanel生成对应的Lua Panel脚本，之后你可以引用这个LuaPanel，或者直接把这个脚本当控制器用

所以C#脚本避免不了，但是这个脚本其实最终是用不到的（因为所有绑定关系都在Lua里实现了）

有一个问题就是，当你把附着这些C#脚本的prefab打入bundle后，在一些旧环境下运行时，

因为旧环境中没有这些C#脚本，所以在prefab被加载时可能会有一堆“script missing”的警告

如果你家老板不介意这个，那么下面可以忽略

如果你家老板介意，而你又肛不过他的时候

那么我建议你在打bundle之前把所有附着了MakeLuaPanel子类的prefab和scene先临时备份下，

然后删除对应的Component,打完bundle再恢复这些prefab和scene

而不建议你在生成了lua脚本后直接删除继承了MakeLuaPanel的Component，

这样子做的话后续的修改你得重新一个个拖物体到C#里面，不方便
（其实是作者太懒还没写在找理由）
