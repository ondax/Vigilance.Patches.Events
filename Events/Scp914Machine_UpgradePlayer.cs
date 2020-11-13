using System;
using System.Collections.Generic;
using Scp914;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradePlayer))]
    public static class Scp914Machine_UpgradePlayer
    {
		public static bool Prefix(Scp914Machine __instance, Inventory inventory, CharacterClassManager player, IEnumerable<CharacterClassManager> players)
		{
			try
			{
				Player ply = Server.PlayerList.GetPlayer(player._hub);
				if (ply == null)
					return true;
				Environment.OnScp914UpgradePlayer(ply, out bool allow);
				if (!allow)
					return false;
				for (int i = inventory.items.Count - 1; i > -1; i--)
				{
					Inventory.SyncItemInfo syncItemInfo = inventory.items[i];
					ItemType itemType = __instance.UpgradeItemID(syncItemInfo.id);
					if (itemType < ItemType.KeycardJanitor)
					{
						inventory.items.RemoveAt(i);
					}
					else
					{
						syncItemInfo.id = itemType;
						inventory.items[i] = syncItemInfo;
						Scp914Machine.TryFriendshipAchievement(itemType, player, players);
					}
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add(nameof(Scp914Machine.UpgradePlayer), e);
				return true;
            }
		}
    }
}
