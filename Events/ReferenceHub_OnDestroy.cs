using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class ReferenceHub_OnDestroy
    {
        public static bool Prefix(ReferenceHub __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance);
                if (player == null)
                    return true;
                ServerConsole.AddLog($"\"{player.Nick}\" disconnected from {player.IpAddress} ({player.UserId})", ConsoleColor.White);
                Environment.OnPlayerLeave(player, out bool destroy);
                Server.PlayerList.Remove(__instance);
                ReferenceHub.Hubs.Remove(__instance.gameObject);
                ReferenceHub.HubIds.Remove(__instance.queryProcessor.PlayerId);
                if (ReferenceHub._hostHub == __instance)
                    ReferenceHub._hostHub = null;
                if (ReferenceHub._localHub == __instance)
                    ReferenceHub._localHub = null;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(ReferenceHub.OnDestroy), e);
                return true;
            }
        }
    }
}
