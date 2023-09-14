using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace EquinoxsModUtils.Patches
{
    public class TechTreeStatePatch
    {
        [HarmonyPatch(typeof(TechTreeState), "ReadSaveState")]
        [HarmonyPrefix]
        private static void LoadNewUnlockStates() {
            ModUtils.LogEMUInfo($"Adding {ModUtils.unlocksToAdd.Count} new UnlockStates");
            foreach (TechTreeState.UnlockState state in ModUtils.unlockStatesToAdd) {
                TechTreeState.instance.unlockStates.AddItem(state);
                ModUtils.LogEMUInfo($"Added UnlockState for tech '{state.unlockRef.displayName}'");
            }
        }
    }
}
