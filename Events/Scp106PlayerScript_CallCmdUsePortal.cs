using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using MEC;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdUsePortal))]
    public static class Scp106PlayerScript_CallCmdUsePortal
    {
        public static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (!__instance._hub.playerMovementSync.Grounded)
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                if (__instance.iAm106 && __instance.portalPosition != Vector3.zero && !__instance.goingViaThePortal)
                {
                    Environment.OnSCP106Teleport(player, player.Position, __instance.portalPosition, true, out Vector3 other, out bool allow);
                    if (allow)
                        Timing.RunCoroutine(__instance._DoTeleportAnimation(), Segment.Update);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp106PlayerScript.CallCmdUsePortal), e);
                return true;
            }
        }
    }
}
