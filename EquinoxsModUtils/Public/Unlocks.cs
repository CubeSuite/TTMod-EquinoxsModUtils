using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    public static partial class EMU 
    {
        /// <summary>
        /// Contains several functions for working with Unlocks / the Tech Tree
        /// </summary>
        public static class Unlocks 
        {
            // Members
            private static Dictionary<int, Unlock> unlockCache = new Dictionary<int, Unlock>();
            private static Dictionary<string, int> unlockNameToIDMap = new Dictionary<string, int>();

            // Public Functions

            /// <summary>
            /// Finds the Unlock with the id given in the arguments
            /// </summary>
            /// <param name="id">The id of the desired Unlock</param>
            /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
            /// <returns></returns>
            public static Unlock GetUnlockByID(int id, bool shouldLog = false) {
                if (shouldLog) LogEMUInfo($"Looking for Unlock with id '{id}'");

                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUWarning("GetUnlockByID() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
                    return null;
                }

                if (unlockCache.ContainsKey(id)) {
                    if (shouldLog) LogEMUInfo($"Found unlock in cache");
                    return unlockCache[id];
                }

                for (int i = 0; i < GameDefines.instance.unlocks.Count; i++) {
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

                if (!LoadingStates.hasGameDefinesLoaded) {
                    LogEMUWarning("GetUnlockByName() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                LogEMUWarning("Try using a name from EMU.Names.Unlocks");
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
                LogEMUWarning("Try using a name from EMU.Names.Unlocks");
                return null;
            }

            /// <summary>
            /// Used to change the sprite of an Unlock after GameDefines has loaded
            /// </summary>
            /// <param name="unlockID">The uniqueId of the Unlock to update.</param>
            /// <param name="sprite">The new sprite to use</param>
            /// <param name="shouldLog">Whether [EMU] Info messages should be logged for this call</param>
            public static void UpdateUnlockSprite(int unlockID, Sprite sprite, bool shouldLog = false) {
                if (shouldLog) LogEMUInfo($"Trying to update sprite of Unlock with ID '{unlockID}'");
                if (LoadingStates.hasGameDefinesLoaded) {
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
                    LogEMUError("UpdateUnlockSprite() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                if (LoadingStates.hasGameDefinesLoaded) {
                    Unlock unlock = GetUnlockByName(displayName);
                    unlock.sprite = sprite;
                    if (shouldLog) LogEMUInfo($"Updated sprite of Unlock '{displayName}'");
                }
                else {
                    LogEMUError("UpdateUnlockSprite() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                if (LoadingStates.hasGameDefinesLoaded) {
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
                    LogEMUError("UpdateUnlocktreePosition() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                if (LoadingStates.hasGameDefinesLoaded) {
                    Unlock unlock = GetUnlockByName(displayName);
                    if (unlock != null) {
                        unlock.treePosition = treePosition;
                        if (shouldLog) LogEMUInfo($"Updated treePosition of Unlock '{displayName}' to {treePosition}");
                    }
                    else {
                        LogEMUWarning($"Could not update treePosition of unknown Unlock '{displayName}'");
                    }
                }
                else {
                    LogEMUWarning("UpdateUnlockTreePosition() called before GameDefines has loaded");
                    LogEMUWarning("Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                if (LoadingStates.hasGameDefinesLoaded) {
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
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
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
                if (LoadingStates.hasGameDefinesLoaded) {
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
                    LogEMUError($"UpdateUnlockTier() called before GameDefines has loaded");
                    LogEMUWarning($"Try using the event EMU.Events.GameDefinesLoaded or checking with EMU.LoadingStates.hasGameDefinesLoaded");
                }
            }
        }
    }
}
