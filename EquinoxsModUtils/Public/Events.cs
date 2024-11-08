using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils
{
    /// <summary>
    /// Container for all public content in EquinoxsModUtils
    /// </summary>
    public static partial class EMU 
    {
        /// <summary>
        /// Contains several events that are fired at various points of the load process and more.
        /// </summary>
        public static class Events
        {
            /// <summary>
            /// Fires when GameState.instance is no longer null
            /// </summary>
            public static event Action GameStateLoaded;

            /// <summary>
            /// Fires when GameDefines.instance is no longer null
            /// </summary>
            public static event Action GameDefinesLoaded;

            /// <summary>
            /// Fires when MachineManager.instance is no longer null
            /// </summary>
            public static event Action MachineManagerLoaded;

            /// <summary>
            /// Fires when SaveState.instance.metadata is no longer null.
            /// Does not fire for a new game as SaveState.instance.metadata.worldName is null.
            /// 'sender' argument is the world name
            /// </summary>
            public static event EventHandler SaveStateLoaded;

            /// <summary>
            /// Fires when TechTreeState.instance is no longer null
            /// </summary>
            public static event Action TechTreeStateLoaded;

            /// <summary>
            /// Fires whenever the game saves. Do not use this event for EMUAdditions.CustomData. 
            /// See EMUAdditions github for more information.
            /// 'sender' argument is the world name
            /// </summary>
            public static event EventHandler GameSaved;

            /// <summary>
            /// Fires when the loading screen is closed by the player pressing any key
            /// </summary>
            public static event Action GameLoaded;

            /// <summary>
            /// Fires when the player laods a save while already in-game.
            /// </summary>
            public static event Action GameUnloaded;

            // Internal Functions

            internal static void FireGameStateLoaded() {
                GameStateLoaded?.Invoke();
            }

            internal static void FireGameDefinesLoaded() {
                GameDefinesLoaded?.Invoke();
            }

            internal static void FireMachineManagerLoaded() {
                MachineManagerLoaded?.Invoke();
            }

            internal static void FireSaveStateLoaded() {
                SaveStateLoaded?.Invoke(SaveState.instance.metadata.worldName, EventArgs.Empty);
            }

            internal static void FireTechTreeStateLoaded() {
                TechTreeStateLoaded?.Invoke();
            }

            internal static void FireGameSaved() {
                GameSaved?.Invoke(SaveState.instance.metadata.worldName, EventArgs.Empty);
            }

            internal static void FireGameLoaded() {
                GameLoaded?.Invoke();
            }

            internal static void FireGameUnloaded() {
                GameUnloaded?.Invoke();
            }
        }
    }
}
