using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Patches
{
    internal class TechTreeStatePatch
    {
        [HarmonyPatch(typeof(TechTreeState), "ReadSaveState")]
        [HarmonyPrefix]
        public static void AddStatesForCustomUnlocks() {
            ModUtils.AddStatesForCustomUnlocks();
        }
    }
}
