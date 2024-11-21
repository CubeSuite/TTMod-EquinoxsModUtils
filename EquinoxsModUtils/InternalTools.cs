using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;
using static RootMotion.FinalIK.InteractionObject;

namespace EquinoxsModUtils
{
    internal static class InternalTools
    {
        // Members
        internal static bool printResources = false;
        internal static bool printUnlocks = false;
        internal static bool isCursorFree = false;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, String text, String caption, int type);

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
                    text = text.Replace("HideManagerGameObject = false", "HideManagerGameObject = true");
                    File.WriteAllText(configFile, text);

                    MessageBox(IntPtr.Zero, "Techtonica needs to restart to activate mods, after it closes, please launch it again.", "Please Restart", 0x00000040);
                    Application.Quit();
                }
                else {
                    LogEMUInfo("HideGameManagerObject has been correctly set to true");
                }
            }
            else {
                LogEMUError("Couldn't check BepInEx.cfg - doesn't exist");
            }
        }
    }
}
