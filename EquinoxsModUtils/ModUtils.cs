using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EquinoxsModUtils.Patches;
using FluffyUnderware.DevTools.Extensions;
using HarmonyLib;
using MessagePack.Formatters;
using TriangleNet;
using UnityEngine;
using Voxeland5;

namespace EquinoxsModUtils
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class ModUtils : BaseUnityPlugin
    {
        // Plugin Details
        private const string MyGUID = "com.equinox.EquinoxsModUtils";
        private const string PluginName = "EquinoxsModUtils";
        private const string VersionString = "3.3.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // Objects & Variables
        public static bool hasGameStateLoaded = false;
        public static bool hasGameDefinesLoaded = false;
        public static bool hasMachineManagerLoaded = false;
        public static bool hasSaveStateLoaded = false;
        public static bool hasTechTreeStateLoaded = false;
        public static bool hasGameLoaded = false;
        private static bool loadingUIObserved = false;

        private static List<Unlock> unlocksToAdd = new List<Unlock>();
        private static Dictionary<string, List<string>> unlockDependencies = new Dictionary<string, List<string>>();

        private static Dictionary<string, int> resourceNameToIDMap = new Dictionary<string, int>();
        private static Dictionary<string, int> unlockNameToIDMap = new Dictionary<string, int>();
        private static Dictionary<int, Unlock> unlockCache = new Dictionary<int, Unlock>();

        internal static Dictionary<string, string> hashTranslations = new Dictionary<string, string>();
        internal static List<TechTreeState.UnlockState> unlockStatesToAdd = new List<TechTreeState.UnlockState>();
        internal static List<int> customUnlockIDs = new List<int>();

        // Events
        public static event EventHandler GameStateLoaded;
        public static event EventHandler GameDefinesLoaded;
        public static event EventHandler MachineManagerLoaded;
        public static event EventHandler SaveStateLoaded;
        public static event EventHandler TechTreeStateLoaded;
        public static event EventHandler GameLoaded;

        // Testing
        // ToDo: Disable before release

        private static bool doUnlockTest = false;
        private static bool printResources = false;
        private static bool printUnlocks = false;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            CheckBepInExConfig();

            Harmony.CreateAndPatchAll(typeof(LocsUtilityPatch));
            Harmony.CreateAndPatchAll(typeof(TechTreeStatePatch));
            Harmony.CreateAndPatchAll(typeof(SaveStatePatch));
            Harmony.CreateAndPatchAll(typeof(TechActivatedSystemMessagePatch));
            Harmony.CreateAndPatchAll(typeof(SteamLobbyConnectorPatch));
            Harmony.CreateAndPatchAll(typeof(UnlockPatch));
            Harmony.CreateAndPatchAll(typeof(TechTreeGridPatch));

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;

            if (doUnlockTest) {
                AddNewUnlock(new NewUnlockDetails() {
                    category = Unlock.TechCategory.Synthesis,
                    coreTypeNeeded = ResearchCoreDefinition.CoreType.Red,
                    coreCountNeeded = 1,
                    description = "Test Unlock Description",
                    displayName = "Test Unlock",
                    numScansNeeded = 0,
                    requiredTier = TechTreeState.ResearchTier.Tier1,
                    treePosition = 50
                });
            }
        }

        private void Update() {
            if (!hasGameStateLoaded) CheckIfGameStateLoaded();
            if (!hasGameDefinesLoaded) CheckIfGameDefinesLoaded();
            if (!hasMachineManagerLoaded) CheckIfMachineManagerLoaded();
            if (!hasSaveStateLoaded) CheckIfSaveStateLoaded();
            if (!hasTechTreeStateLoaded) CheckIfTechTreeStateLoaded();
            if (!hasGameLoaded) CheckIfGameLoaded();
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
            Debug.Log($"[EMU]: {message}");
        }

        internal static void LogEMUWarning(string message) {
            Debug.LogWarning($"[EMU]: {message}");
        }

        internal static void LogEMUError(string message) {
            Debug.LogError($"[EMU]: {message}");
        }

        // Name Printing For Updates

        private static void PrintAllResourceNames() {
            Debug.Log("Printing all resources");
            foreach(ResourceInfo info in GameDefines.instance.resources) {
                Debug.Log(info.displayName);
            }
        }

        private static void PrintAllUnlockNames() {
            Debug.Log("Printing all unlocks");
            foreach(Unlock unlock in GameDefines.instance.unlocks) {
                Debug.Log(LocsUtility.TranslateStringFromHash(unlock.displayNameHash));
            }
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
                AddCustomUnlocks();
                GameDefinesLoaded?.Invoke(GameDefines.instance, EventArgs.Empty);
                LogEMUInfo("GameDefines.instace loaded");

                foreach(Unlock unlock in GameDefines.instance.unlocks) {
                    string name = LocsUtility.TranslateStringFromHash(unlock.displayNameHash);
                    if(unlock.requiredTier == TechTreeState.ResearchTier.NONE) {
                        LogEMUWarning($"Unlock '{name}' has required Tier NONE, setting to Tier0");
                        unlock.requiredTier = TechTreeState.ResearchTier.Tier0;
                    }
                }

                if (printResources) PrintAllResourceNames();
                if (printUnlocks) PrintAllUnlockNames();
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

                TechTreeStateLoaded?.Invoke(TechTreeState.instance, EventArgs.Empty);
                LogEMUInfo("TechTreeState.instance loaded");
            }
        }

        private static void CheckIfMachineManagerLoaded() {
            if(MachineManager.instance == null) return;
            hasMachineManagerLoaded = true;
            MachineManagerLoaded?.Invoke(MachineManager.instance, EventArgs.Empty);
            LogEMUInfo("MachineManager.instance loaded");
        }

        private static void CheckIfGameLoaded() {
            if(LoadingUI.instance == null && !loadingUIObserved) {
                return;
            }

            else if (LoadingUI.instance != null && !loadingUIObserved) {
                loadingUIObserved = true; 
                return;
            }

            else if(LoadingUI.instance == null && loadingUIObserved) {
                hasGameLoaded = true;
                GameLoaded?.Invoke(null, EventArgs.Empty);
                LogEMUInfo("Game Loaded");
            }
        }

        // Add New Techs

        private static void AddCustomUnlocks() {
            LogEMUInfo($"Adding {unlocksToAdd.Count} new Unlocks");
            for (int i = 0; i < unlocksToAdd.Count; i++) {
                Unlock unlock = unlocksToAdd[i];
                if(!NullCheck(unlock, "Unlock")) continue;

                if (FindDependencies(ref unlock)) {
                    unlock.uniqueId = GetNewUnlockUniqueID();
                    customUnlockIDs.Add(unlock.uniqueId);
                    GameDefines.instance.unlocks.Add(unlock);
                    LogEMUInfo($"Added new Unlock: '{unlock.uniqueId}'");

                    // Unlock State
                    unlockStatesToAdd.Add(new TechTreeState.UnlockState() {
                        isActive = false,
                        everActive = true,
                        isDiscovered = true,
                        scansCompleted = 0,
                        scansRequired = 0,
                        dependencies = new int[0],
                        requirementFor = new int[0],
                        tier = unlock.requiredTier,
                        exists = true,
                        unlockRef = unlock,
                        unlockedRecipes = new List<SchematicsRecipeData>(),
                        unlockedResources = new List<ResourceInfo>(),
                        unlockedUpgrades = new List<UpgradeInfo>()
                    });
                }
            }
        }

        private static void CleanUnlockStates() {
            LogEMUInfo("Cleaning Unlock States");
            for (int i = 0; i < TechTreeState.instance.unlockStates.Count();) {
                TechTreeState.UnlockState state = TechTreeState.instance.unlockStates[i];
                if (state.unlockRef == null || GameDefines.instance.unlocks.Contains(state.unlockRef)) {
                    i++;
                    continue;
                }

                TechTreeState.instance.unlockStates.RemoveAt(i);
                GameState.instance.acknowledgedUnlocks.Remove(state.unlockRef.uniqueId);
                LogEMUInfo($"Could not find Unlock for UnlockState #{i}. Removed.");
            }

            LogEMUInfo($"Clearing duplicate unlock states");
            List<TechTreeState.UnlockState> uniqueStates = new List<TechTreeState.UnlockState>();
            for(int i = 0; i < TechTreeState.instance.unlockStates.Count(); i++) {
                bool isUnique = true;
                foreach(TechTreeState.UnlockState state in uniqueStates) {
                    if (TechTreeState.instance.unlockStates[i].unlockRef == null || state.unlockRef == null) continue;
                    if (TechTreeState.instance.unlockStates[i].unlockRef.uniqueId == state.unlockRef.uniqueId) {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique) uniqueStates.Add(TechTreeState.instance.unlockStates[i]);
            }

            int numDuplicates = TechTreeState.instance.unlockStates.Count() - uniqueStates.Count;
            LogEMUInfo($"Found '{uniqueStates.Count}' unique states");
            LogEMUInfo($"Removing {numDuplicates} duplicates");
            TechTreeState.instance.unlockStates = uniqueStates.ToArray();
        }

        private static void CleanTechProgress() {
            LogEMUInfo("Cleaning Tech Progress");
            for (int i = 0; i < SaveState.instance.techTree.researchProgress.Count;) {
                SaveState.TechTreeSaveInfo.TechProgress progress = SaveState.instance.techTree.researchProgress[i];
                if(progress.techIndex >= TechTreeState.instance.unlockStates.Length) {
                    SaveState.instance.techTree.researchProgress.RemoveAt(i);
                    LogEMUInfo($"Could not find UnlockState for TechProgress #{progress.techIndex}. Removed.");
                }
                else {
                    ++i;
                }
            }
        }

        internal static void AddStatesForCustomUnlocks() {
            CleanUnlockStates();
            CleanTechProgress();
            LogEMUInfo($"Adding {unlocksToAdd.Count} new UnlockStates");
            foreach (TechTreeState.UnlockState state in unlockStatesToAdd) {
                bool foundState = false;
                for (int i = 0; i < TechTreeState.instance.unlockStates.Count(); i++) {
                    TechTreeState.UnlockState existingState = TechTreeState.instance.unlockStates[i];
                    if (existingState.unlockRef == null) {
                        LogEMUInfo("Skipping unlock with null unlockRef");
                        continue;
                    }
                    if (state.unlockRef.uniqueId == existingState.unlockRef.uniqueId) {
                        TechTreeState.instance.unlockStates[i] = state;
                        foundState = true;
                        LogEMUInfo($"Updated UnlockState for Unlock #{state.unlockRef.uniqueId}");
                        i = TechTreeState.instance.unlockStates.Count();

                        LogEMUInfo($"Existing tech name: {LocsUtility.TranslateStringFromHash(existingState.unlockRef.displayNameHash)}");
                        break;
                    }
                }

                if (!foundState) {
                    TechTreeState.instance.unlockStates.AddItem(state);
                    LogEMUInfo($"Added UnlockState for Unlock #{state.unlockRef.uniqueId}");
                }
            }
        }
        
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

        private static bool AreListIntsEqual(List<int> list1, List<int> list2, bool sort) {
            if(list1.Count != list2.Count) return false;

            if (sort) {
                list1.Sort();
                list2.Sort();
            }

            for(int i = 0; i < list1.Count; i++) {
                if (list1[i] != list2[i]) return false;
            }

            return true;
        }

        // Public Functions For Other Devs

        #region Resources

        /// <summary>
        /// Finds the ResourceInfo that matches the name given in the argument.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired resource</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>ResourceInfo if successful, null if not.</returns>
        public static ResourceInfo GetResourceInfoByName(string name, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for resource with name '{name}'");
            if (hasGameDefinesLoaded) {
                if (resourceNameToIDMap.ContainsKey(name)) {
                    if (shouldLog) LogEMUInfo("Found resource in cache");
                    return GameDefines.instance.resources[resourceNameToIDMap[name]];
                }

                foreach (ResourceInfo info in GameDefines.instance.resources) {
                    if (info.displayName == name) {
                        if (shouldLog) LogEMUInfo($"Found resource with name '{name}'");
                        if (!resourceNameToIDMap.ContainsKey(name)) {
                            resourceNameToIDMap.Add(name, GameDefines.instance.resources.IndexOf(info));
                        }

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
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>resID if successful, -1 otherwise</returns>
        public static int GetResourceIDByName(string name, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for ID of resource with name '{name}'");
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

        /// <summary>
        /// Returns the MachineTypeEnum for the provided resID. Returns MachineTypeEnum.NONE for failed calls.
        /// </summary>
        /// <param name="resID">The ID of the ResourceInfo that you want the MachineTypeEnum value for</param>
        public static MachineTypeEnum GetMachineTypeFromResID(int resID) {
            if (!hasSaveStateLoaded) {
                LogEMUError("GetMachineTypeFromResID() called before SaveState.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.SaveStateLoaded or checking with ModUtils.hasSaveStateLoaded");
                return MachineTypeEnum.NONE;
            }

            try {
                return ((BuilderInfo)SaveState.GetResInfoFromId(resID)).GetInstanceType();
            }
            catch (Exception e) {
                LogEMUError($"Error occurred during GetMachineTypeFromResID(resID = {resID})");
                LogEMUError($"{e.Message}");
                LogEMUError($"{e.StackTrace}");
                return MachineTypeEnum.NONE;
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
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>Thresher recipe if successful, null if not</returns>
        public static SchematicsRecipeData FindThresherRecipeFromOutputs(string output1Name, string output2Name, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for thresher recipe with outputs: '{output1Name}', '{output2Name}'");
            if (!hasGameDefinesLoaded) {
                LogEMUWarning("FindThresherRecipeFromOutputs() called before GameDefines.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
                return null;
            }

            foreach (SchematicsRecipeData schematic in GameDefines.instance.schematicsRecipeEntries) {
                if (schematic.outputTypes.Count() == 2) {
                    if ((schematic.outputTypes[0].displayName == output1Name && schematic.outputTypes[1].displayName == output2Name) ||
                        (schematic.outputTypes[0].displayName == output2Name && schematic.outputTypes[1].displayName == output1Name)) {
                        if (shouldLog) LogEMUInfo("Found thresher recipe");
                        return schematic;
                    }
                }
            }

            LogEMUWarning("Could not find thresher recipe");
            LogEMUWarning("Try using the resource names in EquinoxsModUtils.ResourceNames");
            return null;
        }

        /// <summary>
        /// Tries to find the recipe that matches the ingredient provided in the arguments.
        /// </summary>
        /// <param name="ingredientResID">The ResourceInfo.uniqueId of the ingredient</param>
        /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
        /// <returns>Match recipe if successful, null otherwise</returns>
        public static SchematicsRecipeData TryFindThresherRecipe(int ingredientResID, bool shouldLog = false) {
            if (!hasGameDefinesLoaded) {
                LogEMUError($"TryFindThresherRecipe() called before GameDefines.instance loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return null;
            }

            if (shouldLog) LogEMUInfo($"Attempting to find thresher recipe with ingredient #{ingredientResID}");
            foreach (SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                if (recipe.ingTypes[0].uniqueId == ingredientResID) {
                    if (shouldLog) LogEMUInfo($"Found Thresher recipe for resource #{ingredientResID}");
                    return recipe;
                }
            }

            LogEMUError($"Could not find a recipe for resource #{ingredientResID}, please check this value.");
            return null;
        }

        /// <summary>
        /// Tries to find the recipe that matches the ingredients and results provided in the arguments.
        /// Save the results if calling more than once.
        /// </summary>
        /// <param name="ingredientIDs">List of the ResourceInfo.uniqueId's of the ingredients</param>
        /// <param name="resultIDs">List of the ResourceInfo.uniqueId's of the results</param>
        /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
        /// <returns>Matching recipe if successful, null otherwise</returns>
        public static SchematicsRecipeData TryFindRecipe(List<int> ingredientIDs, List<int> resultIDs, bool shouldLog = false) {
            if (!hasGameDefinesLoaded) {
                LogEMUError($"TryFindRecipe() called before GameDefines.instance loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return null;
            }

            if (shouldLog) LogEMUInfo("Attempting to find recipe");

            foreach (SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                List<int> recipeIngredients = recipe.ingTypes.Select(item => item.uniqueId).ToList();
                List<int> recipeResults = recipe.outputTypes.Select(item => item.uniqueId).ToList();

                if(AreListIntsEqual(ingredientIDs, recipeIngredients, true) && 
                   AreListIntsEqual(resultIDs, recipeResults, true)) {
                    if (shouldLog) LogEMUInfo($"Found recipe");
                    return recipe;
                }
            }

            LogEMUError($"Could not find recipe, please check the resource IDs passed in the arguments.");
            return null;
        }

        #endregion

        #region Tech Tree

        /// <summary>
        /// Finds the Unlock with the id given in the arguments
        /// </summary>
        /// <param name="id">The id of the desired Unlock</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns></returns>
        public static Unlock GetUnlockByID(int id, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for Unlock with id '{id}'");

            if (!hasGameDefinesLoaded) {
                LogEMUWarning("GetUnlockByID() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
                return null;
            }

            if (unlockCache.ContainsKey(id)) {
                if (shouldLog) LogEMUInfo($"Found unlock in cache");
                return unlockCache[id];
            }

            for(int i = 0; i < GameDefines.instance.unlocks.Count; i++) {
                Unlock tech = GameDefines.instance.unlocks[i];
                if (tech.uniqueId == id) {
                    if (shouldLog) LogEMUInfo($"Found unlock with id '{id}'");
                    unlockCache.Add(id, tech);
                    return tech;
                }
            }

            LogEMUWarning($"Couldn't find Unlock with id '{id}'");
            return null;
        }

        /// <summary>
        /// Finds the Unlock that matches the name given in the argument.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired Unlock</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>Unlock if successful, null if not</returns>
        public static Unlock GetUnlockByName(string name, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for Unlock with name '{name}'");

            if (!hasGameDefinesLoaded) {
                LogEMUWarning("GetUnlockByName() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
                return null;
            }

            if (unlockNameToIDMap.ContainsKey(name)) {
                if (GameDefines.instance.unlocks.Count > unlockNameToIDMap[name]) {
                    if (shouldLog) LogEMUInfo("Found unlock in cache");
                    return GameDefines.instance.unlocks[unlockNameToIDMap[name]];
                }
            }

            foreach (Unlock tech in GameDefines.instance.unlocks) {
                if (tech.displayNameHash == LocsUtility.GetHashString(name)) {
                    if (shouldLog) LogEMUInfo("Found Unlock");
                    if (!unlockNameToIDMap.ContainsKey(tech.displayNameHash)) {
                        unlockNameToIDMap.Add(tech.displayNameHash, tech.uniqueId);
                    }
                    return tech;
                }
            }

            LogEMUWarning($"Couldn't find Unlock with name '{name}'");
            LogEMUWarning("Try using a name from EquinoxsModUtils.UnlockNames");
            return null;
        }

        /// <summary>
        /// Registers a new Unlock to be added to the TechTree once GameDefines and TechTreeState have loaded.
        /// </summary>
        /// <param name="details">Details of the new Unlock. Ensure that all are provided.</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void AddNewUnlock(NewUnlockDetails details, bool shouldLog = false) {
            if (details.dependencyNames.Count > 2) {
                LogEMUError($"New Tech '{details.displayName}' cannot have more than 2 dependencies. Abandoning attempt to add.");
                return;
            }

            if(details.coreTypeNeeded != ResearchCoreDefinition.CoreType.Red &&
               details.coreTypeNeeded != ResearchCoreDefinition.CoreType.Green) {
                LogEMUError($"New Tech '{details.displayName}' needs to use either Red (Purple in-game) or Green (Blue in-game) cores. Aborting attempt to add tech.");
                return;
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

            //// Unlock State
            //unlockStatesToAdd.Add(new TechTreeState.UnlockState() {
            //    isActive = false,
            //    everActive = true,
            //    isDiscovered = true,
            //    scansCompleted = 0,
            //    scansRequired = 0,
            //    dependencies = new int[0],
            //    requirementFor = new int[0],
            //    tier = details.requiredTier,
            //    exists = true,
            //    unlockRef = newUnlock,
            //    unlockedRecipes = new List<SchematicsRecipeData>(),
            //    unlockedResources = new List<ResourceInfo>(),
            //    unlockedUpgrades = new List<UpgradeInfo>()
            //});

            if (shouldLog) LogEMUInfo($"Successfully created new Unlock '{details.displayName}'");
        }

        /// <summary>
        /// Used to change the sprite of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="unlockID">The uniqueId of the Unlock to update.</param>
        /// <param name="sprite">The new sprite to use</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockSprite(int unlockID, Sprite sprite, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update sprite of Unlock with ID '{unlockID}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GameDefines.instance.unlocks[unlockID];
                    unlock.sprite = sprite;
                    if (shouldLog) LogEMUInfo($"Updated sprite of Unlock with ID '{unlockID}'");
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
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockSprite(string displayName, Sprite sprite, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update sprite of Unlock '{displayName}'");
            if (hasGameDefinesLoaded) {
                Unlock unlock = GetUnlockByName(displayName);
                unlock.sprite = sprite;
                if (shouldLog) LogEMUInfo($"Updated sprite of Unlock '{displayName}'");
            }
            else {
                LogEMUWarning("UpdateUnlockSprite() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the treePosition of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="unlockID">The uniqueId of the Unlock to update.</param>
        /// <param name="treePosition">The new treePosition value to use</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockTreePosition(int unlockID, float treePosition, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update treePosition of Unlock with ID '{unlockID}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GetUnlockByID(unlockID);
                    unlock.treePosition = treePosition;
                    if (shouldLog) LogEMUInfo($"Updated treePosition of Unlock with ID '{unlockID}'");
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
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockTreePosition(string displayName, float treePosition, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update treePosition of Unlock '{displayName}'");
            if (hasGameDefinesLoaded) {
                Unlock unlock = GetUnlockByName(displayName);
                if(unlock != null) {
                    unlock.treePosition = treePosition;
                    //GameDefines.instance.unlocks[unlock.uniqueId].treePosition = treePosition;
                    if (shouldLog) LogEMUInfo($"Updated treePosition of Unlock '{displayName}' to {treePosition}");
                }
                else {
                    LogEMUWarning($"Could not update treePosition of unknown Unlock '{displayName}'");
                }
            }
            else {
                LogEMUWarning("UpdateUnlockTreePosition() called before GameDefines has loaded");
                LogEMUWarning("Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the requiredTier of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="unlockID">The uniqueId of the Unlock to update.</param>
        /// <param name="tier">The new requiredTier value to use</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockTier(int unlockID, TechTreeState.ResearchTier tier, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update tier of Unlock with ID '{unlockID}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GetUnlockByID(unlockID);
                    unlock.requiredTier = tier;
                    if (shouldLog) LogEMUInfo($"Updated requiredTier of Unlock with ID '{unlockID}'");
                }
                catch (Exception e) {
                    LogEMUError($"Error occurred while trying to update requiredTier of Unlock with ID '{unlockID}'");
                    LogEMUError(e.Message);
                    LogEMUError(e.StackTrace);
                }
            }
            else {
                LogEMUWarning($"UpdateUnlockTier() called before GameDefines has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        /// <summary>
        /// Used to change the requiredTier of an Unlock after GameDefines has loaded
        /// </summary>
        /// <param name="displayName">The displayName of the Unlock to update</param>
        /// <param name="tier">The new requiredTier value to use</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        public static void UpdateUnlockTier(string displayName, TechTreeState.ResearchTier tier, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Trying to update tier of Unlock '{displayName}'");
            if (hasGameDefinesLoaded) {
                try {
                    Unlock unlock = GetUnlockByName(displayName);
                    unlock.requiredTier = tier;
                    if (shouldLog) LogEMUInfo($"Updated requiredTier of Unlock '{displayName}'");
                }
                catch (Exception e) {
                    LogEMUError($"Error occurred while trying to update requiredTier of Unlock '{displayName}'");
                    LogEMUError(e.Message);
                    LogEMUError(e.StackTrace);
                }
            }
            else {
                LogEMUWarning($"UpdateUnlockTier() called before GameDefines has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking with ModUtils.hasGameDefinesLoaded");
            }
        }

        #endregion

        #region Machine Building

        /// <summary>
        /// Builds a machine that corresponds to the resID argument at the position and rotation given in gridInfo.
        /// </summary>
        /// <param name="resId">The resource ID of the type of machine you would like to build.</param>
        /// <param name="gridInfo">A GridInfo instance that contains the minPos and yawRotation of the machine you would like to build.</param>
        /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
        /// <param name="variationIndex">Optional - The variation to use for structure builds.</param>
        /// <param name="recipe">Optional - The recipe or filter that you would like to the machine to have selected.</param>
        /// <param name="chainData">Optional - The ChainData to use for building a conveyor belt</param>
        /// <param name="reverseConveyor">Optional - The value to use for ConveyorBuildInfo.isReversed</param>
        public static void BuildMachine(int resId, GridInfo gridInfo, bool shouldLog = false, int variationIndex = -1, int recipe = -1, ConveyorBuildInfo.ChainData? chainData = null, bool reverseConveyor = false) {
            if (!hasSaveStateLoaded) {
                LogEMUError("BuildMachine() called before SaveState.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.SaveStateLoaded or checking with ModUtils.hasSaveStateLoaded");
                return;
            }

            MachineBuilder.buildMachine(resId, gridInfo, shouldLog, variationIndex, recipe, chainData, reverseConveyor);
        }

        /// <summary>
        /// Builds a machine that corresponds to the resourceName argument at the position and rotation given in gridInfo.
        /// </summary>
        /// <param name="resourceName">The name of the machine that you would like to build</param>
        /// <param name="gridInfo">A GridInfo instance that contains the minPos and yawRotation of the machine you would like to build.</param>
        /// <param name="shouldLog">Whether EMU Info messages should be logged for this call</param>
        /// <param name="variationIndex">Optional - The variation to use for structure builds.</param>
        /// <param name="recipe">Optional - The recipe or filter that you would like to the machine to have selected.</param>
        /// <param name="chainData">Optional - The ChainData to use for building a conveyor belt</param>
        /// <param name="reverseConveyor">Optional - The value to use for ConveyorBuildInfo.isReversed</param>
        public static void BuildMachine(string resourceName, GridInfo gridInfo, bool shouldLog = false, int variationIndex = -1, int recipe = -1, ConveyorBuildInfo.ChainData? chainData = null, bool reverseConveyor = false) {
            if (!hasSaveStateLoaded) {
                LogEMUError("BuildMachine() called before SaveState.instance has loaded");
                LogEMUWarning("Try using the event ModUtils.SaveStateLoaded or checking with ModUtils.hasSaveStateLoaded");
                return;
            }

            int resID = GetResourceIDByName(resourceName);
            if(resID == -1) {
                LogEMUError($"Could not build machine '{resourceName}'. Couldn't find a resource matching this name.");
                LogEMUWarning($"Try using the ResourceNames class for a perfect match.");
                return;
            }

            MachineBuilder.buildMachine(resID, gridInfo, shouldLog, variationIndex, recipe, chainData, reverseConveyor);
        }

        #endregion

        #region Reflection And Misc

        /// <summary>
        /// Get the value of a private field from an instance of a non-static class.
        /// </summary>
        /// <typeparam name="T">The class that the field belongs to</typeparam>
        /// <param name="name">The name of the field</param>
        /// <param name="instance">The instance of the class that you would like to get the value from</param>
        /// <returns>The value of the field if successful (it can be null), default(V) otherwise</returns>
        public static object GetPrivateField<T>(string name, T instance) {
            FieldInfo field = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if(field == null) {
                LogEMUError($"Could not find the field '{name}' under type {typeof(T)}. Aborting attempt to get value");
                return default;
            }

            return field.GetValue(instance);
        }

        /// <summary>
        /// Sets the value of a private field on an instance of a non-static class.
        /// </summary>
        /// <typeparam name="T">The class that the field belongs to</typeparam>
        /// <param name="name">The name of the field</param>
        /// <param name="instance">The instance of the class that you would like to modify</param>
        /// <param name="value">The new value to set</param>
        public static void SetPrivateField<T>(string name, T instance, object value) {
            FieldInfo field = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if(field == null) {
                LogEMUError($"Could not find the field '{name}' under type {typeof(T)}. Aborting attempt to set value");
                return;
            }

            field.SetValue(instance, value);
        }

        /// <summary>
        /// Checks if the provided object is null and logs if it is null
        /// </summary>
        /// <param name="obj">The object to be checked</param>
        /// <param name="name">The name of the object to add to the log line</param>
        /// <param name="shouldLog">Whether an info message should be logged if the object is not null</param>
        /// <returns>true if not null</returns>
        public static bool NullCheck(object obj, string name, bool shouldLog = false) {
            if (obj == null) {
                LogEMUWarning($"{name} is null");
                return false;
            }
            else {
                if(shouldLog) LogEMUInfo($"{name} is not null");
                return true;
            }
        }

        /// <summary>
        /// Checks if a mod is installed by trying to find the dll provided in the argument.
        /// </summary>
        /// <param name="dllName">The name of the mod's dll file. You do not need to include .dll at the end</param>
        /// <param name="shouldLog">Whether an info message should be logged with the result</param>
        /// <returns></returns>
        public static bool IsModInstalled(string dllName, bool shouldLog = false) {
            dllName = dllName.Replace(".dll", "");
            string pluginsFolder = AppDomain.CurrentDomain.BaseDirectory + "BepInEx/plugins";
            string[] files = Directory.GetFiles(pluginsFolder);
            foreach(string file in files) {
                if (file.Contains(dllName)) {
                    if(shouldLog) LogEMUInfo($"Found {dllName}.dll, mod is installed");
                    return true;
                }
            }

            string[] modFolders = Directory.GetDirectories(pluginsFolder);
            foreach(string modFolder in modFolders) {
                string[] modFiles = Directory.GetFiles(modFolder);
                foreach(string modFile in modFiles) {
                    if (modFile.Contains(dllName)) {
                        if (shouldLog) LogEMUInfo($"Found {dllName}.dll, mod is installed");
                        return true;
                    }
                }
            }

            LogEMUWarning($"Could not find the file {dllName}.dll, mod is not installed");
            return false;
        }

        /// <summary>
        /// Gets a Texture2D of the Resource passed in the arguments for using in GUI.
        /// </summary>
        /// <param name="name">The name of the Resource.</param>
        /// <param name="shouldLog">Passed to GetResourceInfoByName()</param>
        /// <returns>A Texture2D of the Resource's Sprite for use in GUI.</returns>
        public static Texture2D GetImageForResource(string name, bool shouldLog = false) {
            ResourceInfo info = GetResourceInfoByName(name, shouldLog);
            Sprite sprite = info.sprite;
            if (sprite.rect.width != sprite.texture.width) {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else {
                return sprite.texture;
            }
        }

        #endregion
    }
}
