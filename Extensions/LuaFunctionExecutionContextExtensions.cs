using JetBrains.Annotations;
using Lua;

namespace BepInExUtils.LuaLoader.Extensions;

public static class LuaFunctionExecutionContextExtensions
{
    extension(LuaFunctionExecutionContext context)
    {
        [PublicAPI]
        public void ThrowIfArgumentTypeMismatch(int index, LuaValueType expectedType)
        {
            if (!context.HasArgument(index) || context.GetArgument(index).Type == expectedType) return;
            LuaRuntimeException.BadArgument(context.State.GetTraceback(), index + 1,
                context.Thread.GetCurrentFrame().Function.Name,
                expectedType.ToString(),
                context.GetArgument(index).Type.ToString());
        }

        [PublicAPI]
        public LuaValue GetArgumentOrNil(int index, LuaValueType expectedType)
        {
            var value = context.HasArgument(index) ? context.GetArgument(index) : LuaValue.Nil;
            context.ThrowIfArgumentTypeMismatch(index, expectedType);
            return value;
        }

        [PublicAPI]
        public T? GetArgumentOrDefault<T>(int index) =>
            context.HasArgument(index) ? context.GetArgument<T>(index) : default;

        public void ThrowIfArgumentNotChar(int index, string value)
        {
            if (value.Length < 2) return;
            LuaRuntimeException.BadArgument(context.State.GetTraceback(), index + 1,
                context.Thread.GetCurrentFrame().Function.Name, nameof(Char), nameof(LuaValueType.String));
        }
    }
}