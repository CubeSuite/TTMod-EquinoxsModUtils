using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    internal class GameDefinesPatch
    {
        private static bool loadedCustomResources = false;

        [HarmonyPatch(typeof(GameDefines), "GetMaxResId")]
        [HarmonyPrefix]
        static void AddResources() {
            if (loadedCustomResources) return;
            loadedCustomResources = true;

            foreach (SchematicsSubHeader subheader in ModUtils.subHeadersToAdd) {
                subheader.uniqueId = ModUtils.GetNewSchematicsSubHeaderID();

                string combinedTitle = subheader.title;
                string[] parts = combinedTitle.Split('/');

                subheader.filterTag = ModUtils.GetSchematicsHeaderByTitle(parts[0]);
                subheader.title = parts[1];

                GameDefines.instance.schematicsSubHeaderEntries.Add(subheader);
            }

            foreach (NewResourceDetails details in ModUtils.resourcesToAdd) {
                ResourceInfo parentInfo = ModUtils.GetResourceInfoByNameUnsafe(details.parentName);
                
                ResourceInfo info = details.ConvertToResourceInfo();
                info.uniqueId = ModUtils.GetNewResourceID();

                if (string.IsNullOrEmpty(details.subHeaderTitle)) {
                    ModUtils.LogEMUError($"NewResourceDetails '{info.displayName}'.subHeaderTitle is nul or empty, aborting attempt to add.");
                    continue;
                }

                info.headerType = ModUtils.GetSchematicsSubHeaderByTitle(details.headerTitle, details.subHeaderTitle);

                if(info.model3D == null) {
                    ModUtils.LogEMUWarning($"NewResourceDetails '{info.displayName}'.model3D is null, using Parent's instead.");
                    info.model3D = parentInfo.model3D;
                }

                if(info.rawConveyorResourcePrefab == null) {
                    ModUtils.LogEMUWarning($"NewResourceDetails '{info.displayName}'.rawConveyorResourcePrefab is null, using Parent's instead.");
                    info.rawConveyorResourcePrefab = parentInfo.rawConveyorResourcePrefab;

                    if(details.rawConveyorResourcePrefab == null) {
                        ModUtils.LogEMUWarning($"NewResourceDetails '{info.displayName}'.rawConveyorResourcePrefab is null, using Parent's instead.");
                    }
                }
                
                if(info.headerType == null) {
                    ModUtils.LogEMUWarning($"NewResourceDetails '{info.displayName}'.headerType is null, using Parent's instead.");
                    info.headerType = parentInfo.headerType;
                }

                if (!string.IsNullOrEmpty(details.unlockName)) {
                    info.unlock = ModUtils.GetUnlockByNameUnsafe(details.unlockName);
                }

                if(info.unlock == null) {
                    ModUtils.LogEMUWarning($"NewResourceDetails '{info.displayName}'.unlock is null, using Parent's instead.");
                    info.unlock = parentInfo.unlock;
                }

                GameDefines.instance.resources.Add(info);
                ResourceNames.SafeResources.Add(info.displayName);
            }

            foreach(NewRecipeDetails details  in ModUtils.recipesToAdd) {
                SchematicsRecipeData recipe = details.ConvertToRecipe();
                recipe.uniqueId = ModUtils.GetNewRecipeID();

                if (!string.IsNullOrEmpty(details.unlockName)) {
                    recipe.unlock = ModUtils.GetUnlockByNameUnsafe(details.unlockName);
                }

                if (recipe.unlock == null) {
                    ModUtils.LogEMUWarning($"NewRecipeDetails.unlock is null, using outputs[0]'s instead.");
                    recipe.unlock = recipe.outputTypes[0].unlock;
                }

                GameDefines.instance.schematicsRecipeEntries.Add(recipe);
            }

            ModUtils.SetPrivateStaticField("_topResId", GameDefines.instance, -1);
        }
    }
}
