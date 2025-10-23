using JetBrains.Annotations;
using Lua;
using UnityEngine;

namespace BepInExUtils.LuaLoader.LuaObjects;

[LuaObject]
[PublicAPI]
public partial class LuaVector3
{
    private Vector3 _vector;

    // Add LuaMember attribute to members that will be used in Lua
    // The argument specifies the name used in Lua (if omitted, the member name is used)
    [LuaMember("x")]
    public float X
    {
        get => _vector.x;
        set => _vector = _vector with
        {
            x = value
        };
    }

    [LuaMember("y")]
    public float Y
    {
        get => _vector.y;
        set => _vector = _vector with
        {
            y = value
        };
    }

    [LuaMember("z")]
    public float Z
    {
        get => _vector.z;
        set => _vector = _vector with
        {
            z = value
        };
    }

    // Static methods are treated as regular Lua functions
    [LuaMember("create")]
    public static LuaVector3 Create(float x, float y, float z) =>
        new()
        {
            _vector = new(x, y, z)
        };

    // Instance methods implicitly receive the instance (this) as the first argument
    // In Lua, this is accessed with instance:method() syntax
    [LuaMember("normalized")]
    public LuaVector3 Normalized() =>
        new()
        {
            _vector = Vector3.Normalize(_vector)
        };
}