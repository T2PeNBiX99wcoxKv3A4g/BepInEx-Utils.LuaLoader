using JetBrains.Annotations;
using Lua;

namespace BepInExUtils.LuaLoader.LuaExtensions;

[PublicAPI]
public sealed class StringLibraryExtensions
{
    public static readonly StringLibraryExtensions Instance = new();

    public readonly LuaFunction[] Functions;

    public StringLibraryExtensions() =>
        Functions =
        [
            new("split", Split),
            new("contains", Contains),
            new("startsWith", StartsWith),
            new("endsWith", EndsWith),
            new("insert", Insert),
            new("padLeft", PadLeft),
            new("padRight", PadRight),
            new("toTable", ToTable),
            new("remove", Remove),
            new("trim", Trim),
            new("trimStart", TrimStart),
            new("trimEnd", TrimEnd)
        ];

    public ValueTask<int> Split(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var separator = context.GetArgument<string>(1);
        var result = self.Split(separator);
        var retLuaTable = new LuaTable();

        for (var i = 0; i < result.Length; i++)
        {
            var tableKey = i + 1;
            retLuaTable[tableKey] = result[i];
        }

        buffer.Span[0] = retLuaTable;
        return new(1);
    }

    public ValueTask<int> Contains(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var value = context.GetArgument<string>(1);
        var result = self.Contains(value);

        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> StartsWith(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var value = context.GetArgument<string>(1);
        var result = self.StartsWith(value);

        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> EndsWith(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var value = context.GetArgument<string>(1);
        var result = self.EndsWith(value);

        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> Insert(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var startIndex = context.GetArgument<int>(1);
        var value = context.GetArgument<string>(2);
        var result = self.Insert(startIndex, value);

        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> PadLeft(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var totalWidth = context.GetArgument<int>(1);
        var paddingChar = ' ';

        if (context.HasArgument(2))
        {
            var value = context.GetArgument<string>(2);
            context.ThrowIfArgumentNotChar(2, value);
            paddingChar = value[0];
        }

        var result = self.PadLeft(totalWidth, paddingChar);
        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> PadRight(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var totalWidth = context.GetArgument<int>(1);
        var paddingChar = ' ';

        if (context.HasArgument(2))
        {
            var value = context.GetArgument<string>(2);
            context.ThrowIfArgumentNotChar(2, value);
            paddingChar = value[0];
        }

        var result = self.PadRight(totalWidth, paddingChar);
        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> ToTable(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var result = self.ToCharArray();
        var table = new LuaTable();

        for (var i = 0; i < result.Length; i++)
        {
            var tableKey = i + 1;
            table[tableKey] = result[i].ToString();
        }

        buffer.Span[0] = table;
        return new(1);
    }

    public ValueTask<int> Remove(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var startIndex = context.GetArgument<int>(1);

        if (context.HasArgument(2))
        {
            var count = context.GetArgument<int>(2);
            var result = self.Remove(startIndex, count);
            buffer.Span[0] = result;
            return new(1);
        }

        var result2 = self.Remove(startIndex);
        buffer.Span[0] = result2;
        return new(1);
    }

    public ValueTask<int> Trim(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var chars = new List<char>();

        for (var i = 2; i <= context.ArgumentCount; i++)
        {
            var charString = context.GetArgument<string>(i);
            context.ThrowIfArgumentNotChar(i, charString);
            chars.Add(charString[0]);
        }

        var result = self.Trim(chars.ToArray());
        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> TrimStart(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var chars = new List<char>();

        for (var i = 2; i <= context.ArgumentCount; i++)
        {
            var charString = context.GetArgument<string>(i);
            context.ThrowIfArgumentNotChar(i, charString);
            chars.Add(charString[0]);
        }

        var result = self.TrimStart(chars.ToArray());
        buffer.Span[0] = result;
        return new(1);
    }

    public ValueTask<int> TrimEnd(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<string>(0);
        var chars = new List<char>();

        for (var i = 2; i <= context.ArgumentCount; i++)
        {
            var charString = context.GetArgument<string>(i);
            context.ThrowIfArgumentNotChar(i, charString);
            chars.Add(charString[0]);
        }

        var result = self.TrimEnd(chars.ToArray());
        buffer.Span[0] = result;
        return new(1);
    }
}