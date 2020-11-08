using System;
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
                Server.PlayerList.Reset();
                RagdollManager_SpawnRagdoll.Owners.Clear();
                RagdollManager_SpawnRagdoll.Ragdolls.Clear();
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerStats.Roundrestart), e);
            }
        }
    }
}
