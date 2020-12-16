using System;
using Harmony;
using Vigilance.API;
using System.Collections.Generic;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdDropItem))]
    public static class Inventory_CallCmdDropItem
    {
        public static Dictionary<Player, List<Pickup>> Pickups = new Dictionary<Player, List<Pickup>>();

        public static bool Prefix(Inventory __instance, int itemInventoryIndex)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || itemInventoryIndex < 0 ||
                    itemInventoryIndex >= __instance.items.Count)
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Inventory.SyncItemInfo syncItemInfo = __instance.items[itemInventoryIndex];
                if (__instance.items[itemInventoryIndex].id != syncItemInfo.id)
                    return false;
                Environment.OnDropItem(syncItemInfo, player, true, out syncItemInfo, out bool allow);
                if (!allow)
                    return false;
                Pickup droppedPickup = __instance.SetPickup(syncItemInfo.id, syncItemInfo.durability, __instance.transform.position, __instance.camera.transform.rotation, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
                __instance.items.RemoveAt(itemInventoryIndex);
                Environment.OnDroppedItem(droppedPickup, player);
                if (player != null)
                {
                    if (!Pickups.ContainsKey(player))
                        Pickups.Add(player, new List<Pickup>());
                    Pickups[player].Add(droppedPickup);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Inventory.CallCmdDropItem), e);
                return true;
            }
        }
    }
}
