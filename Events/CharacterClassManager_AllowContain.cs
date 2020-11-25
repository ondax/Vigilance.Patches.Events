using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    public static class CharacterClassManager_AllowContain
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
                    return false;
                foreach (Player player in Server.PlayerList.Players.Values)
                {
                    if (!player.Hub.isDedicatedServer && player.Hub.Ready && Vector3.Distance(player.Hub.transform.position, __instance._lureSpj.transform.position) < 1.97f)
                    {
                        if (!player.IsAnySCP && player.IsAlive && !player.GodMode)
                        {
                            Environment.OnFemurEnter(player, true, out bool allow);
                            if (!allow)
                                return false;
                            player.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, player.PlayerId), player.GameObject, true);
                            __instance._lureSpj.SetState(true);
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.AllowContain), e);
                return true;
            }
        }
    }
}
