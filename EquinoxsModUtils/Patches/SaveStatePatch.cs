using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    public class SaveStatePatch
    {
        public static string dataFolder = $"{Application.persistentDataPath}/Equinox's Mod Utils";

        private static List<string> addedJsons = new List<string>();

        [HarmonyPatch(typeof(SaveState), "SaveToFile")]
        [HarmonyPostfix]
        static void saveMod(SaveState __instance, string saveLocation, bool saveToPersistent = true) {
            int count = ModUtils.unlockStatesToAdd.Count;
            ModUtils.LogEMUInfo($"Saving {count} custom Unlocks to file...");

            Directory.CreateDirectory(dataFolder);
            List<string> unlockStateJsons = new List<string>();
            foreach (TechTreeState.UnlockState state in ModUtils.unlockStatesToAdd) {
                unlockStateJsons.Add(JsonUtility.ToJson(state));
            }

            string saveFile = $"{dataFolder}/{__instance.metadata.worldName}.json";
            File.WriteAllLines(saveFile, unlockStateJsons);
            ModUtils.LogEMUInfo($"{count} Custom Unlocks Saved");
        }

        [HarmonyPatch(typeof(SaveState), "LoadFileData", typeof(SaveState.SaveMetadata), typeof(string))]
        [HarmonyPostfix]
        static void loadMod(SaveState __instance, SaveState.SaveMetadata saveMetadata, string replayLocation) {
            ModUtils.LogEMUInfo("Loading custom Unlocks from file...");

            string filePath = $"{dataFolder}/{saveMetadata.worldName}.json";
            if (File.Exists(filePath)) {
                string[] jsons = File.ReadAllLines(filePath);
                foreach (string json in jsons) {
                    if (string.IsNullOrEmpty(json)) continue;
                    if(addedJsons.Contains(json)) continue;
                    TechTreeState.UnlockState state = (TechTreeState.UnlockState)JsonUtility.FromJson(json, typeof(TechTreeState.UnlockState));

                    string name = $"Unlock #{state.unlockRef.uniqueId}";
                    string translatedName = LocsUtility.TranslateStringFromHash(state.unlockRef.displayNameHash);
                    if (!string.IsNullOrEmpty(state.unlockRef.displayName)) {
                        name = state.unlockRef.displayName;
                    }
                    else if (!string.IsNullOrEmpty(translatedName)) {
                        name = translatedName;
                    }

                    int existingIndex = -1;
                    if (isUnlockStateUnique(state, out existingIndex)) {
                        ModUtils.unlockStatesToAdd.Add(state);
                        addedJsons.Add(json);
                        //ModUtils.LogEMUInfo($"Loaded unique UnlockState from file for unlock '{name}'");
                    }
                    else {
                        ModUtils.unlockStatesToAdd[existingIndex] = state;
                        ModUtils.LogEMUInfo($"Overwrote UnlockState for Unlock '{name}' with state loaded from file");
                    }
                }

                ModUtils.LogEMUInfo($"Loaded {ModUtils.unlockStatesToAdd.Count} UnlockStates from file and registered them to be added to GameDefines");
            }
        }


        static bool isUnlockStateUnique(TechTreeState.UnlockState state, out int index) { 
            for(int i = 0; i < ModUtils.unlockStatesToAdd.Count; i++) {
                TechTreeState.UnlockState stateToAdd = ModUtils.unlockStatesToAdd[i];
                if (stateToAdd.unlockRef == null || state.unlockRef == null) continue;
                if (stateToAdd.unlockRef.displayNameHash == state.unlockRef.displayNameHash) {
                    index = i;
                    return false;
                }
            }

            index = -1;
            return true;
        }
    }
}