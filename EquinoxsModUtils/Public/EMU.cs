using EquinoxsModUtils.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    /// <summary>
    /// Container for all public content in EquinoxsModUtils
    /// </summary>
    public static partial class EMU
    {
        /// <summary>
        /// Get the value of a private field from an instance of a non-static class.
        /// </summary>
        /// <typeparam name="T">The class that the field belongs to</typeparam>
        /// <param name="name">The name of the field</param>
        /// <param name="instance">The instance of the class that you would like to get the value from</param>
        /// <returns>The value of the field if successful (it can be null), default(V) otherwise</returns>
        public static object GetPrivateField<T>(string name, T instance) {
            FieldInfo field = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) {
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
            if (field == null) {
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
        /// Checks if a mod is installed by trying to find the dll provided in the argument.
        /// </summary>
        /// <param name="dllName">The name of the mod's dll file. You do not need to include .dll at the end</param>
        /// <param name="shouldLog">Whether an info message should be logged with the result</param>
        /// <returns></returns>
        public static bool IsModInstalled(string dllName, bool shouldLog = false) {
            dllName = dllName.Replace(".dll", "");
            string pluginsFolder = AppDomain.CurrentDomain.BaseDirectory + "BepInEx/plugins";
            string[] files = Directory.GetFiles(pluginsFolder);
            foreach (string file in files) {
                if (file.Contains(dllName)) {
                    if (shouldLog) LogEMUInfo($"Found {dllName}.dll, mod is installed");
                    return true;
                }
            }

            string[] modFolders = Directory.GetDirectories(pluginsFolder);
            foreach (string modFolder in modFolders) {
                string[] modFiles = Directory.GetFiles(modFolder);
                foreach (string modFile in modFiles) {
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
        /// Unlocks the cursor and allows the player to move it around.
        /// Also tells the game a menu is open.
        /// </summary>
        /// <param name="free">True to free the cursor, false to lock it to the center of the screen again.</param>
        public static void FreeCursor(bool free) {
            if (free && InternalTools.isCursorFree) return;
            if (!free && !InternalTools.isCursorFree) return;

            InputHandler.instance.uiInputBlocked = free;
            InputHandler.instance.playerAimStickBlocked = free;
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
            UIManagerPatch.freeCursor = free;
        }

    }
}
