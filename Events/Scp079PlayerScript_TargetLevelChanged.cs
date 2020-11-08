using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.TargetLevelChanged))]
    public static class Scp079PlayerScript_TargetLevelChanged
    {
        public static bool Prefix(Scp079PlayerScript __instance, int newLvl)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (player == null)
                    return true;
                Environment.OnSCP079GainLvl(player, newLvl, true, out newLvl, out bool allow);
                if (allow)
                    __instance.Lvl = (byte)newLvl;
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp079PlayerScript.TargetLevelChanged), e);
                return true;
            }
        }
    }
}
