using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using MEC;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    public static class CharacterClassManager_ApplyProperties
    {
        public static void Postfix(CharacterClassManager __instance, bool lite = false, bool escape = false)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return;
                Environment.OnSpawn(player, player.Position, player.Role, true, out Vector3 pos, out RoleType role, out bool allow);
                if (player.Distance(pos) > 1f)
                    player.Teleport(pos);
                if (player.Role != role)
                    player.SetRole(role, true, false);

                if (!allow)
                {
                    player.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(20000f, player.Nick, DamageTypes.Wall, player.PlayerId), player.GameObject);
                    return;
                }

                if (!ConfigManager.MakeSureToGiveItems)
                    return;
                Timing.CallDelayed(0.1f, () =>
                {
                    Inventory inventory = player.Hub.inventory;
                    if (inventory == null)
                    {
                        Log.Add(nameof(CharacterClassManager.ApplyProperties), $"Inventory of {player.Nick} ({__instance.UserId}) is null, trying to get component .. [1]", LogType.Warn);
                        inventory = player.GetComponent<Inventory>();
                    }

                    foreach (ItemType item in player.Hub.characterClassManager.CurRole.startItems)
                    {
                        if (inventory == null)
                        {
                            Log.Add(nameof(CharacterClassManager.ApplyProperties), $"Inventory of {player.Nick} ({__instance.UserId}) is null, trying to add component .. [2]", LogType.Warn);
                            inventory = player.AddComponent<Inventory>();
                        }

                        if (inventory == null)
                            Log.Add(nameof(CharacterClassManager.ApplyProperties), $"Inventory of {player.Nick} ({__instance.UserId}) is null [3]", LogType.Warn);

                        if (!player.HasItem(item))
                        {
                            player.AddItem(item);
                            Log.Add(nameof(CharacterClassManager.ApplyProperties), $"Adding {item} to {player.Nick}'s inventory [null: {(inventory == null).ToString().ToLower()}]", LogType.Warn);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.ApplyProperties), e);
            }
        }
    }
}
