using System.Diagnostics.CodeAnalysis;
using BepInExUtils.Attributes;
using Lua;

namespace BepInExUtils.LuaLoader.Extensions;

[AccessExtensions]
[AccessInstance<LuaState>]
[AccessMethod("SetMetatable", typeof(LuaValue), typeof(LuaTable))]
public static partial class LuaStateExtensions
{
    extension(LuaState state)
    {
        public bool TryGetMetatable(LuaValue value,
            [NotNullWhen(true)] [SuppressMessage("ReSharper", "OutParameterValueIsAlwaysDiscarded.Global")]
            out LuaTable? result)
        {
            var parameters = new object?[]
            {
                value, null
            };
            var ret = state.MethodInvoke<bool>(nameof(TryGetMetatable), parameters);
            result = (LuaTable?)parameters[1];
            return ret;
        }
    }
}