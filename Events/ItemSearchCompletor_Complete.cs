using System;
using Harmony;
using Vigilance.API;
using Searching;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
    public static class ItemSearchCompletor_Complete
    {
        private static bool Prefix(ItemSearchCompletor __instance)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(__instance.Hub);
                if (player == null)
                    return true;
                Environment.OnPickupItem(__instance.TargetPickup, player, true, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(nameof(ItemSearchCompletor.Complete), e);
                return true;
            }
        }
    }
}
