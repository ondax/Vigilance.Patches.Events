using System;
using Harmony;
using Vigilance.API;
using PlayableScps;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Enrage))]
    public static class Scp096_Enrage
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance.Hub);
                if (player == null)
                    return true;
                Environment.OnSCP096Enrage(player, true, out bool allow);
                if (!allow)
                    return false;
                if (__instance.Enraged)
                {
                    __instance.AddReset();
                    return false;
                }
                __instance.SetMovementSpeed(12f);
                __instance.SetJumpHeight(10f);
                __instance.PlayerState = Scp096PlayerState.Enraged;
                __instance.EnrageTimeLeft += __instance.EnrageTime;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp096.Enrage), e);
                return true;
            }
        }
    }
}
