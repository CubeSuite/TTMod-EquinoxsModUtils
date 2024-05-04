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
        [HarmonyPatch(typeof(LocsUtility), nameof(LocsUtility.TranslateStringFromHash), new Type[] { typeof(string), typeof(string), typeof(UnityEngine.Object) })]
        [HarmonyPrefix]
        private static bool GetModdedTranslation(ref string __result, string hash) {
            if (string.IsNullOrEmpty(hash)) return true;

            if (ModUtils.hashTranslations.ContainsKey(hash)) {
                __result = ModUtils.hashTranslations[hash];
                return false;
            }

            return true;
        }
    }
}
