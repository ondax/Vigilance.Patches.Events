using Harmony;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.CheckFinish))]
    public static class Generator079_CheckFinish
    {
        public static bool Prefix(Generator079 __instance)
        {
            try
            {
                if (__instance.prevFinish || __instance._localTime > 0.0)
                    return false;
                Environment.OnGeneratorFinish(__instance, true, out bool allow);
                if (!allow)
                    return false;
                __instance.prevFinish = true;
                __instance.epsenRenderer.sharedMaterial = __instance.matLetGreen;
                __instance.epsdisRenderer.sharedMaterial = __instance.matLedBlack;
                __instance._asource.PlayOneShot(__instance.unlockSound);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Generator079.CheckFinish), e);
                return true;
            }
        }
    }
}
