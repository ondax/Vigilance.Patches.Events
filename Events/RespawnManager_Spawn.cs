using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Respawning.NamingRules;
using UnityEngine;
using NorthwoodLib.Pools;
using Respawning;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
    public static class RespawnManager_Spawn
    {
        public static bool Prefix(RespawnManager __instance)
        {
            try
            {
                SpawnableTeam spawnableTeam;
                if (!RespawnWaveGenerator.SpawnableTeams.TryGetValue(__instance.NextKnownTeam, out spawnableTeam) || __instance.NextKnownTeam == SpawnableTeamType.None)
                {
                    ServerConsole.AddLog("Fatal error. Team '" + __instance.NextKnownTeam.ToString() + "' is undefined.", ConsoleColor.Red);
                    return false;
                }
                List<ReferenceHub> list = ReferenceHub.GetAllHubs().Values.Where(h => !h.serverRoles.OverwatchEnabled && h.characterClassManager.NetworkCurClass == RoleType.Spectator && h.characterClassManager.NetworkCurClass != RoleType.None).ToList();
                if (__instance._prioritySpawn)
                    list = list.OrderBy(h => h.characterClassManager.DeathTime).ToList();
                else
                    list.ShuffleList();
                RespawnTickets singleton = RespawnTickets.Singleton;
                int num = singleton.GetAvailableTickets(__instance.NextKnownTeam);
                if (num == 0)
                {
                    num = RespawnTickets.DefaultTeamAmount;
                    RespawnTickets.Singleton.GrantTickets(RespawnTickets.DefaultTeam, RespawnTickets.DefaultTeamAmount, true);
                }
                int num2 = Mathf.Min(num, spawnableTeam.MaxWaveSize);
                Environment.OnTeamRespawn(list.Select(h => h.gameObject).ToList(), __instance.NextKnownTeam, true, out __instance.NextKnownTeam, out bool allow);
                if (!allow)
                    return false;
                while (list.Count > num2)
                {
                    list.RemoveAt(list.Count - 1);
                }
                list.ShuffleList();
                List<ReferenceHub> list2 = ListPool<ReferenceHub>.Shared.Rent();
                foreach (ReferenceHub referenceHub in list)
                {
                    try
                    {
                        RoleType classid = spawnableTeam.ClassQueue[Mathf.Min(list2.Count, spawnableTeam.ClassQueue.Length - 1)];
                        referenceHub.characterClassManager.SetPlayersClass(classid, referenceHub.gameObject, false, false);
                        list2.Add(referenceHub);
                        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                        {
                            "Player ",
                            referenceHub.LoggedNameFromRefHub(),
                            " respawned as ",
                            classid.ToString(),
                            "."
                        }), ServerLogs.ServerLogType.GameEvent, false);
                    }
                    catch (Exception ex)
                    {
                        if (referenceHub != null)
                        {
                            Log.Add(nameof(RespawnManager.Spawn), ex);
                            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Player " + referenceHub.LoggedNameFromRefHub() + " couldn't be spawned. Err msg: " + ex.Message, ServerLogs.ServerLogType.GameEvent, false);
                        }
                        else
                        {
                            Log.Add(nameof(RespawnManager.Spawn), ex);
                            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Couldn't spawn a player - target's ReferenceHub is null.", ServerLogs.ServerLogType.GameEvent, false);
                        }
                    }
                }
                if (list2.Count > 0)
                {
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new object[]
                    {
                        "RespawnManager has successfully spawned ",
                        list2.Count,
                        " players as ",
                        __instance.NextKnownTeam.ToString(),
                        "!"
                    }), ServerLogs.ServerLogType.GameEvent, false);
                    RespawnTickets.Singleton.GrantTickets(__instance.NextKnownTeam, -list2.Count * spawnableTeam.TicketRespawnCost, false);
                    UnitNamingRule unitNamingRule;
                    if (UnitNamingRules.TryGetNamingRule(__instance.NextKnownTeam, out unitNamingRule))
                    {
                        string text;
                        unitNamingRule.GenerateNew(__instance.NextKnownTeam, out text);
                        foreach (ReferenceHub referenceHub2 in list2)
                        {
                            referenceHub2.characterClassManager.NetworkCurSpawnableTeamType = (byte)__instance.NextKnownTeam;
                            referenceHub2.characterClassManager.NetworkCurUnitName = text;
                        }
                        unitNamingRule.PlayEntranceAnnouncement(text);
                    }
                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, __instance.NextKnownTeam);
                }
                ListPool<ReferenceHub>.Shared.Return(list2);
                __instance.NextKnownTeam = SpawnableTeamType.None;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(RespawnManager.Spawn), e);
                return true;
            }
        }
    }
}
