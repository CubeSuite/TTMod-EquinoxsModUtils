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
    internal class SaveStatePatch
    {
        [HarmonyPatch(typeof(SaveState), "SaveToFile")]
        [HarmonyPostfix]
        static void saveMod(SaveState __instance, string saveLocation, bool saveToPersistent = true) {
            EMU.Events.FireGameSaved();
        }
    }
}