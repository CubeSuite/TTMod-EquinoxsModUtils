using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    public class SaveStatePatch
    {
        [HarmonyPatch(typeof(SaveState), "SaveToFile")]
        [HarmonyPostfix]
        static void saveMod(SaveState __instance, string saveLocation, bool saveToPersistent = true) {
            ModUtils.SaveUnlockStates(__instance.metadata.worldName);
            ModUtils.SaveCustomMachineData(__instance.metadata.worldName);
            ModUtils.FireGameSavedEvent(__instance.metadata.worldName);
        }
    }
}