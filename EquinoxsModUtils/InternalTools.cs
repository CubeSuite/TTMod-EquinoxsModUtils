using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    internal static class InternalTools
    {
        // Members
        internal static bool printResources = false;
        internal static bool printUnlocks = false;
        internal static bool isCursorFree = false;

        // Internal Functions

        internal static void PrintAllResourceNames() {
            LogEMUInfo("Printing all resources\n\n");
            foreach (ResourceInfo info in GameDefines.instance.resources) {
                LogEMUInfo(info.displayName);
            }

            LogEMUInfo("\n\nPrinted all resources");
        }

        internal static void PrintAllUnlockNames() {
            LogEMUInfo("Printing all unlocks\n\n");
            foreach (Unlock unlock in GameDefines.instance.unlocks) {
                LogEMUInfo(LocsUtility.TranslateStringFromHash(unlock.displayNameHash));
            }
            
            LogEMUInfo("\n\nPrinted all unlocks");
        }

        internal static void CheckBepInExConfig() {
            string configFile = $"{Environment.CurrentDirectory}/BepInEx/config/BepInEx.cfg";
            LogEMUInfo($"Config file path: '{configFile}'");
            if (File.Exists(configFile)) {
                string text = File.ReadAllText(configFile);
                if (!text.Contains("HideManagerGameObject = true")) {
                    LogEMUError("HideGameManagerObject has not been set to true in BepInEx.cfg");
                    LogEMUError("Equinox's Mod Utils and any mods that use this library will not work if this is not enabled");
                }
                else {
                    LogEMUInfo("HideGameManagerObject has been correctly set to true");
                }
            }
        }
    }
}
