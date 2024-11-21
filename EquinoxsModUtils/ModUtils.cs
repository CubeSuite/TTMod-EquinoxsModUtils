using BepInEx;
using BepInEx.Logging;
using EquinoxsModUtils.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EquinoxsModUtils
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class ModUtils : BaseUnityPlugin
    {
        // Plugin Details
        private const string MyGUID = "com.equinox.EquinoxsModUtils";
        private const string PluginName = "EquinoxsModUtils";
        private const string VersionString = "6.1.3";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        internal static ManualLogSource Log = new ManualLogSource(PluginName);

        // Unity Functions

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            InternalTools.CheckBepInExConfig();

            Harmony.CreateAndPatchAll(typeof(FHG_UtilsPatch));
            Harmony.CreateAndPatchAll(typeof(FlowManagerPatch));
            Harmony.CreateAndPatchAll(typeof(SaveStatePatch));
            Harmony.CreateAndPatchAll(typeof(StringPatternsList));
            Harmony.CreateAndPatchAll(typeof(TechTreeGridPatch));
            Harmony.CreateAndPatchAll(typeof(UIManagerPatch));
            Harmony.CreateAndPatchAll(typeof(UnlockPatch));

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }

        private void Update() {
            EMU.LoadingStates.CheckLoadingStates();
        }
    }
}
