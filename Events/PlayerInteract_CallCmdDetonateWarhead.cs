using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdDetonateWarhead))]
    public static class PlayerInteract_CallCmdDetonateWarhead
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                if (Map.OutsitePanelScript == null || Map.OutsitePanelScript.transform == null)
                    return true;
                if (Map.OutsitePanel == null)
                    return true;
                if (__instance._playerInteractRateLimit.CanExecute() && (__instance._hc.CufferId <= 0 || PlayerInteract.CanDisarmedInteract) && __instance._playerInteractRateLimit.CanExecute())
                {
                    if (__instance.ChckDis(Map.OutsitePanelScript.transform.position) && AlphaWarheadOutsitePanel.nukeside.enabled && Map.OutsitePanel.keycardEntered)
                    {
                        Environment.OnWarheadStart(player, AlphaWarheadController.Host.timeToDetonation, true, out AlphaWarheadController.Host.timeToDetonation, out bool allow);
                        AlphaWarheadController.Host.StartDetonation();
                        ServerLogs.AddLog(ServerLogs.Modules.Warhead, __instance._hub.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
                        __instance.OnInteract();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdDetonateWarhead), e);
                return true;
            }
        }
    }
}
