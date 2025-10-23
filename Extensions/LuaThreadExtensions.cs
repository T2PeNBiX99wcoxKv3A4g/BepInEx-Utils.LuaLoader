using BepInExUtils.Attributes;
using JetBrains.Annotations;
using Lua;
using Lua.Runtime;

namespace BepInExUtils.LuaLoader.Extensions;

[AccessExtensions]
[AccessInstance<LuaThread>]
[AccessProperty<LuaStack>("Stack")]
[PublicAPI]
public static partial class LuaThreadExtensions
{
}