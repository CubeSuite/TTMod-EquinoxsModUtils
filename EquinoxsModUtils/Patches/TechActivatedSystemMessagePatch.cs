using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    internal class TechActivatedSystemMessagePatch
    {
        [HarmonyPatch(typeof(TechActivatedSystemMessage), "ToString")]
        [HarmonyPrefix]
        static void SetDefaultSprite(TechActivatedSystemMessage __instance) {
            if(!ModUtils.NullCheck(__instance.unlock.sprite, "Unlock Sprite")){
                ModUtils.NullCheck(UIManager.instance, "UIManager");
                ModUtils.NullCheck(UIManager.instance.techTreeMenu, "techTreeMenu");

                TechTreeNode node = UIManager.instance.techTreeMenu.GridUI.GetNodeByUnlock(GameDefines.instance.unlocks[0]);
                ModUtils.NullCheck(node, "TechTreeNode");

                Sprite defaultSprite = (Sprite)ModUtils.GetPrivateField("iconSprite", node);
                ModUtils.NullCheck(defaultSprite, "defaultSprite");

                __instance.unlock.sprite = defaultSprite;
                GameDefines.instance.unlocks[GameDefines.instance.unlocks.Count - 1] = __instance.unlock;
                ModUtils.LogEMUInfo($"Set Unlock '{__instance.unlock.displayName}' to default sprite");
            }
        }
    }
}
