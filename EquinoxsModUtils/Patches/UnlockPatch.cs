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
            List<int> coreActivationEnergyCosts = GameDefines.instance.coreActivationEnergyCosts;
            for (int i = 0; i < __instance.coresNeeded.Count; i++) {
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
