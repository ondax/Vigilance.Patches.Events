using System;
using Harmony;
using Vigilance.API;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.EndEnrage))]
    public static class Scp096_EndEnrage
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance.Hub);
                if (player == null)
                    return true;
                Environment.OnSCP096Calm(player, true, out bool allow);
                if (!allow)
                    return false;
                __instance.EndCharge();
                __instance.SetMovementSpeed(0f);
                __instance.SetJumpHeight(4f);
                __instance.ResetShield();
                __instance.PlayerState = Scp096PlayerState.Calming;
                __instance._calmingTime = 6f;
                __instance._targets.Clear();
                NetworkServer.SendToClientOfPlayer(__instance.Hub.characterClassManager.netIdentity, new Scp096ToSelfMessage(__instance.EnrageTimeLeft, __instance._chargeCooldown));
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp096.EndEnrage), e);
                return true;
            }
        }
    }
}
