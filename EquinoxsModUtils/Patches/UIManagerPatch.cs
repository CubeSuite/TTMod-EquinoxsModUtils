using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Patches
{
    internal class UIManagerPatch
    {
        public static bool freeCursor = false;

        [HarmonyPatch(typeof(UIManager), "get_anyMenuOpen")]
        [HarmonyPostfix]
        static void AnyMenuOpenPostfix(ref bool __result) {
            __result = UIManager.instance.hasOpenMenu || 
                       UIManager.instance.pauseMenuOpen || 
                       UIManager.instance.inventoryCraftingMenuOpen || 
                       UIManager.instance.craftingMenuOpen || 
                       UIManager.instance.assemblerMenuOpen || 
                       UIManager.instance.smelterMenuOpen || 
                       UIManager.instance.drillMenuOpen || 
                       UIManager.instance.researchLabMenuOpen || 
                       UIManager.instance.researchMenuOpen || 
                       UIManager.instance.editShortcutMenuOpen || 
                       UIManager.instance.sapTapMenuOpen || 
                       freeCursor;
        }
    }
}
