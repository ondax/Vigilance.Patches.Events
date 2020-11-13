using System;
using GameCore;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Mirror;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    public static class BanPlayer_BanUser
    {
        public static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
                    duration = int.MaxValue;
                string userId = null;
                string address = user.GetComponent<NetworkIdentity>().connectionToClient.address;
                Player targetPlayer = Server.PlayerList.GetPlayer(user);
                Player issuerPlayer = Server.PlayerList.GetPlayer(issuer);
                if (targetPlayer == null || issuerPlayer == null)
                    return true;
                reason = string.IsNullOrEmpty(reason) ? "No reason provided." : reason;
                try
                {
                    if (ConfigFile.ServerConfig.GetBool("online_mode", false))
                        userId = targetPlayer.UserId;
                }
                catch (Exception e)
                {
                    ServerConsole.AddLog("Failed during issue of User ID ban (1)!");
                    Log.Add(nameof(BanPlayer.BanUser), e);
                    return false;
                }
                string message = $"You have been {((duration > 0) ? "banned" : "kicked")}. ";
                if (!string.IsNullOrEmpty(reason))
                    message += "Reason: " + reason;
                if (!ServerStatic.PermissionsHandler.IsVerified || !targetPlayer.Hub.serverRoles.BypassStaff)
                {
                    if (duration > 0)
                    {
                        string originalName = string.IsNullOrEmpty(targetPlayer.Nick) ? "(no nick)" : targetPlayer.Nick;
                        long issuanceTime = TimeBehaviour.CurrentTimestamp();
                        long banExpieryTime = TimeBehaviour.GetBanExpirationTime((uint)duration);
                        Environment.OnBan(issuerPlayer, targetPlayer, reason, issuanceTime, banExpieryTime, true, out banExpieryTime, out bool allow);
                        if (!allow)
                            return false;
                        try
                        {
                            if (userId != null && !isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = userId,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.UserId);

                            }
                        }
                        catch (Exception e)
                        {
                            ServerConsole.AddLog("Failed during issue of User ID ban (2)!");
                            Log.Add(nameof(BanPlayer.BanUser), e);
                            return true;
                        }

                        try
                        {
                            if (ConfigFile.ServerConfig.GetBool("ip_banning", false) || isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = address,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.IP);
                            }
                        }
                        catch (Exception e)
                        {
                            ServerConsole.AddLog("Failed during issue of IP ban!");
                            Log.Add(nameof(BanPlayer.BanUser), e);
                            return true;
                        }
                    }
                }

                ServerConsole.Disconnect(targetPlayer.GameObject, message);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(BanPlayer.BanUser), e);
                return true;
            }
        }
    }
}
