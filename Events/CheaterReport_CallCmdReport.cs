using System;
using System.Collections.Generic;
using Harmony;
using Vigilance.API;
using UnityEngine;
using RemoteAdmin;
using Console = GameCore.Console;
using Cryptography;
using System.Threading;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.CallCmdReport), typeof(int), typeof(string), typeof(byte[]), typeof(bool))]
    public static class CheaterReport_CallCmdReport
    {
        public static bool Prefix(CheaterReport __instance, int playerId, string reason, byte[] signature, bool notifyGm)
        {
            try
            {
                if (!__instance._commandRateLimit.CanExecute(true))
                    return false;
                float num = UnityEngine.Time.time - __instance._lastReport;
                Player player = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (player == null)
                    return true;
                GameConsoleTransmission gct = player.GetComponent<GameConsoleTransmission>();              
                if (num < 2f)
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded (1).", "red");
                    return false;
                }
                if (num > 60f)
                    __instance._reportedPlayersAmount = 0;
                if (__instance._reportedPlayersAmount > 5)
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded (2).", "red");
                    return false;
                }
                if (notifyGm && (!ServerStatic.GetPermissionsHandler().IsVerified || string.IsNullOrEmpty(ServerConsole.Password)))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Server is not verified - you can't use report feature on __instance server.", "red");
                    return false;
                }
                if (player.PlayerId == playerId)
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] You can't report yourself!", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reason))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Please provide a valid report reason!", "red");
                    return false;
                }
                ReferenceHub referenceHub;
                if (!ReferenceHub.TryGetHub(playerId, out referenceHub))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Can't find player with that PlayerID.", "red");
                    return false;
                }
                ReferenceHub hub = ReferenceHub.GetHub(__instance.gameObject);
                CharacterClassManager reportedCcm = referenceHub.characterClassManager;
                CharacterClassManager reporterCcm = hub.characterClassManager;
                if (__instance._reportedPlayers == null)
                    __instance._reportedPlayers = new HashSet<int>();
                if (__instance._reportedPlayers.Contains(playerId))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] You have already reported that player.", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reportedCcm.UserId))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Failed: User ID of reported player is null.", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reporterCcm.UserId))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Failed: your User ID of is null.", "red");
                    return false;
                }
                string reporterNickname = hub.nicknameSync.MyNick;
                string reportedNickname = referenceHub.nicknameSync.MyNick;
                if (!notifyGm)
                {
                    Environment.OnLocalReport(reason, Server.PlayerList.GetPlayer(reporterCcm._hub), Server.PlayerList.GetPlayer(reportedCcm._hub), true, out bool allo);
                    if (!allo)
                        return false;
                    Console.AddLog(string.Concat(new string[]
                    {
                        "Player ",
                        hub.LoggedNameFromRefHub(),
                        " reported player ",
                        referenceHub.LoggedNameFromRefHub(),
                        " with reason ",
                        reason,
                        "."
                    }), Color.gray, false);
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Player report successfully sent to local administrators.", "green");
                    if (CheaterReport.SendReportsByWebhooks)
                    {
                        new Thread(delegate ()
                        {
                            __instance.LogReport(gct, reporterCcm.UserId, reportedCcm.UserId, ref reason, playerId, false, reporterNickname, reportedNickname);
                        })
                        {
                            Priority = System.Threading.ThreadPriority.Lowest,
                            IsBackground = true,
                            Name = "Reporting player (locally) - " + reportedCcm.UserId + " by " + reporterCcm.UserId
                        }.Start();
                    }
                    return false;
                }
                if (signature == null)
                    return false;
                if (!ECDSA.VerifyBytes(reportedCcm.SyncedUserId + ";" + reason, signature, __instance.GetComponent<ServerRoles>().PublicKey))
                {
                    gct.SendToClient(__instance.connectionToClient, "[REPORTING] Invalid report signature.", "red");
                    return false;
                }
                __instance._lastReport = UnityEngine.Time.time;
                __instance._reportedPlayersAmount++;
                Environment.OnGlobalReport(reason, Server.PlayerList.GetPlayer(reporterCcm.gameObject), Server.PlayerList.GetPlayer(reportedCcm.gameObject), true, out bool allow);
                if (!allow)
                    return false;
                GameCore.Console.AddLog(string.Concat(new string[]
                {
                    "Player ",
                    hub.LoggedNameFromRefHub(),
                    " reported player ",
                    referenceHub.LoggedNameFromRefHub(),
                    " with reason ",
                    reason,
                    ". Sending report to Global Moderation."
                }), Color.gray, false);
                new Thread(delegate ()
                {
                    __instance.IssueReport(gct, reporterCcm.UserId, reportedCcm.UserId, reportedCcm.AuthToken, reportedCcm.connectionToClient.address, reporterCcm.AuthToken, reporterCcm.connectionToClient.address, ref reason, ref signature, ECDSA.KeyToString(__instance.GetComponent<ServerRoles>().PublicKey), playerId, reporterNickname, reportedNickname);
                })
                {
                    Priority = System.Threading.ThreadPriority.Lowest,
                    IsBackground = true,
                    Name = "Reporting player - " + reportedCcm.UserId + " by " + reporterCcm.UserId
                }.Start();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(CheaterReport.CallCmdReport), e);
                return true;
            }
        }
    }
}
