using Harmony;
using Vigilance.API;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public static class PlayerInteract_OnInteract
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Environment.OnInteract(player, true, out bool allow);
                if (allow && !ConfigManager.ShouldKeepScp268)
                    __instance._scp268.ServerDisable();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.OnInteract), e);
                return true;
            }
        }
    }
}
