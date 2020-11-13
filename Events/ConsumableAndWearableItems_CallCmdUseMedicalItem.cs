using System;
using Harmony;
using Vigilance.API;
using MEC;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem))]
    public static class UseMedicalPatch
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                __instance._cancel = false;
                if (__instance.cooldown > 0f)
                    return false;
                for (int i = 0; i < __instance.usableItems.Length; i++)
                {
                    if (__instance.usableItems[i].inventoryID == player.ItemInHand && __instance.usableCooldowns[i] <= 0f)
                    {
                        int hp = Mathf.CeilToInt(__instance.hpToHeal);
                        Environment.OnUseMedical(player, player.ItemInHand, hp, true, out hp, out bool allow);
                        __instance.hpToHeal = hp;
                        if (!allow)
                            return false;
                        Timing.RunCoroutine(__instance.UseMedicalItem(i), Segment.FixedUpdate);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem), e);
                return true;
            }
        }
    }
}
