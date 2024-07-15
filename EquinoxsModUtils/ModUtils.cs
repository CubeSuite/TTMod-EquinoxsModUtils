using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading.Tasks;
using AwesomeCharts;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EquinoxsModUtils.Patches;
using FIMSpace.Generating.Rules.Placement;
using FluffyUnderware.DevTools.Extensions;
using HarmonyLib;
using MessagePack.Formatters;
using Mirror;
using ProceduralNoiseProject;
using TriangleNet;
using UnityEngine;
using UnityEngine.Networking;
using Voxeland5;

namespace EquinoxsModUtils
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class ModUtils : BaseUnityPlugin
    {
        // Plugin Details
        private const string MyGUID = "com.equinox.EquinoxsModUtils";
        private const string PluginName = "EquinoxsModUtils";
        private const string VersionString = "5.1.0";

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
        
        private static string dataFolder = $"{Application.persistentDataPath}/Equinox's Mod Utils";

        internal static List<NewResourceDetails> resourcesToAdd = new List<NewResourceDetails>();
        internal static List<NewRecipeDetails> recipesToAdd = new List<NewRecipeDetails>();
        internal static List<SchematicsSubHeader> subHeadersToAdd = new List<SchematicsSubHeader>();
        private static List<Unlock> unlocksToAdd = new List<Unlock>();

        private static Dictionary<string, List<string>> unlockDependencies = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> resourceNameToIDMap = new Dictionary<string, int>();
        private static Dictionary<string, int> unlockNameToIDMap = new Dictionary<string, int>();
        private static Dictionary<int, Unlock> unlockCache = new Dictionary<int, Unlock>();

        public static Dictionary<string, string> hashTranslations = new Dictionary<string, string>();
        internal static List<TechTreeState.UnlockState> unlockStatesToAdd = new List<TechTreeState.UnlockState>();
        internal static List<string> customUnlockHashedNames = new List<string>();

        private static Dictionary<string, object> customMachineData = new Dictionary<string, object>();
        private static bool hasCustomMachineDataLoaded = false;

        private static float sSinceLastPacedLog = 0;

        // Events
        public static event EventHandler GameStateLoaded;
        public static event EventHandler GameDefinesLoaded;
        public static event EventHandler MachineManagerLoaded;
        public static event EventHandler SaveStateLoaded;
        public static event EventHandler TechTreeStateLoaded;

        public static event EventHandler GameSaved;
        public static event EventHandler GameLoaded;
        public static event EventHandler GameUnloaded;

        // Testing
        // ToDo: Disable before release

        private static bool doUnlockTest = false;
        private static bool printResources = false;
        private static bool printUnlocks = false;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            CheckBepInExConfig();

            Harmony.CreateAndPatchAll(typeof(FHG_UtilsPatch));
            Harmony.CreateAndPatchAll(typeof(FlowManagerPatch));
            Harmony.CreateAndPatchAll(typeof(GameDefinesPatch));
            Harmony.CreateAndPatchAll(typeof(LocsUtilityPatch));
            Harmony.CreateAndPatchAll(typeof(SaveStatePatch));
            Harmony.CreateAndPatchAll(typeof(StringPatternsList));
            Harmony.CreateAndPatchAll(typeof(TechActivatedSystemMessagePatch));
            Harmony.CreateAndPatchAll(typeof(TechTreeGridPatch));
            Harmony.CreateAndPatchAll(typeof(TechTreeStatePatch));
            Harmony.CreateAndPatchAll(typeof(UIManagerPatch));
            Harmony.CreateAndPatchAll(typeof(UnlockPatch));

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

            sSinceLastPacedLog += Time.deltaTime;
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
            //Debug.Log($"[EMU]: {message}");
            Log.LogInfo(message);
        }

        internal static void LogEMUWarning(string message) {
            //Debug.LogWarning($"[EMU]: {message}");
            Log.LogWarning(message);
        }

        internal static void LogEMUError(string message) {
            //Debug.LogError($"[EMU]: {message}");
            Log.LogError(message);
        }

        internal static void FireGameSavedEvent(string worldName) {
            GameSaved?.Invoke(worldName, EventArgs.Empty);
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
            if (SaveState.instance == null) return;
            if (SaveState.instance.metadata == null) return;
            if (string.IsNullOrEmpty(SaveState.instance.metadata.worldName)) return;

            hasSaveStateLoaded = true;
            LoadCustomMachineData(SaveState.instance.metadata.worldName);

            SaveStateLoaded?.Invoke(SaveState.instance, EventArgs.Empty);
            LogEMUInfo("SaveState.instance loaded");
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
                LoadUnlockStates(SaveState.instance.metadata.worldName);

                hasGameLoaded = true;
                GameLoaded?.Invoke(null, EventArgs.Empty);
                LogEMUInfo("Game Loaded");
            }
        }

        internal static void TriggerGameUnloaded() {
            GameUnloaded?.Invoke(null, EventArgs.Empty);
        }

        // Add New Techs

        private static void AddCustomUnlocks() {
            LogEMUInfo($"Adding {unlocksToAdd.Count} new Unlocks");
            for (int i = 0; i < unlocksToAdd.Count; i++) {
                Unlock unlock = unlocksToAdd[i];
                if(!NullCheck(unlock, "Unlock")) continue;

                if (FindDependencies(ref unlock)) {
                    unlock.uniqueId = GetNewUnlockUniqueID();
                    customUnlockHashedNames.Add(unlock.displayNameHash);
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

        // Data Functions

        internal static void SaveUnlockStates(string worldName) {
            List<string> fileLines = new List<string>();
            foreach (string name in ModUtils.customUnlockHashedNames) {
                Unlock unlock = ModUtils.GetUnlockByName(LocsUtility.TranslateStringFromHash(name));
                fileLines.Add($"{name}|{TechTreeState.instance.IsUnlockActive(unlock.uniqueId)}");
            }

            Directory.CreateDirectory(dataFolder);
            string saveFile = $"{dataFolder}/{worldName}.txt";
            File.WriteAllLines(saveFile, fileLines);
        }

        internal static void LoadUnlockStates(string worldName) {
            string saveFile = $"{dataFolder}/{worldName}.txt";
            if (!File.Exists(saveFile)) return;

            string[] fileLines = File.ReadAllLines(saveFile);
            foreach(string line in fileLines) {
                string hashedName = line.Split('|')[0];
                if (int.TryParse(hashedName, out int oldID)) continue;

                bool unlocked = bool.Parse(line.Split('|')[1]);
                if (!customUnlockHashedNames.Contains(hashedName)) {
                    LogEMUError($"Saved Unlock '{hashedName}' is not in customUnlocksIDs");
                    continue;
                }

                if (unlocked) {
                    UnlockTechAction action = new UnlockTechAction {
                        info = new UnlockTechInfo {
                            unlockID = ModUtils.GetUnlockByName(LocsUtility.TranslateStringFromHash(hashedName)).uniqueId,
                            drawPower = false
                        }
                    };
                    NetworkMessageRelay.instance.SendNetworkAction(action);
                }
            }
        }

        internal static void SaveCustomMachineData(string worldName) {
            List<string> fileLines = new List<string>();
            foreach(KeyValuePair<string, object> dataPair in customMachineData) {
                fileLines.Add($"{dataPair.Key}|{dataPair.Value}");
            }

            Directory.CreateDirectory(dataFolder);
            string saveFile = $"{dataFolder}/{worldName} CustomData.txt";
            File.WriteAllLines(saveFile, fileLines);
        }

        internal static void LoadCustomMachineData(string worldName) {
            string saveFile = $"{dataFolder}/{worldName} CustomData.txt";
            if (!File.Exists(saveFile)) {
                LogEMUWarning($"No CustomData save file found for world '{worldName}'");
                return;
            }

            string[] fileLines = File.ReadAllLines(saveFile);
            foreach(string line in fileLines) {
                string[] parts = line.Split('|');
                string valueString = parts[1];
                object value = null;
                
                string key = parts[0];
                string[] keyParts = key.Split('-');
                string type = keyParts[2];

                switch (type) {
                    case "System.UInt32": value = uint.Parse(valueString); break;
                    case "System.Int32": value = int.Parse(valueString); break;
                    case "System.Single": value = float.Parse(valueString); break;
                    case "System.Double": value = double.Parse(valueString); break;
                    case "System.Boolean": value = bool.Parse(valueString); break;
                    case "System.String": value = valueString; break;
                    case "System.Char": value = char.Parse(valueString); break;
                    default:
                        LogEMUError($"Cannot load custom data (key: '{key}') with unhandled type: '{type}'");
                        continue;
                }

                customMachineData[key] = value;
            }
            
            hasCustomMachineDataLoaded = true;
            Log.LogInfo("Loaded custom machine data");
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
        /// Finds the ResourceInfo that matches the name given in the argument without checking if GameDefines.instance has loaded.
        /// </summary>
        /// <param name="name">The displayName of the desired resource</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>ResourceInfo if successful, null if not.</returns>
        public static ResourceInfo GetResourceInfoByNameUnsafe(string name, bool shouldLog = false) {
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

        /// <summary>
        /// Requests EMU to create a Resource with the provided details at the correct time.
        /// </summary>
        /// <param name="details">Container for the details of your new resource</param>
        public static void AddNewResource(NewResourceDetails details) {
            resourcesToAdd.Add(details);
        }

        /// <summary>
        /// Finds a new uniqueId to use for a new instance of ResourceInfo or derived class.
        /// </summary>
        /// <param name="shouldLog">Whether the new ID should be logged in an EMU Info message.</param>
        /// <returns>The new ID to use</returns>
        public static int GetNewResourceID(bool shouldLog = false) {
            int max = 0;
            foreach (ResourceInfo info in GameDefines.instance.resources) {
                if (info.uniqueId > max) max = info.uniqueId;
            }

            if (shouldLog) LogEMUInfo($"Found new Resource ID: {max + 1}");
            return max + 1;
        }

        /// <summary>
        /// Updates the .unlock member of a ResourceInfo. Use once TechTreeState has loaded.
        /// </summary>
        /// <param name="resourceName">The display name of the ResourceInfo to update</param>
        /// <param name="unlockName">The display name of the Unlock that unlocks this item</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged on success. Passed to internal functions.</param>
        public static void UpdateResourceUnlock(string resourceName, string unlockName, bool shouldLog = false) {
            ResourceInfo resource = GetResourceInfoByName(resourceName, shouldLog);
            if (!NullCheck(resource, resourceName)) return;

            Unlock unlock = GetUnlockByName(unlockName, shouldLog);
            if(!NullCheck(unlock, unlockName)) return;

            resource.unlock = unlock;
            if (shouldLog) {
                LogEMUInfo($"Successfully set .unlock for {resource.displayName} to Unlock '{unlockName}'");
            }
        }

        /// <summary>
        /// Updates the .headerType member of a ResourceInfo. Use once GameDefines has loaded.
        /// </summary>
        /// <param name="resourceName">The display name of the ResourceInfo to update</param>
        /// <param name="header">The SchematicsSubHeader that the ResourceInfo should use</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged on success. Passed to internal functions.</param>
        public static void UpdateResourceHeaderType(string resourceName, SchematicsSubHeader header, bool shouldLog = false) {
            ResourceInfo resource = GetResourceInfoByName(resourceName, shouldLog);
            if (!NullCheck(resource, resourceName)) return;

            resource.headerType = header;
            if (shouldLog) {
                LogEMUInfo($"Successfully set .headerType for {resource.displayName}");
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

        /// <summary>
        /// Requests EMU to create a SchematicsRecipeData with the provided details at the correct time.
        /// </summary>
        /// <param name="details">Container for the details of your new recipe</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged if recipe is valid</param>
        public static void AddNewRecipe(NewRecipeDetails details, bool shouldLog = false) {
            if(details.ingredients.Count == 0) {
                LogEMUError("NewRecipeDetails has no ingredients, will not be added");
                return;
            }

            if(details.outputs.Count == 0) {
                LogEMUError("NweRecipeDetails has no outputs, will not be added");
                return;
            }
            
            recipesToAdd.Add(details);
            if(shouldLog) LogEMUInfo($"Registered NewRecipeDetails for adding to game: {details}");
        }

        /// <summary>
        /// Adds a new SchematicsSubHeader with the details provided in the arguments.
        /// </summary>
        /// <param name="title">The title of the new SchematicsSubHeader</param>
        /// <param name="parent">The parent SchematicsHeader this should appear under</param>
        /// <param name="priority">Controls where the sub-category should be placed</param>
        /// <param name="shouldLog">Whether an EMU Info messages should be logged on success</param>
        public static void AddNewSchematicsSubHeader(string title, string parentTitle, int priority, bool shouldLog = false) {
            SchematicsSubHeader subHeader = (SchematicsSubHeader)ScriptableObject.CreateInstance(typeof(SchematicsSubHeader));
            subHeader.title = $"{parentTitle}/{title}";
            subHeader.priority = priority;
            subHeadersToAdd.Add(subHeader);
            if (shouldLog) LogEMUInfo($"Registered new SchematicsSubHeader '{title}' for adding to game");
        }

        /// <summary>
        /// Tries to find the SchematicsHeader with a title matching the one in the argument.
        /// </summary>
        /// <param name="title">The title to search for.</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
        /// <returns>The SchematicsHeader if successful, null otherwise</returns>
        public static SchematicsHeader GetSchematicsHeaderByTitle(string title, bool shouldLog = false) {
            if (!NullCheck(GameDefines.instance, "GameDefines.instance")) {
                LogEMUError("GetSchematicsHeaderByTitle() called before GameDefines.instance has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return null;
            }

            foreach (SchematicsHeader header in GameDefines.instance.schematicsHeaderEntries) {
                if (header.title == title) {
                    if (shouldLog) LogEMUInfo($"Found SchematicsHeader with title '{title}'");
                    return header;
                }
            }

            LogEMUError($"Could not find SchematicsHeader with title '{title}'");
            return null;
        }

        /// <summary>
        /// Tries to find the SchematicsSubHeader with a title matching the one in the argument.
        /// </summary>
        /// <param name="title">The title to search for.</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged on success</param>
        /// <returns>The SchematicsSubHeader if successful, null otherwise</returns>
        public static SchematicsSubHeader GetSchematicsSubHeaderByTitle(string parentTitle, string title, bool shouldLog = false) {
            if (!NullCheck(GameDefines.instance, "GameDefines.instance")) {
                LogEMUError("GetSchematicsSubHeaderByTitle() called before GameDefines.instance has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return null;
            }

            foreach(SchematicsSubHeader subHeader in GameDefines.instance.schematicsSubHeaderEntries) {
                if (subHeader.title == title && subHeader.filterTag.title == parentTitle) {
                    if (shouldLog) LogEMUInfo($"Found SchematicsSubHeader with title '{title}'");
                    return subHeader;
                }
            }

            LogEMUError($"Could not find SchematicsSubHeader with title '{title}'");
            return null;
        }

        /// <summary>
        /// Finds a new uniqueId to use for a new SchematicsRecipeData instance.
        /// </summary>
        /// <param name="shouldLog">Whether the new ID should be logged in an EMU Info message.</param>
        /// <returns>The new ID if successful, -1 otherwise.</returns>
        public static int GetNewRecipeID(bool shouldLog = false) {
            if (!NullCheck(GameDefines.instance, "GameDefines.instance")) {
                LogEMUError("GetNewRecipeID() called before GameDefines.instance has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return -1;
            }

            int max = 0;
            foreach (SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                if (recipe.uniqueId > max) max = recipe.uniqueId;
            }

            if (shouldLog) LogEMUInfo($"Found new Recipe ID: {max + 1}");
            return max + 1;
        }

        /// <summary>
        /// Finds a new uniqueID to use for a new SchematicsSubHeader instance.
        /// </summary>
        /// <param name="shouldLog">Wheter the new ID should be logged in an EMU Info message.</param>
        /// <returns>The new ID if successful, -1 otherwise.</returns>
        public static int GetNewSchematicsSubHeaderID(bool shouldLog = false) {
            if(!NullCheck(GameDefines.instance, "GameDefines.instance")) {
                LogEMUError("GetNewSchematicsSubHeaderID() called before GameDefines.instance has loaded");
                LogEMUWarning($"Try using the event ModUtils.GameDefinesLoaded or checking ModUtils.hasGameDefinesLoaded.");
                return -1;
            }

            int max = 0;
            foreach(SchematicsSubHeader subHeader in GameDefines.instance.schematicsSubHeaderEntries) {
                if(subHeader.uniqueId > max) max = subHeader.uniqueId;
            }

            if (shouldLog) LogEMUInfo($"Found new SchematicsSubHeader ID: {max + 1}");
            return max + 1;
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
        /// Finds the Unlock that matches the name given in the argument without checking if GameDefines.instance is null.
        /// Language may affect this function.
        /// </summary>
        /// <param name="name">The displayName of the desired Unlock</param>
        /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
        /// <returns>Unlock if successful, null if not</returns>
        public static Unlock GetUnlockByNameUnsafe(string name, bool shouldLog = false) {
            if (shouldLog) LogEMUInfo($"Looking for Unlock with name '{name}'");

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

            if (hashTranslations.ContainsKey(displayNameHash)) {
                LogEMUError($"The Unlock name '{details.displayName}' is already in use. Abandoning attempt to add"); 
                return;
            }

            if (hashTranslations.ContainsKey(descriptionHash)) {
                LogEMUError($"The Unlock description '{details.description}' is already in use. Abandoning attempt to add");
                return;
            }

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
        /// Sets the value of a private static field
        /// </summary>
        /// <typeparam name="T">The class that the field belongs to</typeparam>
        /// <param name="name">The name of the field</param>
        /// <param name="instance">The instance of the class that you would like to modify</param>
        /// <param name="value">The new value to set</param>
        public static void SetPrivateStaticField<T>(string name, T instance, object value) {
            FieldInfo field = typeof(T).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null) {
                LogEMUError($"Could not find the static field '{name}' under type {typeof(T)}. Aborting attempt to set value");
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
        /// Copies all the values from 'original' onto 'target'. Be cautious of shared references.
        /// </summary>
        /// <typeparam name="T">The type of the object to be cloned.</typeparam>
        /// <param name="original">The object to copy fields from</param>
        /// <param name="target">The object to set fields for</param>
        public static void CloneObject<T>(T original, ref T target) {
            foreach (FieldInfo fieldInfo in target.GetType().GetFields()) {
                fieldInfo.SetValue(target, fieldInfo.GetValue(original));
            }
        }

        /// <summary>
        /// Loops through all members of 'obj' and logs its type, name and value.
        /// </summary>
        /// <param name="obj">The object to print all values of.</param>
        /// <param name="name">The name of the object to print at the start of the function.</param>
        public static void DebugObject(object obj, string name) {
            if (!NullCheck(obj, name)) {
                LogEMUError("Can't debug null object");
                return;
            }

            Dictionary<Type, string> basicTypeNames = new Dictionary<Type, string>
            {
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(char), "char" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
                { typeof(string), "string" }
            };

            Type objType = obj.GetType();
            FieldInfo[] fields = objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            LogEMUInfo($"Debugging {objType.Name} '{name}':");
            foreach (FieldInfo field in fields) {
                string value = field.GetValue(obj).ToString();
                string type = basicTypeNames.ContainsKey(field.FieldType) ? basicTypeNames[field.FieldType] : field.FieldType.ToString();

                if (type == "char") value = $"'{value}'";
                else if (type == "string") value = $"\"{value}\"";

                LogEMUInfo($"\t{type} {field.Name} = {value}");
            }
        }

        /// <summary>
        /// Calls Debug.Log(message) if the time since the last call is greater than delaySeconds
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="delaySeconds">How many seconds must pass before logging again. Default = 1s</param>
        public static void PacedLog(string message, float delaySeconds = 1f) {
            if(sSinceLastPacedLog > delaySeconds) {
                Debug.Log(message);
                sSinceLastPacedLog = 0;
            }
        }

        public static void FreeCursor(bool free) {
            InputHandler.instance.uiInputBlocked = free;
            InputHandler.instance.playerAimStickBlocked = free;
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
            UIManagerPatch.freeCursor = free;
        }

        #endregion

        #region Data Saving

        /// <summary>
        /// Adds a custom member for an instance of a machine if it has not already been added. See repo README for explanation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceId">The instanceId of the target machine.</param>
        /// <param name="name">The name of the new member.</param>
        /// <param name="value">The value of the new member.</param>
        public static void AddCustomDataForMachine<T>(uint instanceId, string name, T value) {
            List<string> acceptableTypes = new List<string>() {
                typeof(uint).ToString(),
                typeof(int).ToString(),
                typeof(float).ToString(),
                typeof(double).ToString(),
                typeof(bool).ToString(),
                typeof(string).ToString(),
                typeof(char).ToString(),
            };
            if (!acceptableTypes.Contains(typeof(T).ToString())) {
                LogEMUError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
            }

            string key = $"{instanceId}-{name}-{typeof(T)}";
            if (!customMachineData.ContainsKey(key)) {
                customMachineData.Add(key, value);
            }
        }

        /// <summary>
        /// Sets the value of a custom member for an instance of a machine. See repo README for explanation.
        /// </summary>
        /// <typeparam name="T">The type of the member. See repo README for acceptable types.</typeparam>
        /// <param name="instanceId">The instanceId of the target machine.</param>
        /// <param name="name">The name of the new member.</param>
        /// <param name="value">The value of the new member.</param>
        public static void UpdateCustomDataForMachine<T>(uint instanceId, string name, T value){
            List<string> acceptableTypes = new List<string>() {
                typeof(uint).ToString(),
                typeof(int).ToString(),
                typeof(float).ToString(),
                typeof(double).ToString(),
                typeof(bool).ToString(),
                typeof(string).ToString(),
                typeof(char).ToString(),
            };
            if (!acceptableTypes.Contains(typeof(T).ToString())) {
                LogEMUError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
            }

            string key = $"{instanceId}-{name}-{typeof(T)}";
            if (!customMachineData.ContainsKey(key)) {
                LogEMUWarning($"Custom data with key '{key}' has not been added for machine yet, adding instead of updating.");
                AddCustomDataForMachine(instanceId, name, value);
                return;
            }

            customMachineData[key] = value;
        }

        /// <summary>
        /// Gets the value of a custom member for an instance of a machine. See repo README for exlpanation.
        /// </summary>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <param name="instanceId">The instanceId of the target machine.</param>
        /// <param name="name">The name of the new member.</param>
        /// <returns>The value of the new member if successful, default(T) otherwise.</returns>
        public static T GetCustomDataForMachine<T>(uint instanceId, string name){
            if (!hasCustomMachineDataLoaded) {
                LogEMUError($"GetCustomDataForMachine() called before custom data has loaded.");
                LogEMUInfo($"Try using the SaveStateLoaded event or hasSaveStateLoaded variable");
                return default;
            }

            List<string> acceptableTypes = new List<string>() {
                typeof(uint).ToString(),
                typeof(int).ToString(),
                typeof(float).ToString(),
                typeof(double).ToString(),
                typeof(bool).ToString(),
                typeof(string).ToString(),
                typeof(char).ToString(),
            };
            if (!acceptableTypes.Contains(typeof(T).ToString())) {
                LogEMUError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
            }

            string key = $"{instanceId}-{name}-{typeof(T)}";
            if (customMachineData.ContainsKey(key)) {
                return (T)customMachineData[key];
            }
            else {
                LogEMUError($"Could not find custom data with key '{key}'");
                return default;
            }
        }

        /// <summary>
        /// Checks if any custom data exists for the machine instanceId provided in the argument.
        /// </summary>
        /// <param name="instanceId">The instanceId of the machine.</param>
        /// <returns>true if data is found, false if not</returns>
        public static bool CustomDataExistsForMachine(uint instanceId) {
            foreach(string key in customMachineData.Keys) {
                if (key.Split('-')[0] == instanceId.ToString()) return true;
            }

            return false;
        }

        #endregion

        #region Images

        /// <summary>
        /// Gets a Texture2D of the Resource passed in the arguments for using in GUI.
        /// </summary>
        /// <param name="name">The name of the Resource.</param>
        /// <param name="shouldLog">Passed to GetResourceInfoByName()</param>
        /// <returns>A Texture2D of the Resource's Sprite for use in GUI.</returns>
        public static Texture2D GetImageForResource(string name, bool shouldLog = false) {
            ResourceInfo info = GetResourceInfoByName(name, shouldLog);
            if (info == null) return null;

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

        /// <summary>
        /// Creates a Texture2D from the Embedded Resource at the path provided in the argument.
        /// </summary>
        /// <param name="path">The path of the Embedded Resource image.</param>
        /// <param name="shouldLog">Whether an EMU Info message should be logged on successful load.</param>
        /// <returns>Texture2D if file is found, null otherwise</returns>
        public static Texture2D LoadTexture2DFromFile(string resourceName, bool shouldLog = false, Assembly assembly = null) {
            if(assembly == null) assembly = Assembly.GetCallingAssembly();

            string[] resourceNames = assembly.GetManifestResourceNames();
            string fullPath = Array.Find(resourceNames, name => name.EndsWith(resourceName));

            if (fullPath == null) {
                LogEMUError($"Could not find image resource '{resourceName}' in mod assembly.");
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(fullPath)) {
                if (stream == null) {
                    LogEMUError($"Could not load image resource '{resourceName}' from mod assembly stream.");
                    return null;
                }

                using (MemoryStream memoryStream = new MemoryStream()) {
                    stream.CopyTo(memoryStream);
                    byte[] fileData = memoryStream.ToArray();

                    Texture2D output = new Texture2D(2, 2);
                    output.LoadImage(fileData);

                    if (shouldLog) LogEMUInfo($"Created Texture2D from image resource '{resourceName}'");

                    return output;
                }
            }
        }

        /// <summary>
        /// Calls LoadTexture2DFromFile() and converts the result to a Sprite.
        /// </summary>
        /// <param name="path">The path of the Embedded Resource image.</param>
        /// <param name="shouldLog">Passed to LoadTexture2DFromFile()</param>
        /// <returns></returns>
        public static Sprite LoadSpriteFromFile(string path, bool shouldLog = false) {
            Assembly assembly = Assembly.GetCallingAssembly();
            Texture2D texture = LoadTexture2DFromFile(path, shouldLog, assembly);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 512);
        }

        #endregion
    }
}
