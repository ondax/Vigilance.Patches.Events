using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.OpenClose))]
    public static class Generator079_OpenClose
    {
        public static bool Prefix(Generator079 __instance, GameObject person)
        {
            Player player = Server.PlayerList.GetPlayer(person);
            if (player == null)
                return true;
            if (__instance._doorAnimationCooldown > 0f || __instance._deniedCooldown > 0f)
                return false;
            try
            {
                if (__instance.isDoorUnlocked)
                {
                    bool allow = true;
                    if (__instance.NetworkisDoorOpen)
                        Environment.OnGeneratorClose(__instance, player, true, out allow);
                    else
                        Environment.OnGeneratorOpen(__instance, player, true, out allow);
                    if (!allow)
                        return false;
                    __instance._doorAnimationCooldown = 1.5f;
                    __instance.NetworkisDoorOpen = !__instance.isDoorOpen;
                    __instance.RpcDoSound(__instance.isDoorOpen);
                    return false;
                }

                if (player.Hub.inventory.curItem > ItemType.KeycardJanitor)
                {
                    string[] permissions = player.Hub.inventory.GetItemByID(player.Hub.inventory.curItem).permissions;
                    for (int i = 0; i < permissions.Length; i++)
                    {
                        if (permissions[i] == "ARMORY_LVL_2" || player.BypassMode)
                        {
                            Environment.OnGeneratorUnlock(__instance, player, true, out bool allow);
                            if (!allow)
                                return false;
                            __instance.NetworkisDoorUnlocked = true;
                            __instance._doorAnimationCooldown = 0.5f;
                            return false;
                        }
                    }
                }
                __instance.RpcDenied();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Generator079.OpenClose), e);
                return true;
            }
        }
    }
}
