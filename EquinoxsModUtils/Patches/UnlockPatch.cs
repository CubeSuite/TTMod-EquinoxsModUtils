using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Patches
{
    internal class UnlockPatch
    {
        [HarmonyPatch(typeof(Unlock), "GetActivationEnergy")]
        [HarmonyPrefix]
        static bool getEnergyFix(Unlock __instance, ref int __result) {
            int num = 0;
            ModUtils.NullCheck(__instance, "Unlock __instance");
            ModUtils.NullCheck(__result, "__result");
            ModUtils.NullCheck(GameDefines.instance, "GameDefines.instance");
            List<int> coreActivationEnergyCosts = GameDefines.instance.coreActivationEnergyCosts;
            ModUtils.NullCheck(coreActivationEnergyCosts, "coreActivationEnergyCosts");
            for (int i = 0; i < __instance.coresNeeded.Count; i++) {
                ModUtils.NullCheck(__instance.coresNeeded, "coresNeeded");
                ModUtils.NullCheck(__instance.coresNeeded[i], "coresNeeded[i]");
                ModUtils.NullCheck(__instance.coresNeeded[i].type, "coresNeeded[i].type");
                ModUtils.NullCheck(__instance.coresNeeded[i].number, "coresNeeded[i].number");
                int type = (int)__instance.coresNeeded[i].type;
                if (type < 0) type = 0;
                int num2 = __instance.coresNeeded[i].number * coreActivationEnergyCosts[Math.Min(type, coreActivationEnergyCosts.Count - 1)];
                num += num2;
            }
            __result = num;
            return false;
        }
    }
}
