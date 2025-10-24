using BepInEx;
using BepInExUtils.Attributes;
using BepInExUtils.LuaLoader.Behaviour;
using UnityEngine;

namespace BepInExUtils.LuaLoader;

[BepInUtils("io.github.ykysnk.BepinExUtils.LuaLoader", "BepinEx Utils Lua Loader", Version)]
[BepInDependency("io.github.ykysnk.BepinExUtils", "1.0.0")]
public partial class Main
{
    private const string SectionOptions = "Options";
    private const string Version = "0.0.2";

    public void Init()
    {
        var obj = new GameObject(nameof(LuaManager));
        obj.AddComponent<LuaManager>();
        DontDestroyOnLoad(obj);
    }
}