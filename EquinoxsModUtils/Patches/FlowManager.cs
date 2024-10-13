using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Patches
{
    internal class FlowManagerPatch
    {
        [HarmonyPatch(typeof(FlowManager), nameof(FlowManager.LoadSaveGame))]
        [HarmonyPrefix]
        static void OnLoadingSecondSave() {
            if (!EMU.LoadingStates.hasGameLoaded) return;
            EMU.Events.FireGameUnloaded();
        }

        [HarmonyPatch(typeof(FlowManager), nameof(FlowManager.OnStartQuit))]
        [HarmonyPrefix]
        static void OnGameQuitting() {
            EMU.Events.FireGameUnloaded();
        }
    }
}
