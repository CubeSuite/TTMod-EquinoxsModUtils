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
        /// Contains bools that can be used to check if certain aspects of the game have loaded yet
        /// </summary>
        public static class LoadingStates
        {
            // Members
            private static bool loadingUIObserved = false;

            /// <summary>
            /// Set to true once GameState.instance is no longer null
            /// </summary>
            public static bool hasGameStateLoaded;

            /// <summary>
            /// Set to true once GameDefines.instance is no longer null
            /// </summary>
            public static bool hasGameDefinesLoaded;

            /// <summary>
            /// Set to true once MachineManager.instance is no longer null
            /// </summary>
            public static bool hasMachineManagerLoaded;

            /// <summary>
            /// Set to true once SaveState.instance.metadata.worldName is no longer null or empty
            /// </summary>
            public static bool hasSaveStateLoaded;

            /// <summary>
            /// Set to true once TechTreeState.instance is no longer null
            /// </summary>
            public static bool hasTechTreeStateLoaded;

            /// <summary>
            /// Set to true once the loading screen has been closed by the player pressing any key
            /// </summary>
            public static bool hasGameLoaded;

            // Internal Functions

            internal static void CheckLoadingStates() {
                if (hasGameLoaded) return;

                if (!hasGameStateLoaded) CheckIfGameStateLoaded();
                if (!hasGameDefinesLoaded) CheckIfGameDefinesLoaded();
                if (!hasSaveStateLoaded) CheckIfSaveStateLoaded();
                if (!hasTechTreeStateLoaded) CheckIfTechTreeStateLoaded();
                if (!hasMachineManagerLoaded) CheckIfMachineManagerLoaded();
                if (!hasGameLoaded) CheckIfGameLoaded();
            }

            // Private Functions

            private static void CheckIfGameStateLoaded() {
                if (GameLogic.instance == null) return;
                if (GameState.instance != null) {
                    hasGameStateLoaded = true;
                    Events.FireGameStateLoaded();
                    LogEMUInfo("GameState.intance loaded.");
                }
            }

            private static void CheckIfGameDefinesLoaded() {
                if (GameLogic.instance == null) return;
                if (GameDefines.instance != null) {
                    hasGameDefinesLoaded = true;
                    Events.FireGameDefinesLoaded();
                    LogEMUInfo("GameDefines.instace loaded");

                    // ToDo: Remove if not needed
                    //foreach (Unlock unlock in GameDefines.instance.unlocks) {
                    //    string name = LocsUtility.TranslateStringFromHash(unlock.displayNameHash);
                    //    if (unlock.requiredTier == TechTreeState.ResearchTier.NONE) {
                    //        LogEMUWarning($"Unlock '{name}' has required Tier NONE, setting to Tier0");
                    //        unlock.requiredTier = TechTreeState.ResearchTier.Tier0;
                    //    }
                    //}

                    if (InternalTools.printResources) InternalTools.PrintAllResourceNames();
                    if (InternalTools.printUnlocks) InternalTools.PrintAllUnlockNames();
                }
            }

            private static void CheckIfSaveStateLoaded() {
                if (GameLogic.instance == null) return;
                if (SaveState.instance == null) return;
                if (SaveState.instance.metadata == null) return;
                if (string.IsNullOrEmpty(SaveState.instance.metadata.worldName)) return;

                hasSaveStateLoaded = true;

                Events.FireSaveStateLoaded();
                LogEMUInfo("SaveState.instance loaded");
            }

            private static void CheckIfTechTreeStateLoaded() {
                if (MachineManager.instance == null) return;
                if (TechTreeState.instance != null) {
                    hasTechTreeStateLoaded = true;

                    Events.FireTechTreeStateLoaded();
                    LogEMUInfo("TechTreeState.instance loaded");
                }
            }

            private static void CheckIfMachineManagerLoaded() {
                if (MachineManager.instance == null) return;
                hasMachineManagerLoaded = true;
                Events.FireMachineManagerLoaded();
                LogEMUInfo("MachineManager.instance loaded");
            }

            private static void CheckIfGameLoaded() {
                if (LoadingUI.instance == null && !loadingUIObserved) {
                    return;
                }

                else if (LoadingUI.instance != null && !loadingUIObserved) {
                    loadingUIObserved = true;
                    return;
                }

                else if (LoadingUI.instance == null && loadingUIObserved) {
                    hasGameLoaded = true;
                    Events.FireGameLoaded();
                    LogEMUInfo("Game Loaded");
                }
            }
        }
    }
}
