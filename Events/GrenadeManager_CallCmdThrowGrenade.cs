using System;
using Harmony;
using Vigilance.API;
using Grenades;
using UnityEngine;
using Mirror;
using MEC;
using Vigilance.Enums;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(GrenadeManager), nameof(GrenadeManager.CallCmdThrowGrenade))]
    public static class GrenadeManager_CallCmdThrowGrenade
    {
        public static bool Prefix(GrenadeManager __instance, int id, bool slowThrow, double time)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;
                if (id < 0 || __instance.availableGrenades.Length <= id)
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance.hub);
                if (player == null)
                    return true;
                GrenadeSettings grenadeSettings = __instance.availableGrenades[id];
                if (grenadeSettings.inventoryID != __instance.hub.inventory.curItem)
                    return false;
                float delay = Mathf.Clamp((float)(time - NetworkTime.time), 0f, grenadeSettings.throwAnimationDuration);
                float forceMultiplier = slowThrow ? 0.5f : 1f;
                Environment.OnThrowGrenade(player, grenadeSettings.grenadeInstance.GetComponent<Grenade>(), __instance, grenadeSettings.grenadeInstance, ((GrenadeType)(int)grenadeSettings.inventoryID), true, out bool allow);
                if (!allow)
                    return false;
                Timing.RunCoroutine(__instance._ServerThrowGrenade(grenadeSettings, forceMultiplier, __instance.hub.inventory.GetItemIndex(), delay), Segment.FixedUpdate);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(GrenadeManager.CallCmdThrowGrenade), e);
                return true;
            }
        }
    }
}
