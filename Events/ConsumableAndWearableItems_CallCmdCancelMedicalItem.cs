using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem))]
    public static class ConsumableAndWearableItems_CallCmdCancelMedicalItem
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Environment.OnCancelMedical(__instance.cooldown, player, __instance._hub.inventory.curItem, true, out __instance.cooldown, out bool allow);
                if (!allow)
                    return false;
                foreach (ConsumableAndWearableItems.UsableItem usableItem in __instance.usableItems)
                {
                    if (usableItem.inventoryID == __instance._hub.inventory.curItem && usableItem.cancelableTime > 0f)
                        __instance._cancel = true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem), e);
                return true;
            }
        }
    }
}
