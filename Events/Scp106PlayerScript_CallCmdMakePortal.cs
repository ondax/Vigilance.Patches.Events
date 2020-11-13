using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMakePortal))]
    public static class Scp106PlayerScript_CallCmdMakePortal
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
                Transform transform = __instance.transform;
                Debug.DrawRay(transform.position, -transform.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (__instance.iAm106 && !__instance.goingViaThePortal && Physics.Raycast(new Ray(__instance.transform.position, -__instance.transform.up), out raycastHit, 10f, __instance.teleportPlacementMask))
                {
                    Vector3 pos = raycastHit.point - Vector3.up;
                    Environment.OnSCP106CreatePortal(player, pos, true, out pos, out bool allow);
                    if (allow)
                        __instance.SetPortalPosition(pos);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp106PlayerScript.CallCmdMakePortal), e);
                return true;
            }
        }
    }
}
