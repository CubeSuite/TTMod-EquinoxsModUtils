using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    internal class FlowManagerPatch
    {
        [HarmonyPatch(typeof(FlowManager), nameof(FlowManager.LoadSaveGame))]
        [HarmonyPrefix]
        static async void OnLoadingSecondSave() {
            if (!EMU.LoadingStates.hasGameLoaded) return;
            EMU.LoadingStates.ResetLoadingStates();

            await Task.Run(async () => {
                while (GameDefines.instance != null) {
                    await Task.Delay(1);
                    Debug.Log("Waiting for GameDefines to go null");
                }

                Debug.Log("GameDefines went null");
                EMU.LoadingStates.shouldMonitorLoadingStates = true;
            });
        
            EMU.Events.FireGameUnloaded();
        }

        [HarmonyPatch(typeof(FlowManager), nameof(FlowManager.OnStartQuit))]
        [HarmonyPrefix]
        static void OnGameQuitting() {
            EMU.Events.FireGameUnloaded();
        }
    }
}
