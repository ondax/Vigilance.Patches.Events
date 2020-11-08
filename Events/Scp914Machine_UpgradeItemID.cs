using System;
using Harmony;
using Scp914;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeItem))]
    public static class Scp914Machine_UpgradeItem
    {
        public static bool Prefix(Scp914Machine __instance, Pickup item)
        {
            try
            {
                Environment.OnScp914UpgradeItem(item, out ItemType itemType);
                if (itemType < ItemType.KeycardJanitor)
                {
                    item.Delete();
                    return false;
                }

                item.SetIDFull(itemType);
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
