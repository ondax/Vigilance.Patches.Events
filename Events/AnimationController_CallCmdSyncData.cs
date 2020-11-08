using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.CallCmdSyncData))]
    public static class AnimationController_CallCmdSyncData
    {
        public static bool Prefix(AnimationController __instance, byte state, Vector2 v2)
        {
            try
            {
                if (!__instance._mSyncRateLimit.CanExecute(true))
                    return false;
                if (__instance.ccm == null || __instance.ccm._hub == null)
                    return true;
                Player player = Server.PlayerList.GetPlayer(__instance.ccm._hub);
                if (player == null)
                    return true;
                Environment.OnSyncData(player, v2, state, true, out state, out bool allow);
                if (!allow)
                    return false;
                __instance.NetworkcurAnim = state;
                __instance.Networkspeed = v2;
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
