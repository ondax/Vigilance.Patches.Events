using System;
using GameCore;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class PlayerStats_Roundrestart
    {
        public static void Postfix(PlayerStats __instance)
        {
            try
            {
                Environment.OnRoundRestart();
                RagdollManager_SpawnRagdoll.Owners.Clear();
                RagdollManager_SpawnRagdoll.Ragdolls.Clear();
                if (ConfigManager.DisableLocksOnRestart)
                {
                    RoundSummary.RoundLock = false;
                    RoundStart.LobbyLock = false;
                } 
                Server.PlayerList.Reset();
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerStats.Roundrestart), e);
            }
        }
    }
}
