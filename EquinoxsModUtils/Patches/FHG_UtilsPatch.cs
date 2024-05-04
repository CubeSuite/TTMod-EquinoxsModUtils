using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    internal class FHG_UtilsPatch
    {
        [HarmonyPatch(typeof(FHG_Utils), "Inline")]
        [HarmonyPostfix]
        static void ReplaceEmptySprite(ref string __result) {
            __result = __result.Replace("<sprite=\"\" index=0>", "");
        }
    }
}
