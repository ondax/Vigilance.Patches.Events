using Harmony;
using MapGeneration;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
    public static class SeedSynchronizer_Start
    {
        public static bool Prefix(SeedSynchronizer __instance)
        {
            try
            {
                int seed = Environment.GenerateMapSeed();
                Environment.OnGenerateSeed(seed, out seed);
                __instance.Network_syncSeed = seed;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }
}
