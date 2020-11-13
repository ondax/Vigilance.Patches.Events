using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
    public static class Handcuffs_CallCmdFreeTeammate
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                    return false;
                if (__instance.MyReferenceHub.characterClassManager.CurRole.team == Team.SCP)
                    return false;
                Player myPlayer = Server.PlayerList.GetPlayer(__instance.MyReferenceHub);
                Player myTarget = Server.PlayerList.GetPlayer(target);
                if (myPlayer == null || myTarget == null)
                    return true;
                Environment.OnUncuff(myTarget, myPlayer, true, out bool allow);
                if (allow)
                    myTarget.CufferId = -1;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Handcuffs.CallCmdFreeTeammate), e);
                return true;
            }
        }
    }
}
