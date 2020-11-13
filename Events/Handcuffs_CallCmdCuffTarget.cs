using System;
using GameCore;
using Harmony;
using UnityEngine;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdCuffTarget))]
    public static class Handcuffs_CallCmdCuffTarget
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                    return false;
                Player targetPlayer = Server.PlayerList.GetPlayer(target);
                Player owner = Server.PlayerList.GetPlayer(__instance.MyReferenceHub);
                if (targetPlayer == null || owner == null)
                    return true;
                if (__instance.MyReferenceHub.inventory.curItem != ItemType.Disarmer || __instance.MyReferenceHub.characterClassManager.CurClass < RoleType.Scp173)
                    return false;
                if (targetPlayer.Hub.handcuffs.CufferId < 0)
                {
                    if (!ConfigManager.AllowCuffWhileHolding && targetPlayer.Hub.inventory.curItem == ItemType.None)
                        return false;
                    Team team = __instance.MyReferenceHub.characterClassManager.CurRole.team;
                    Team team2 = targetPlayer.Hub.characterClassManager.CurRole.team;
                    bool flag = false;

                    if (team == Team.CDP)
                    {
                        if (team2 == Team.MTF || team2 == Team.RSC)
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.RSC)
                    {
                        if (team2 == Team.CHI || team2 == Team.CDP)
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.CHI)
                    {
                        if (team2 == Team.MTF || team2 == Team.RSC)
                        {
                            flag = true;
                        }

                        if (team2 == Team.CDP && ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d", false))
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.MTF)
                    {
                        if (team2 == Team.CHI || team2 == Team.CDP)
                        {
                            flag = true;
                        }

                        if (team2 == Team.RSC && ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers", false))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        if (team2 == Team.MTF && team == Team.CDP)
                        {
                            owner.Achieve(Enums.Achievement.TablesHaveTurned);
                        }
                        __instance.ClearTarget();
                        targetPlayer.Hub.handcuffs.NetworkCufferId = __instance.MyReferenceHub.queryProcessor.PlayerId;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Handcuffs.CallCmdCuffTarget), e);
                return true;
            }
        }
    }
}
