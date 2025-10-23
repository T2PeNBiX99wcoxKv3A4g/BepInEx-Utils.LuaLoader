using Lua;

namespace BepInExUtils.LuaLoader.Extensions;

public static class LuaTableExtensions
{
    extension(LuaTable table)
    {
        public LuaTable Copy(LuaTable? lookupTable = null)
        {
            var copy = new LuaTable();
            var tempKey = LuaValue.Nil;

            copy.Metatable = table.Metatable;

            while (table.TryGetNext(tempKey, out var valuePair))
            {
                tempKey = valuePair.Key;

                if (valuePair.Value.Type != LuaValueType.Table)
                    copy[valuePair.Key] = valuePair.Value;
                else
                {
                    var valueTable = valuePair.Value.Read<LuaTable>();
                    lookupTable ??= new();
                    lookupTable[table] = copy;
                    if (lookupTable.TryGetValue(valuePair.Value, out var value))
                        copy[valuePair.Key] = value;
                    else
                        copy[valuePair.Key] = valueTable.Copy(lookupTable);
                }
            }

            return copy;
        }
    }
}