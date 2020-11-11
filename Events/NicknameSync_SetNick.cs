using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static class NicknameSync_SetNick
    {
        public static bool Prefix(NicknameSync __instance, string nick)
        {
            try
            {
                __instance.MyNick = nick;
                if (__instance.isLocalPlayer && ServerStatic.IsDedicated || __instance == null || string.IsNullOrEmpty(nick))
                    return false;
                Player player = Server.PlayerList.Add(__instance._hub);
                if (player == null)
                    return true;
                if (ServerGuard.SteamShield.CheckAccount(player))
                    return false;
                if (ServerGuard.VPNShield.CheckIP(player))
                    return false;
                Environment.OnPlayerJoin(player);
                ServerConsole.AddLog($"\"{player.Nick}\" joined from {player.IpAddress} ({player.UserId})", ConsoleColor.White);
                ServerLogs.AddLog(ServerLogs.Modules.Networking, $"Nickname of {player.UserId} is now {player.Nick}", ServerLogs.ServerLogType.ConnectionUpdate);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(NicknameSync.SetNick), e);
                return true;
            }
        }
    }
}
