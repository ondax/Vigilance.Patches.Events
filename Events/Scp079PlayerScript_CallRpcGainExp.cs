using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Mirror;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallRpcGainExp))]
    public static class Scp079PlayerScript_CallRpcGainExp
    {
        public static bool Prefix(Scp079PlayerScript __instance, ExpGainType type, RoleType details)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (player == null)
                    return true;
                switch (type)
                {
                    case ExpGainType.KillAssist:
                    case ExpGainType.PocketAssist:
                        {
                            Team team = player.Hub.characterClassManager.Classes.SafeGet(details).team;
                            int num = 6;
                            float num2;
                            switch (team)
                            {
                                case Team.SCP:
                                    num2 = __instance.GetManaFromLabel("SCP Kill Assist", __instance.expEarnWays);
                                    num = 11;
                                    break;
                                case Team.MTF:
                                    num2 = __instance.GetManaFromLabel("MTF Kill Assist", __instance.expEarnWays);
                                    num = 9;
                                    break;
                                case Team.CHI:
                                    num2 = __instance.GetManaFromLabel("Chaos Kill Assist", __instance.expEarnWays);
                                    num = 8;
                                    break;
                                case Team.RSC:
                                    num2 = __instance.GetManaFromLabel("Scientist Kill Assist", __instance.expEarnWays);
                                    num = 10;
                                    break;
                                case Team.CDP:
                                    num2 = __instance.GetManaFromLabel("Class-D Kill Assist", __instance.expEarnWays);
                                    num = 7;
                                    break;
                                default:
                                    num2 = 0f;
                                    break;
                            }
                            num--;
                            if (type == ExpGainType.PocketAssist)
                                num2 /= 2f;
                            if (NetworkServer.active)
                            {
                                Environment.OnSCP079GainExp(player, type, num2, true, out num2, out bool allow);
                                if (!allow)
                                    return false;
                                __instance.AddExperience(num2);
                                return false;
                            }
                            break;
                        }
                    case ExpGainType.DirectKill:
                    case ExpGainType.HardwareHack:
                        break;
                    case ExpGainType.AdminCheat:
                        try
                        {
                            if (NetworkServer.active)
                            {
                                float exp = (float)details;
                                Environment.OnSCP079GainExp(player, type, exp, true, out exp, out bool allow);
                                if (!allow)
                                    return false;
                                __instance.AddExperience(exp);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Add(nameof(Scp079PlayerScript.CallRpcGainExp), e);
                            GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error occured in RpcGainExp() while gaining points using Admin Cheats", MessageImportance.Normal, false);
                        }
                        break;
                    case ExpGainType.GeneralInteractions:
                        {
                            float num3 = 0f;
                            switch (details)
                            {
                                case RoleType.ClassD:
                                    num3 = __instance.GetManaFromLabel("Door Interaction", __instance.expEarnWays);
                                    break;
                                case RoleType.Spectator:
                                    num3 = __instance.GetManaFromLabel("Tesla Gate Activation", __instance.expEarnWays);
                                    break;
                                case RoleType.Scientist:
                                    num3 = __instance.GetManaFromLabel("Lockdown Activation", __instance.expEarnWays);
                                    break;
                                case RoleType.Scp079:
                                    num3 = __instance.GetManaFromLabel("Elevator Use", __instance.expEarnWays);
                                    break;
                            }
                            if (num3 != 0f)
                            {
                                float num4 = 1f / Mathf.Clamp(__instance.levels[__instance.curLvl].manaPerSecond / 1.5f, 1f, 7f);
                                num3 = Mathf.Round(num3 * num4 * 10f) / 10f;
                                if (NetworkServer.active)
                                {
                                    Environment.OnSCP079GainExp(player, type, num3, true, out num3, out bool allow);
                                    if (!allow)
                                        return false;
                                    __instance.AddExperience(num3);
                                    return false;
                                }
                            }
                            break;
                        }
                    default:
                        return false;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp079PlayerScript.CallRpcGainExp), e);
                return true;
            }
        }
    }
}
