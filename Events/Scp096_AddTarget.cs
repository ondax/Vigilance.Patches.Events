using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.AddTarget))]
    public static class Scp096_AddTarget
    {
        public static bool Prefix(Scp096 __instance, GameObject target)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(target);
                if (player == null)
                    return true;
                if (!__instance.CanReceiveTargets || __instance._targets.Contains(player.Hub))
                    return false;
                if (!__instance._targets.IsEmpty())
                    __instance.AddReset();
                __instance._targets.Add(player.Hub);
                NetworkServer.SendToClientOfPlayer(player.Hub.characterClassManager.netIdentity, new Scp096ToTargetMessage(player.Hub));
                __instance.AdjustShield(ConfigManager.Scp096ShieldPerPlayer, true);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp096.AddTarget), e);
                return true;
            }
        }
    }
}
