using EquinoxsDebuggingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    public static partial class EMU 
    {
        /// <summary>
        /// Contains several functions for working with Resources (ResourceInfo)
        /// </summary>
        public static class Resources 
        {
            // Members
            private static Dictionary<string, int> resourceNameToIndexMap = new Dictionary<string, int>();

            // Public Functions

            /// <summary>
            /// Finds the ResourceInfo that matches the name given in the argument.
            /// Language may affect this function.
            /// </summary>
            /// <param name="name">The displayName of the desired resource</param>
            /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
            /// <returns>ResourceInfo if successful, null if not.</returns>
            public static ResourceInfo GetResourceInfoByName(string name, bool shouldLog = false) {
                if (shouldLog) LogEMUInfo($"Looking for resource with name '{name}'");
                if (LoadingStates.hasGameDefinesLoaded) {
                    if (resourceNameToIndexMap.ContainsKey(name)) {
                        if (shouldLog) LogEMUInfo("Found resource in cache");
                        return GameDefines.instance.resources[resourceNameToIndexMap[name]];
                    }

                    foreach (ResourceInfo info in GameDefines.instance.resources) {
                        if (info.displayName == name) {
                            if (shouldLog) LogEMUInfo($"Found resource with name '{name}'");
                            if (!resourceNameToIndexMap.ContainsKey(name)) {
                                resourceNameToIndexMap.Add(name, GameDefines.instance.resources.IndexOf(info));
                            }

                            return info;
                        }
                    }
                }
                else {
                    LogEMUWarning("GetResourceInfoByName() was called before GameDefines.instance has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
                }

                LogEMUWarning($"Could not find resource with name '{name}'");
                LogEMUWarning($"Try using a name from EMU.Names.Resources");
                return null;
            }

            /// <summary>
            /// Finds the ResourceInfo that matches the name given in the argument without checking if GameDefines.instance has loaded.
            /// </summary>
            /// <param name="name">The displayName of the desired resource</param>
            /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
            /// <returns>ResourceInfo if successful, null if not.</returns>
            public static ResourceInfo GetResourceInfoByNameUnsafe(string name, bool shouldLog = false) {
                if (resourceNameToIndexMap.ContainsKey(name)) {
                    if (shouldLog) LogEMUInfo("Found resource in cache");
                    return GameDefines.instance.resources[resourceNameToIndexMap[name]];
                }

                foreach (ResourceInfo info in GameDefines.instance.resources) {
                    if (info.displayName == name) {
                        if (shouldLog) LogEMUInfo($"Found resource with name '{name}'");
                        if (!resourceNameToIndexMap.ContainsKey(name)) {
                            resourceNameToIndexMap.Add(name, GameDefines.instance.resources.IndexOf(info));
                        }

                        return info;
                    }
                }

                LogEMUWarning($"Could not find resource with name '{name}'");
                LogEMUWarning($"Try using a name from EMU.Names.Resources");
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
                if (LoadingStates.hasSaveStateLoaded) {
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
                    LogEMUWarning("Try using the event EMU.Events.SaveStateLoaded or checking with EMU.LoadingStates.hasSaveStateLoaded");
                    return -1;
                }
            }

            /// <summary>
            /// Returns the MachineTypeEnum for the provided resID. Returns MachineTypeEnum.NONE for failed calls.
            /// </summary>
            /// <param name="resID">The ID of the ResourceInfo that you want the MachineTypeEnum value for</param>
            public static MachineTypeEnum GetMachineTypeFromResID(int resID) {
                if (!LoadingStates.hasSaveStateLoaded) {
                    LogEMUError("GetMachineTypeFromResID() called before SaveState.instance has loaded");
                    LogEMUWarning("Try using the event EMU.Events.SaveStateLoaded or checking with EMU.LoadingStates.hasSaveStateLoaded");
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
            /// Updates the .unlock member of a ResourceInfo. Use once TechTreeState has loaded.
            /// </summary>
            /// <param name="resourceName">The display name of the ResourceInfo to update</param>
            /// <param name="unlockName">The display name of the Unlock that unlocks this item</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on success. Passed to internal functions.</param>
            public static void UpdateResourceUnlock(string resourceName, string unlockName, bool shouldLog = false) {
                if (!LoadingStates.hasTechTreeStateLoaded) {
                    LogEMUError("UpdateResourceUnlock() called before TechTreeState.instance has loaded.");
                    LogEMUWarning("Try using the event EMU.Events.TechTreeStateLoaded or checking with EMU.LoadingStates.hasTechTreeStateLoaded");
                    return;
                }
                
                ResourceInfo resource = GetResourceInfoByName(resourceName, shouldLog);
                if (!EDT.NullCheck(resource, resourceName)) return;

                Unlock unlock = EMU.Unlocks.GetUnlockByName(unlockName, shouldLog);
                if (!EDT.NullCheck(unlock, unlockName)) return;

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
                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUWarning("UPdateResourceHeaderType() was called before GameDefines.instance has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
                    return;
                }
                
                ResourceInfo resource = GetResourceInfoByName(resourceName, shouldLog);
                if (!EDT.NullCheck(resource, resourceName)) return;

                resource.headerType = header;
                if (shouldLog) {
                    LogEMUInfo($"Successfully set .headerType for {resource.displayName}");
                }
            }
        }
    }
}
