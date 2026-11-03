using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

namespace ZetymPatch;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

		harmony.PatchAll();
    }

    private void OnDestroy()
    {
	    harmony.UnpatchSelf();
    }
}
