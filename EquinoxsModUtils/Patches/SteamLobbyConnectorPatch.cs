using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Patches
{
    internal class SteamLobbyConnectorPatch
    {
        [HarmonyPatch(typeof(SteamLobbyConnector), "SteamMatchmaking_OnLobbyEntered")]
        [HarmonyPostfix]
        static void fireEvent() {
            ModUtils.hasGameLoaded = true;
        }
    }
}
