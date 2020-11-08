using System;
using GameCore;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Respawning;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CallCmdRegisterEscape))]
    public static class CharacterClassManager_CallCmdRegisterEscape
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (Vector3.Distance(__instance.transform.position, __instance.GetComponent<Escape>().worldPosition) >= (float)(Escape.radius * 2))
                    return false;
                bool flag = false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Handcuffs handcuffs = __instance._hub.handcuffs;

                if (handcuffs.CufferId >= 0 && CharacterClassManager.CuffedChangeTeam)
                {
                    CharacterClassManager characterClassManager = ReferenceHub.GetHub(handcuffs.GetCuffer(handcuffs.CufferId)).characterClassManager;
                    if (__instance.CurClass == RoleType.Scientist && (characterClassManager.CurClass == RoleType.ChaosInsurgency || characterClassManager.CurClass == RoleType.ClassD))
                    {
                        flag = true;
                    }
                    if (__instance.CurClass == RoleType.ClassD && (characterClassManager.CurRole.team == Team.MTF || characterClassManager.CurClass == RoleType.Scientist))
                    {
                        flag = true;
                    }
                }
                Environment.OnCheckEscape(player, true, out bool allow);
                if (!allow)
                    return false;
                RespawnTickets singleton = RespawnTickets.Singleton;
                Team team = __instance.CurRole.team;
                if (team != Team.RSC)
                {
                    if (team == Team.CDP)
                    {
                        if (flag)
                        {
                            __instance.SetPlayersClass(RoleType.NtfCadet, __instance.gameObject, false, true);
                            RoundSummary.escaped_scientists++;
                            singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_classd_cuffed_count", 1), false);
                            return false;
                        }
                        __instance.SetPlayersClass(RoleType.ChaosInsurgency, __instance.gameObject, false, true);
                        RoundSummary.escaped_ds++;
                        singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_classd_count", 1), false);
                        return false;
                    }
                }
                else
                {
                    if (flag)
                    {
                        __instance.SetPlayersClass(RoleType.ChaosInsurgency, __instance.gameObject, false, true);
                        RoundSummary.escaped_ds++;
                        singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scientist_cuffed_count", 2), false);
                        return false;
                    }
                    __instance.SetPlayersClass(RoleType.NtfScientist, __instance.gameObject, false, true);
                    RoundSummary.escaped_scientists++;
                    singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_scientist_count", 1), false);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.CallCmdRegisterEscape), e);
                return true;
            }
        }
    }
}
