using BepInExUtils.Commands;
using Lua;
using UnityEngine;

namespace BepInExUtils.LuaLoader.Behaviour;

public class LuaManager : MonoBehaviour
{
    private static readonly string LuaPath = Path.Combine(Application.dataPath, "..", "Lua");

    private readonly Dictionary<string, LuaSubCommand> _luaSubCommands = new();
    private readonly LuaState _state = LuaState.Create();

    private void Awake()
    {
        _state.OpenStandardLibrariesExtensions();
        if (!Directory.Exists(LuaPath))
            Directory.CreateDirectory(LuaPath);

        CommandManager.Instance.AddCommand("lua", "lua test", LuaCommand);
        DefaultSubCommand();
    }

    private void DefaultSubCommand()
    {
        _luaSubCommands.Add("run", async args =>
        {
            try
            {
                var luaCode = string.Join(" ", args);
                var results = await _state.DoStringAsync(luaCode);
                var luaValuesAll = results.Select(value => value.ToString()).ToList();
                await Utils.Logger.InfoAsync($"Lua result: {string.Join(" ", luaValuesAll)}");
            }
            catch (Exception e)
            {
                await Utils.Logger.ErrorAsync($"Lua error {e}\n{e.StackTrace}");
            }
        });
        _luaSubCommands.Add("help",
            async _ => await Utils.Logger.InfoAsync($"Lua sub commands:\n{string.Join("\n", _luaSubCommands.Keys)}"));
        _luaSubCommands.Add("test", async _ =>
        {
            var filePath = Path.Combine(LuaPath, "test.lua");
            if (!File.Exists(filePath)) return;
            try
            {
                await _state.DoFileAsync(filePath);
            }
            catch (Exception e)
            {
                await Utils.Logger.ErrorAsync($"Lua error {e}\n{e.StackTrace}");
            }
        });
    }

    private async Task LuaCommand(string[] args)
    {
        var subCommand = args.GetValueOrDefault(0);

        if (string.IsNullOrEmpty(subCommand))
        {
            await Utils.Logger.ErrorAsync("Lua sub command is null or empty");
            return;
        }

        if (!_luaSubCommands.TryGetValue(subCommand, out var luaSubCommand))
        {
            await Utils.Logger.ErrorAsync($"Lua sub command '{subCommand}' is not found");
            return;
        }

        await luaSubCommand(args.Skip(1).ToArray());
    }

    private delegate Task LuaSubCommand(string[] args);
}