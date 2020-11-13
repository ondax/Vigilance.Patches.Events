using System;
using Harmony;
using Vigilance.API;
using Scp914;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdChange914Knob))]
    public static class PlayerInteract_CallCmdChange914Knob
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Scp914Machine.singleton.working || !__instance.ChckDis(Scp914Machine.singleton.knob.position))
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Scp914Knob scp914Knob = Scp914Machine.singleton.knobState + 1;
                Environment.OnSCP914ChangeKnob(player, scp914Knob, true, out scp914Knob, out bool allow);
                if (!allow)
                    return false;
                Scp914Machine.singleton.NetworkknobState = scp914Knob;
                if (scp914Knob > Scp914Machine.knobStateMax)
                    Scp914Machine.singleton.NetworkknobState = Scp914Machine.knobStateMin;
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdChange914Knob), e);
                return true;
            }
        }
    }
}
