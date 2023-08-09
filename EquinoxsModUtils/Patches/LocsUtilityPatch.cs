using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace EquinoxsModUtils.Patches
{
    public class LocsUtilityPatch
    {
        [HarmonyPatch(typeof(LocsUtility), "TranslateStringFromHash")]
        [HarmonyPrefix]
        private static bool GetModdedTranslation(string hash, ref string __result, string original = null) {
            if (ModUtils.hashTranslations.ContainsKey(hash)) {
                __result = ModUtils.hashTranslations[hash];
                return false;
            }

            return true;
        }
    }
}
