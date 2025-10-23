using System.Diagnostics.CodeAnalysis;
using Lua;
using Lua.Runtime;

namespace BepInExUtils.LuaLoader.LuaObjects;

public class LuaClass : ILuaUserData
{
    private static LuaTable? _metatable;
    private readonly LuaClass? _baseClass;
    private readonly string _className;
    private readonly LuaTable _instanceTable;
    private readonly bool _isInstance;
    private readonly LuaTable _table;
    private bool _locked;

    public LuaClass(string className, LuaClass? baseClass = null)
    {
        _className = className;
        _baseClass = baseClass;
        _table = new()
        {
            [Metamethods.ToString] = new LuaFunction("toString", __toStringInstance)
        };
        _instanceTable = new();
    }

    private LuaClass(string className, LuaClass? baseClass, LuaTable classTable)
    {
        _className = className;
        _baseClass = baseClass;
        _table = classTable.Copy();
        _instanceTable = new();
        _isInstance = true;
    }

    public LuaTable? Metatable
    {
        get
        {
            _metatable ??= new()
            {
                [Metamethods.Index] = new LuaFunction("index", Index),
                [Metamethods.NewIndex] = new LuaFunction("newIndex", NewIndex),
                [Metamethods.Call] = new LuaFunction("call", Call),
                [Metamethods.ToString] = new LuaFunction("toString", __toString)
            };

            if (!_isInstance) return _metatable;
            var metaTable = _table.Copy();

            metaTable[Metamethods.Index] = new LuaFunction("index", IndexInstance);
            metaTable[Metamethods.NewIndex] = new LuaFunction("newIndex", NewIndexInstance);

            return metaTable;
        }
        set { }
    }

    private async ValueTask<LuaValue[]> CallInitFunction(LuaState state, LuaValue[] arguments,
        CancellationToken cancellationToken)
    {
        if (!_isInstance || !_table.TryGetValue("initialize", out var initFunctionValue) &&
            !_table.TryGetValue("init", out initFunctionValue)) return [];
        var initFunction = initFunctionValue.Read<LuaFunction>();
        return await initFunction.InvokeAsync(state, arguments, cancellationToken);
    }

    private static ValueTask<int> Lock(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        self._locked = true;
        buffer.Span[0] = self;
        return new(1);
    }

    private static async ValueTask<int> Call(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        var instanceClass = new LuaClass(self._className, self._baseClass, self._table);
        var arguments = new LuaValue[context.ArgumentCount];

        arguments[0] = instanceClass;

        for (var i = 1; i < context.ArgumentCount; i++)
            arguments[i] = context.GetArgument(i);

        await instanceClass.CallInitFunction(context.State, arguments, cancellationToken);
        buffer.Span[0] = instanceClass;
        return 1;
    }

    private static ValueTask<int> __toString(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        buffer.Span[0] = $"Class<{self._className}>";
        return new(1);
    }

    private static ValueTask<int> __toStringInstance(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        buffer.Span[0] = $"{self._className}<>";
        return new(1);
    }

    private static ValueTask<int> Index(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        var key = context.GetArgument(1);
        if (key.Type == LuaValueType.String)
        {
            var keyString = key.Read<string>();
            var result = keyString switch
            {
                "lock" => !self._locked ? new LuaFunction("lock", Lock) : LuaValue.Nil,
                "name" => self._className,
                "new" => new LuaFunction("new", Call),
                _ => LuaValue.Nil
            };

            if (result != LuaValue.Nil)
            {
                buffer.Span[0] = result;
                return new(1);
            }
        }

        buffer.Span[0] = self._table[key];
        return new(1);
    }

    private static async ValueTask<int> IndexInstance(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        var key = context.GetArgument(1);

        if (key.Type == LuaValueType.String)
        {
            var keyString = key.Read<string>();
            var result = keyString switch
            {
                "name" => self._className,
                _ => LuaValue.Nil
            };

            if (result != LuaValue.Nil)
            {
                buffer.Span[0] = result;
                return 1;
            }
        }

        if (self._table.TryGetValue(Metamethods.Index, out var indexFunctionValue))
        {
            var indexFunction = indexFunctionValue.Read<LuaFunction>();
            var returns = await indexFunction.InvokeAsync(context.State, [self, key, self._instanceTable],
                cancellationToken);

            buffer.Span[0] = returns[0];
            return 1;
        }

        if (self._instanceTable.TryGetValue(key, out var value))
        {
            buffer.Span[0] = value;
            return 1;
        }

        buffer.Span[0] = self._table[key];
        return 1;
    }

    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
    private static ValueTask<int> NewIndex(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        var key = context.GetArgument(1);
        var value = context.GetArgument(2);

        if (key.Type == LuaValueType.String)
        {
            var keyString = key.Read<string>();

            switch (keyString)
            {
                case "name":
                    throw new LuaRuntimeException(context.State.GetTraceback(), $"'{keyString}' cannot overwrite.");
            }
        }

        if (self._locked)
            throw new LuaRuntimeException(context.State.GetTraceback(), $"Class<{self._className}> is locked");

        self._table[key] = value;
        return new(0);
    }

    private static async ValueTask<int> NewIndexInstance(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken cancellationToken)
    {
        var self = context.GetArgument<LuaClass>(0);
        var key = context.GetArgument(1);
        var value = context.GetArgument(2);

        if (key.Type == LuaValueType.String)
        {
            var keyString = key.Read<string>();

            switch (keyString)
            {
                case "name":
                case "new":
                    throw new LuaRuntimeException(context.State.GetTraceback(), $"'{keyString}' cannot overwrite.");
            }
        }

        if (self._table.TryGetValue(Metamethods.NewIndex, out var newIndexFunctionValue))
        {
            var newIndexFunction = newIndexFunctionValue.Read<LuaFunction>();
            await newIndexFunction.InvokeAsync(context.State, [self, key, value, self._instanceTable],
                cancellationToken);
            return 0;
        }

        self._instanceTable[key] = value;
        return 0;
    }

    public static implicit operator LuaValue(LuaClass value) => new(value);
}