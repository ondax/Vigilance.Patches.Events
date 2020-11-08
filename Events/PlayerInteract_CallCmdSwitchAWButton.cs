using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

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
                {
                    return false;
                }
                GameObject gameObject = GameObject.Find("OutsitePanelScript");
                if (!__instance.ChckDis(gameObject.transform.position))
                {
                    return false;
                }
                Item itemByID = __instance._inv.GetItemByID(__instance._inv.curItem);
                Player player = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (player == null)
                    return false;
                Environment.OnWarheadKeycardAccess(player, __instance._sr.BypassMode || (itemByID != null && itemByID.permissions.Contains("CONT_LVL_3")), out bool allow);
                if (allow)
                {
                    gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>().NetworkkeycardEntered = true;
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
