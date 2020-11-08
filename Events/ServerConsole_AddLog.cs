using Harmony;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class ServerConsole_AddLog
    {
        public static void Prefix(string q)
        {
            try
            {
                Environment.OnConsoleAddLog(q);
                if (q == "Waiting for players...")
                    Environment.OnWaitingForPlayers();
            }
            catch (Exception e)
            {
                Log.Add(nameof(ServerConsole.AddLog), e);             
            }
        }
    }
}
