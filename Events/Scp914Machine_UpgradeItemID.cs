using System;
using Harmony;
using Scp914;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeItemID))]
    public static class Scp914Machine_UpgradeItem
    {
        public static bool Prefix(Scp914Machine __instance, ItemType itemID, ref ItemType __result)
        {
            try
            {
                Environment.OnScp914UpgradeItem(itemID, true, out ItemType output, out bool allow);
                if (!allow)
                {
                    Log.Add("Scp914", $"Upgrade has been disallowed, returning: {itemID}", ConsoleColor.Magenta);
                    __result = itemID;
                    return false;
                }
                __result = output;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp914Machine.UpgradeItem), e);
                return true;
            }
        }
    }
}
