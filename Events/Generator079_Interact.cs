using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.Interact))]
    public static class Generator079_Interact
    {
        public static bool Prefix(Generator079 __instance, GameObject person, PlayerInteract.Generator079Operations command)
        {
            try
            {
                switch (command)
                {
                    case PlayerInteract.Generator079Operations.Door:
                        {
                            __instance.OpenClose(person);
                            return false;
                        }
                    case PlayerInteract.Generator079Operations.Tablet:
                        {
                            if (__instance.isTabletConnected || !__instance.isDoorOpen || __instance._localTime <= 0f || Generator079.mainGenerator.forcedOvercharge)
                                return false;
                            Player player = Server.PlayerList.GetPlayer(person);
                            if (player == null)
                                return true;
                            foreach (Inventory.SyncItemInfo item in player.Hub.inventory.items)
                            {
                                if (item.id == ItemType.WeaponManagerTablet)
                                {
                                    Environment.OnGeneratorInsert(__instance, player, true, out bool allow);
                                    if (!allow)
                                        return false;
                                    player.RemoveItem(item);
                                    __instance.NetworkisTabletConnected = true;
                                }
                            }
                            return false;
                        }
                    case PlayerInteract.Generator079Operations.Cancel:
                        break;
                    default:
                        return false;
                }
                Player ply = Server.PlayerList.GetPlayer(person);
                if (ply == null)
                    return true;
                Environment.OnGeneratorEject(__instance, ply, true, out bool allow2);
                if (!allow2)
                    return false;
                __instance.EjectTablet();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
