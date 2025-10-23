using BepInExUtils.LuaLoader.LuaObjects;
using JetBrains.Annotations;
using Lua;

namespace BepInExUtils.LuaLoader.LuaExtensions;

[PublicAPI]
public sealed class BasicLibraryExtensions
{
    public static readonly BasicLibraryExtensions Instance = new();
    public readonly LuaFunction[] Functions;

    public BasicLibraryExtensions() =>
        Functions =
        [
            new("class", Class)
        ];

    public ValueTask<int> Class(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var className = context.GetArgument<string>(0);
        var baseClass = context.GetArgumentOrDefault<LuaClass>(1);
        var luaClass = new LuaClass(className, baseClass);
        buffer.Span[0] = luaClass;
        return new(1);
    }
}