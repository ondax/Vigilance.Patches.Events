using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
    public static class PlayerInteract_CallCmdContain106
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Map.LureSubjectContainer == null || Map.FemurBreaker == null || Map.FemurBreaker.transform == null)
                    return true;
                if (!Map.LureSubjectContainer.allowContain || (__instance._ccm.CurRole.team == Team.SCP && __instance._ccm.CurClass != RoleType.Scp106) || !__instance.ChckDis(Map.FemurBreaker.transform.position) || OneOhSixContainer.used || __instance._ccm.CurRole.team == Team.RIP)
                    return false;
                Player i = Server.PlayerList.GetPlayer(__instance._ccm._hub);
                if (i == null)
                    return true;
                foreach (Player player in Server.PlayerList.Players.Values)
                {
                    if (player.Role == RoleType.Scp106 && !player.GodMode)
                    {
                        Environment.OnSCP106Contain(i, player, true, out bool allow);
                        player.Hub.scp106PlayerScript.Contain(__instance._hub);
                        __instance.RpcContain106(__instance.gameObject);
                        OneOhSixContainer.used = true;
                    }
                }
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdContain106), e);
                return true;
            }
        }
    }
}
