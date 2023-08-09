using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EquinoxsModUtils.Patches;
using HarmonyLib;
using TriangleNet;
using UnityEngine;

namespace EquinoxsModUtils
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class ModUtils : BaseUnityPlugin
    {
        // Plugin Details
        private const string MyGUID = "com.equinox.EquinoxsModUtils";
        private const string PluginName = "EquinoxsModUtils";
        private const string VersionString = "1.0.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // Objects & Variables
        public static bool shouldLog = true;
        public static bool hasGameStateLoaded = false;
        public static bool hasGameDefinesLoaded = false;
        public static bool hasSaveStateLoaded = false;
        public static bool hasTechTreeStateLoaded = false;

        private static List<Unlock> unlocksToAdd = new List<Unlock>();
        private static Dictionary<string, List<string>> unlockDependencies = new Dictionary<string, List<string>>();

        internal static Dictionary<string, string> hashTranslations = new Dictionary<string, string>();
        internal static List<TechTreeState.UnlockState> unlockStatesToAdd = new List<TechTreeState.UnlockState>();

        // Events
        public static event EventHandler GameStateLoaded;
        public static event EventHandler GameDefinesLoaded;
        public static event EventHandler SaveStateLoaded;
        public static event EventHandler TechTreeStateLoaded;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            CheckBepInExConfig();

            Harmony.CreateAndPatchAll(typeof(LocsUtilityPatch));
            Harmony.CreateAndPatchAll(typeof(SaveStatePatch));

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }

        private void Update() {
            if (!hasGameStateLoaded) CheckIfGameStateLoaded();
            if (!hasGameDefinesLoaded) CheckIfGameDefinesLoaded();
            if (!hasSaveStateLoaded) CheckIfSaveStateLoaded();
            if (!hasTechTreeStateLoaded) CheckIfTechTreeStateLoaded();
        }

        private static void CheckBepInExConfig() {
            string configFile = $"{Environment.CurrentDirectory}/BepInEx/config/BepInEx.cfg";
            LogEMUInfo($"Config file path: '{configFile}'");
            if (File.Exists(configFile)) {
                string text = File.ReadAllText(configFile);
                if(!text.Contains("HideManagerGameObject = true")) {
                    LogEMUError("HideGameManagerObject has not been set to true in BepInEx.cfg");
                    LogEMUError("Equinox's Mod Utils and any mods that use this library will not work if this is not enabled");
                }
                else {
                    LogEMUInfo("HideGameManagerObject has been correctly set to true");
                }
            }
        }
        
        internal static void LogEMUInfo(string message) {
            if (shouldLog) {
                Debug.Log($"[EMU]: {message}");
            }
        }

        internal static void LogEMUWarning(string message) {
            if (shouldLog) {
                Debug.LogWarning($"[EMU]: {message}");
            }
        }

        internal static void LogEMUError(string message) {
            Debug.LogError($"[EMU]: {message}");
        }

        // Event Checking

        private static void CheckIfGameStateLoaded() {
            if (GameLogic.instance == null) return;
            if (GameState.instance != null) {
                hasGameStateLoaded = true;
                GameStateLoaded?.Invoke(GameState.instance, EventArgs.Empty);
                LogEMUInfo("GameState.intance loaded.");
            }
        }

        private static void CheckIfGameDefinesLoaded() {
            if (GameLogic.instance == null) return;
            if (GameDefines.instance != null) {
                hasGameDefinesLoaded = true;
                LogEMUInfo($"Adding {unlocksToAdd.Count} new Unlocks");
                for(int i = 0; i < unlocksToAdd.Count; i++) {
                    Unlock unlock = unlocksToAdd[i];
                    if(FindDependencies(ref unlock)) {
                        unlock.uniqueId = GetNewUnlockUniqueID();
                        GameDefines.instance.unlocks.Add(unlock);
                        LogEMUInfo($"Added new Unlock: '{unlock.displayName}'");
                    }
                }

                GameDefinesLoaded?.Invoke(GameDefines.instance, EventArgs.Empty);
                LogEMUInfo("GameDefines.instace loaded");
            }
        }

        private static void CheckIfSaveStateLoaded() {
            if (GameLogic.instance == null) return;
            if (SaveState.instance != null) {
                hasSaveStateLoaded = true;
                SaveStateLoaded?.Invoke(SaveState.instance, EventArgs.Empty);
                LogEMUInfo("SaveState.instance loaded");
            }
        }

        private static void CheckIfTechTreeStateLoaded() {
            if (MachineManager.instance == null) return;
            if (TechTreeState.instance != null) {
                hasTechTreeStateLoaded = true;
                LogEMUInfo($"Adding {unlocksToAdd.Count} new UnlockStates");
                foreach (TechTreeState.UnlockState state in unlockStatesToAdd) {
                    TechTreeState.instance.unlockStates.AddItem(state);
                    LogEMUInfo($"Added UnlockState for tech '{state.unlockRef.displayName}'");
                }

                TechTreeStateLoaded?.Invoke(TechTreeState.instance, EventArgs.Empty);
                LogEMUInfo("TechTreeState.instance loaded");
            }
        }

        // Add New Techs

        private static int GetNewUnlockUniqueID() {
            if (!hasGameDefinesLoaded) {
                LogEMUInfo("Couldn't get new unlock unique ID, GameDefines not loaded yet.");
                LogEMUInfo("Check that you're calling this from an OnGameDefinesLoaded event");
                return -1;
            }

            int highest = -1;
            foreach (Unlock unlock in GameDefines.instance.unlocks) {
                if (unlock.uniqueId > highest) {
                    highest = unlock.uniqueId;
                }
            }

            LogEMUInfo($"Found ID {highest + 1} for new unlock");
            return highest + 1;
        }

        private static bool FindDependencies(ref Unlock unlock) {
            List<Unlock> dependencies = new List<Unlock>();
            List<string> dependencyNames = unlockDependencies[unlock.displayNameHash];
            foreach (string unlockName in dependencyNames) {
                Unlock dependency = GetUnlockByName(unlockName);
                if (dependency != null) {
                    dependencies.Add(dependency);
                }
                else {
                    LogEMUError($"Could not find dependency with name '{unlockName}'. Abandoning attempt to add.");
                    LogEMUError("Try using a name from EquinoxsModUtils.ResourceNames");
                    return false;
                }
            }

            unlock.dependencies = dependencies;
            if (dependencies.Count >= 1) unlock.dependency1 = dependencies[0];
            if (dependencies.Count == 2) unlock.dependency2 = dependencies[1];
            return true;
        }

        // Public Functions For Other Devs

        #region Resources

        /// <summary>
        /// Finds the ResourceInfo that matches the name given in the argument.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired resource</param>
        /// <returns>ResourceInfo if successful, null if not.</returns>
        public static ResourceInfo GetResourceInfoByName(string name) {
            LogEMUInfo($"Looking for resource with name '{name}'");
            if (hasGameDefinesLoaded) {
                foreach (ResourceInfo info in GameDefines.instance.resources) {
                    if (info.displayName == name) {
                        LogEMUInfo($"Found resource with name '{name}'");
                        return info;
                    }
                }
            }
            else {
                LogEMUWarning("GetResourceInfoByName() was called before GameDefines.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }

            LogEMUWarning($"Could not find resource with name '{name}'");
            LogEMUWarning($"Try using a name from EquinoxsModUtils.ResourceNames");
            return null;
        }

        /// <summary>
        /// Finds the resource ID of the Resource with name given in the argument.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired Resource</param>
        /// <returns>resID if successful, -1 otherwise</returns>
        public static int GetResourceIDByName(string name) {
            LogEMUInfo($"Looking for ID of resource with name '{name}'");
            if (hasSaveStateLoaded) {
                ResourceInfo info = GetResourceInfoByName(name);
                if (info != null) {
                    return SaveState.GetIdForResInfo(info);
                }
                else {
                    return -1;
                }
            }
            else {
                LogEMUWarning("GetResourceIDByName() was called before SaveState.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.SaveState loaded or checking with ModUtils.hasSaveStateLoaded");
                return -1;
            }
        }

        #endregion

        #region Schematics

        /// <summary>
        /// Finds a thresher recipe by the names of it's two outputs. Order does not matter.
        /// Does not work for recipes with one output.
        /// </summary>
        /// <param name="output1Name">The displayName of one of the outputs</param>
        /// <param name="output2Name">The displayName of the other output</param>
        /// <returns>Thresher recipe if successful, null if not</returns>
        public static SchematicsRecipeData FindThresherRecipeFromOutputs(string output1Name, string output2Name) {
            LogEMUInfo($"Looking for thresher recipe with outputs: '{output1Name}', '{output2Name}'");
            if (!hasGameDefinesLoaded) {
                LogEMUWarning("FindThresherRecipeFromOutputs() called before GameDefines.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
                return null;
            }

            foreach (SchematicsRecipeData schematic in GameDefines.instance.schematicsRecipeEntries) {
                if (schematic.outputTypes.Count() == 2) {
                    if ((schematic.outputTypes[0].displayName == output1Name && schematic.outputTypes[1].displayName == output2Name) ||
                        (schematic.outputTypes[0].displayName == output2Name && schematic.outputTypes[1].displayName == output1Name)) {
                        LogEMUInfo("Found thresher recipe");
                        return schematic;
                    }
                }
            }

            LogEMUWarning("Could not find thresher recipe");
            LogEMUWarning("Try using the resource names in EquinoxsModUtils.ResourceNames");
            return null;
        }

        #endregion

        #region Tech Tree

        /// <summary>
        /// Finds the Unlock that matches the name given in the argument.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired Unlock</param>
        /// <returns>Unlock if successful, null if not</returns>
        public static Unlock GetUnlockByName(string name) {
            LogEMUInfo($"Looking for Unlock with name '{name}'");
            if (hasGameDefinesLoaded) {
                foreach (Unlock tech in GameDefines.instance.unlocks) {
                    if (tech.displayNameHash == LocsUtility.GetHashString(name)) {
                        LogEMUInfo("Found Unlock");
                        return tech;
                    }
                }
            }
            else {
                LogEMUWarning("GetUnlockByName() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }

            LogEMUWarning($"Couldn't find Unlock with name '{name}'");
            LogEMUWarning("Try using a name from EquinoxsModUtils.UnlockNames");
            return null;
        }

        /// <summary>
        /// Registers a new Unlock to be added to the TechTree once GameDefines and TechTreeState have loaded.
        /// </summary>
        /// <param name="details">Details of the new Unlock. Ensure that all are provided.</param>
        public static void addNewUnlock(NewUnlockDetails details) {
            if (details.dependencyNames.Count > 2) {
                LogEMUError($"New Tech '{details.displayName}' cannot have more than 2 dependencies. Abandoning attempt to add.");
                return;
            }

            if (details.coreTypeNeeded == ResearchCoreDefinition.CoreType.Blue) {
                details.coreTypeNeeded = ResearchCoreDefinition.CoreType.Green; // Temp Bug Fix
            }

            // Basic Details
            Unlock newUnlock = ScriptableObject.CreateInstance<Unlock>();
            newUnlock.category = details.category;
            newUnlock.coresNeeded = new List<Unlock.RequiredCores>() {
                new Unlock.RequiredCores() {
                    type = details.coreTypeNeeded,
                    number = details.coreCountNeeded
                }
            };
            newUnlock.isCoreTech = false;
            newUnlock.isDebugTech = false;
            newUnlock.numScansNeeded = details.numScansNeeded;
            newUnlock.requiredTier = details.requiredTier;
            newUnlock.priority = 0;
            newUnlock.scanDuration = 1;
            newUnlock.sprite = details.sprite;
            newUnlock.treePosition = details.treePosition;
            
            // Hashed Details
            string displayNameHash = LocsUtility.GetHashString(details.displayName);
            string descriptionHash = LocsUtility.GetHashString(details.description);
            hashTranslations.Add(displayNameHash, details.displayName);
            hashTranslations.Add(descriptionHash, details.description);
            newUnlock.displayNameHash = displayNameHash;
            newUnlock.descriptionHash = descriptionHash;

            unlockDependencies.Add(displayNameHash, details.dependencyNames);
            unlocksToAdd.Add(newUnlock);

            // Unlock State
            unlockStatesToAdd.Add(new TechTreeState.UnlockState() {
                isActive = false,
                everActive = false,
                isDiscovered = true,
                scansCompleted = 0,
                scansRequired = 0,
                dependencies = new int[0],
                requirementFor = new int[0],
                tier = details.requiredTier,
                exists = true,
                unlockRef = newUnlock,
                unlockedResources = new List<ResourceInfo>(),
                unlockedUpgrades = new List<UpgradeInfo>()
            });

            LogEMUInfo($"Successfully created new Unlock '{details.displayName}'");
        }

        /// <summary>
        /// Used to change the sprite of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="unlockID">The uniqueId of the Unlock to update. This is returned by addNewUnlock()</param>
        /// <param name="sprite">The new sprite to use</param>
        public static void UpdateUnlockSprite(int unlockID, Sprite sprite) {
            LogEMUInfo($"Trying to update sprite of Unlock with ID '{unlockID}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GameDefines.instance.unlocks[unlockID];
                    unlock.sprite = sprite;
                    LogEMUInfo($"Updated sprite of Unlock with ID '{unlockID}'");
                }
                catch (Exception e) {
                    LogEMUError($"Error occurred while trying to update sprite of Unlock with ID '{unlockID}'");
                    LogEMUError(e.Message);
                    LogEMUError(e.StackTrace);
                }
            }
            else {
                LogEMUWarning("UpdateUnlockSprite() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the sprite of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="displayName">The displayName of the Unlock to update</param>
        /// <param name="sprite">The new sprite to use</param>
        public static void UpdateUnlockSprite(string displayName, Sprite sprite) {
            LogEMUInfo($"Trying to update sprite of Unlock '{displayName}'");
            if (hasGameDefinesLoaded) {
                Unlock unlock = GetUnlockByName(displayName);
                unlock.sprite = sprite;
                LogEMUInfo($"Updated sprite of Unlock '{displayName}'");
            }
            else {
                LogEMUWarning("UpdateUnlockSprite() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the treePosition of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="unlockID">The uniqueId of the Unlock to update. This is returned by addNewUnlock()</param>
        /// <param name="treePosition">The new treePosition value to use</param>
        public static void UpdateUnlockTreePosition(int unlockID, int treePosition) {
            LogEMUInfo($"Trying to update treePosition of Unlock with ID '{unlockID}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GameDefines.instance.unlocks[unlockID];
                    unlock.treePosition = treePosition;
                    LogEMUInfo($"Updated treePosition of Unlock with ID '{unlockID}'");
                }
                catch (Exception e) {
                    LogEMUError($"Error occurred while trying to update treePosition of Unlock with ID '{unlockID}'");
                    LogEMUError(e.Message);
                    LogEMUError(e.StackTrace);
                }
            }
            else {
                LogEMUWarning("UpdateUnlocktreePosition() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the treePosition of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="displayName">The displayName of the Unlock to update</param>
        /// <param name="treePosition">The new treePosition value to use</param>
        public static void UpdateUnlockTreePosition(string displayName, int treePosition) {
            LogEMUInfo($"Trying to update treePosition of Unlock '{displayName}'");
            if (hasGameDefinesLoaded) {
                Unlock unlock = GetUnlockByName(displayName);
                unlock.treePosition = treePosition;
                LogEMUInfo($"Updated treePosition of Unlock '{displayName}'");
            }
            else {
                LogEMUWarning("UpdateUnlockTreePosition() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        #endregion
    }
}
