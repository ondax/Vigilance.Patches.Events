using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdReload))]
    public static class WeaponManager_CallCmdReload
    {
        private static bool Prefix(WeaponManager __instance, ref bool animationOnly)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(false))
                    return false;
                int itemIndex = __instance._hub.inventory.GetItemIndex();
                if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count || (__instance.curWeapon < 0 || __instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID) || __instance._hub.inventory.items[itemIndex].durability >= (double)__instance.weapons[__instance.curWeapon].maxAmmo)
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Environment.OnReload(player, animationOnly, true, out animationOnly, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(nameof(WeaponManager.CallCmdReload), e);
                return true;
            }
        }
    }
}
