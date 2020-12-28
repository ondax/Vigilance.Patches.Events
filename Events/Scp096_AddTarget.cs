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
                Player myPlayer = Server.PlayerList.GetPlayer(__instance.Hub);
                if (player == null || myPlayer == null)
                    return true;
                if (player.Role == RoleType.Tutorial && !ConfigManager.CanTutorialTriggerScp096)
                    return false;
                if (!player.CanTriggerScp096)
                    return false;
                if (API.Ghostmode.CannotTriggerScp096.Contains(player))
                    return false;
                if (!__instance.CanReceiveTargets || __instance._targets.Contains(player.Hub))
                    return false;
                Environment.OnScp096AddTarget(myPlayer, player, true, out bool allow);
                if (!allow)
                    return false;
                if (!__instance._targets.IsEmpty() || __instance.Enraged)
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
