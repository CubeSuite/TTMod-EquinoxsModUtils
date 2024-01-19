using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EquinoxsModUtils.Patches
{
    public class SaveStatePatch
    {
        public static string dataFolder = $"{Application.persistentDataPath}/Equinox's Mod Utils";

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
                    TechTreeState.UnlockState state = (TechTreeState.UnlockState)JsonUtility.FromJson(json, typeof(TechTreeState.UnlockState));
                    ModUtils.unlockStatesToAdd.Add(state);
                }

                ModUtils.LogEMUInfo($"Loaded {ModUtils.unlockStatesToAdd.Count} Custom Unlocks");
            }
        }
    }
}