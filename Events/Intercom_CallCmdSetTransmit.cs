using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.CallCmdSetTransmit))]
    public static class Intercom_CallCmdSetTransmit
    {
        private static bool Prefix(Intercom __instance, bool player)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || Intercom.AdminSpeaking)
                    return false;
                Player ply = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (ply == null)
                    return true;
                if (player)
                {
                    if (!__instance.ServerAllowToSpeak())
                        return false;
                    Environment.OnIntercomSpeak(ply, true, out bool allow);
                    if (!allow)
                        return false;
                    Intercom.host.RequestTransmission(__instance.gameObject);
                }
                else
                {
                    if (!(Intercom.host.Networkspeaker == __instance.gameObject))
                        return false;
                    Environment.OnIntercomSpeak(ply, true, out bool allow);
                    if (!allow)
                        return false;
                    Intercom.host.RequestTransmission(null);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Intercom.CallCmdSetTransmit), e);
                return true;
            }
        }
    }
}
