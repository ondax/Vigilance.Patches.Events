using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
    public static class PlayerInteract_CallCmdSwitchAWButton
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Map.OutsitePanelScript == null || Map.OutsitePanelScript.transform == null || Map.OutsitePanel == null)
                    return true;
                if (!__instance.ChckDis(Map.OutsitePanelScript.transform.position))
                    return false;
                Item itemByID = __instance._inv.GetItemByID(__instance._inv.curItem);
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return false;
                Environment.OnWarheadKeycardAccess(player, __instance._sr.BypassMode || (itemByID != null && itemByID.permissions.Contains("CONT_LVL_3")), out bool allow);
                if (allow)
                {
                    Map.OutsitePanel.NetworkkeycardEntered = !Map.OutsitePanel.NetworkkeycardEntered;
                    __instance.OnInteract();
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdSwitchAWButton), e);
                return true;
            }
        }
    }
}
