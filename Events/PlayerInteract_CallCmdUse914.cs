using System;
using Harmony;
using Vigilance.API;
using Mirror;
using Scp914;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUse914))]
    public static class PlayerInteract_CallCmdUse914
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Scp914Machine.singleton.working || !__instance.ChckDis(Scp914Machine.singleton.button.position))
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Environment.OnSCP914Activate(player, (float)NetworkTime.time, true, out float time, out bool allow);
                if (!allow)
                    return false;
                Scp914Machine.singleton.RpcActivate(time);
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdUse914), e);
                return true;
            }
        }
    }
}
