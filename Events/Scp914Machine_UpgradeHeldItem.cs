using System;
using System.Collections.Generic;
using Scp914;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeHeldItem))]
    public static class Scp914Machine_UpgradeHeldItem
    {
        public static bool Prefix(Scp914Machine __instance, Inventory inventory, CharacterClassManager player, IEnumerable<CharacterClassManager> players)
        {
			try
			{
				if (inventory.curItem == ItemType.None)
					return false;
				Player ply = Server.PlayerList.GetPlayer(player._hub);
				if (ply == null)
					return true;
				Environment.OnScp914UpgradeHeldItem(ply, inventory.GetItemInHand(), out ItemType itemType);
				int itemIndex = inventory.GetItemIndex();
				if (itemIndex < 0 || itemIndex >= inventory.items.Count)
					return false;
				if (itemType == ItemType.None)
				{
					inventory.items.RemoveAt(itemIndex);
					return false;
				}
				Inventory.SyncItemInfo syncItemInfo = inventory.items[itemIndex];
				Item itemByID = Pickup.Inv.GetItemByID(itemType);
				if (syncItemInfo.durability > itemByID.durability)
					syncItemInfo.durability = itemByID.durability;
				if (itemByID.id == ItemType.MicroHID)
					player.GetComponent<MicroHID>().NetworkEnergy = 1f;
				if (itemByID.id == ItemType.GunLogicer)
				{
					syncItemInfo.modBarrel = 0;
					syncItemInfo.modOther = 0;
					syncItemInfo.modSight = 0;
				}
				syncItemInfo.id = itemType;
				inventory.items[itemIndex] = syncItemInfo;
				Scp914Machine.TryFriendshipAchievement(itemType, player, players);
				return false;
			}
			catch (Exception e)
            {
				Log.Add(nameof(Scp914Machine.UpgradeHeldItem), e);
				return true;
            }
		}
    }
}
