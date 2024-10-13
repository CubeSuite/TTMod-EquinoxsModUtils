using HarmonyLib;
using RewiredConsts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils.Patches
{
    internal class TechTreeGridPatch
    {
        [HarmonyPatch(typeof(TechTreeGrid), "InitForCategoryHelper")]
        [HarmonyPrefix]
        static bool FixInitForCategoryHelper(TechTreeGrid __instance, Unlock.TechCategory category, bool isPrimary) {
            TechTreeCategoryContainer[] containers = (TechTreeCategoryContainer[])EMU.GetPrivateField("categoryContainers", __instance);
            List<int> categoryMapping = containers[(int)category].categoryMapping;
            RectTransform nodesXfm = containers[(int)category].nodesXfm;

            Vector2Int[] tierIndices = (Vector2Int[])EMU.GetPrivateField("tierIndices", __instance);

            int num = -99;
            if (isPrimary) {
                for (int i = 0; i < tierIndices.Length; i++) {
                    tierIndices[i] = new Vector2Int(-1, -1);
                }
            }
            
            Vector2Int vector2Int = new Vector2Int(-1, -1);
            bool flag = false;
            bool flag2 = false;

            Dictionary<int, float> _tierScrollPositions = (Dictionary<int, float>)EMU.GetPrivateField("_tierScrollPositions", __instance);
            TechTreeNode[] techTreeNodes = (TechTreeNode[])EMU.GetPrivateField("techTreeNodes", __instance);

            _tierScrollPositions[0] = 0f;
            for (int j = 0; j < categoryMapping.Count; j++) {
                int num2 = categoryMapping[j];
                if (techTreeNodes[num2] == null) {
                    techTreeNodes[num2] = UnityEngine.Object.Instantiate<TechTreeNode>(__instance.elementPrefab, nodesXfm);
                    techTreeNodes[num2].Init(__instance, num2);
                    flag2 = true;
                }
                if (isPrimary) {
                    if (techTreeNodes[num2].myUnlock.requiredTier == TechTreeState.ResearchTier.NONE) {
                        LogEMUWarning($"Setting Unlock #{techTreeNodes[num2].myUnlock.uniqueId} requiredTier to Tier0");
                        techTreeNodes[num2].myUnlock.requiredTier = TechTreeState.ResearchTier.Tier0;
                    }
                    int num3 = techTreeNodes[num2].myUnlock.requiredTier.ToIndex();
                    if (num3 != num) {
                        _tierScrollPositions[num3] = techTreeNodes[num2].xfm.anchoredPosition.y - 35f;
                        if (flag) {
                            vector2Int.y = j - 1;
                            tierIndices[num] = vector2Int;
                        }
                        num = num3;
                        vector2Int.x = j;
                        flag = true;
                    }
                }
                techTreeNodes[num2].RefreshState(new TechTreeGrid.CommonLocStrings());
            }
            if (num >= 0 && num < tierIndices.Length) {
                vector2Int.y = categoryMapping.Count - 1;
                tierIndices[num] = vector2Int;
            }
            if (flag2) {
                containers[(int)category].InitDependencyLines(techTreeNodes, categoryMapping);
            }

            EMU.SetPrivateField("tierIndices", __instance, tierIndices);
            EMU.SetPrivateField("_tierScrollPositions", __instance, _tierScrollPositions);
            EMU.SetPrivateField("techTreeNodes", __instance, techTreeNodes);

            return false;
        }
    }
}
