using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    public static class TeslaGate_PlayerInRange
    {
        public static bool Prefix(TeslaGate __instance, ReferenceHub player)
        {
            try
            {
                Player ply = Server.PlayerList.GetPlayer(player);
                if (ply == null)
                    return true;
                if (Vector3.Distance(__instance.transform.position, ply.Position) <= __instance.sizeOfTrigger)
                {
                    if (ConfigManager.TeslaTriggerableRoles.Contains(ply.Role) && !Map.TeslaGatesDisabled && !ply.GodMode)
                    {
                        Environment.OnTriggerTesla(ply, __instance, true, out bool allow);
                        return allow;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(TeslaGate.PlayerInRange), e);
                return true;
            }
        }
    }
}
