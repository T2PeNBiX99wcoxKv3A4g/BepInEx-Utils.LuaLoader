using JetBrains.Annotations;
using Lua;
using Lua.Internal;
using Lua.Runtime;
using Lua.Standard;

namespace BepInExUtils.LuaLoader.LuaExtensions;

[PublicAPI]
public static class OpenLibsExtensions
{
    public static void OpenStringLibraryExtensions(this LuaState state)
    {
        var @string = new LuaTable(0, StringLibrary.Instance.Functions.Length);
        foreach (var function in StringLibrary.Instance.Functions)
            @string[function.Name] = function;
        foreach (var function in StringLibraryExtensions.Instance.Functions)
            @string[function.Name] = function;

        state.Environment["string"] = @string;
        state.LoadedModules["string"] = @string;
        var luaValue = new LuaValue("");
        if (!state.TryGetMetatable(luaValue, out var result))
        {
            result = new();
            state.SetMetatable(luaValue, result);
        }

        result[Metamethods.Index] = new LuaFunction("index", async (context, buffer, cancellationToken) =>
        {
            var self = context.GetArgument<string>(0);
            var key = context.GetArgument(1);
            context.State.Push(key);
            var toNumberBuffer = new PooledArray<LuaValue>(1024);

            await BasicLibrary.Instance.ToNumber(context with
            {
                ArgumentCount = 1,
                FrameBase = context.Thread.Stack.Count - 1
            }, toNumberBuffer.AsMemory(), cancellationToken);

            if (toNumberBuffer[0].Type == LuaValueType.Number)
            {
                var subBuffer = new PooledArray<LuaValue>(1024);
                context.State.Push(self);
                context.State.Push(toNumberBuffer[0]);
                context.State.Push(toNumberBuffer[0]);

                await StringLibrary.Instance.Sub(context with
                {
                    ArgumentCount = 3,
                    FrameBase = context.Thread.Stack.Count - 3
                }, subBuffer.AsMemory(), cancellationToken);

                buffer.Span[0] = subBuffer[0];
                subBuffer.Dispose();
            }
            else
                buffer.Span[0] = @string[key];

            toNumberBuffer.Dispose();
            return 1;
        });

        result[Metamethods.Add] = new LuaFunction("add", async (context, buffer, cancellationToken) =>
        {
            var self = context.GetArgument<string>(0);
            var value = context.GetArgument(1);

            context.State.Push(value);
            var toStringBuffer = new PooledArray<LuaValue>(1024);

            await BasicLibrary.Instance.ToString(context with
            {
                ArgumentCount = 1,
                FrameBase = context.Thread.Stack.Count - 1
            }, toStringBuffer.AsMemory(), cancellationToken);

            buffer.Span[0] = self + toStringBuffer[0].Read<string>();
            toStringBuffer.Dispose();
            return 1;
        });
    }

    extension(LuaState state)
    {
        [PublicAPI]
        public void OpenBasicLibraryExtensions()
        {
            state.OpenBasicLibrary();
            foreach (var function in BasicLibraryExtensions.Instance.Functions)
                state.Environment[function.Name] = function;
        }

        [PublicAPI]
        public void OpenStandardLibrariesExtensions()
        {
            state.OpenBasicLibraryExtensions();
            state.OpenBitwiseLibrary();
            state.OpenCoroutineLibrary();
            state.OpenIOLibrary();
            state.OpenMathLibrary();
            state.OpenModuleLibrary();
            state.OpenOperatingSystemLibrary();
            state.OpenStringLibraryExtensions();
            state.OpenTableLibrary();
        }
    }
}