using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet;

namespace EquinoxsModUtils
{
    internal static class EMULogging
    {
        internal static void LogEMUInfo(string message) {
            ModUtils.Log.LogInfo(message);
        }

        internal static void LogEMUWarning(string message) {
            ModUtils.Log.LogWarning(message);
        }

        internal static void LogEMUError(string message) {
            ModUtils.Log.LogError(message);
        }

    }
}
