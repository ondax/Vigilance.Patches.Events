using System;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseLocker))]
    public static class PlayerInteract_CallCmdUseLocker
    {
        private static bool Prefix(PlayerInteract __instance, byte lockerId, byte chamberNumber)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) ||
                    (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                LockerManager singleton = LockerManager.singleton;
                if (lockerId >= singleton.lockers.Length)
                    return false;
                if (!__instance.ChckDis(singleton.lockers[lockerId].gameObject.position) ||
                    !singleton.lockers[lockerId].supportsStandarizedAnimation)
                    return false;
                if (chamberNumber >= singleton.lockers[lockerId].chambers.Length)
                    return false;
                if (singleton.lockers[lockerId].chambers[chamberNumber].doorAnimator == null)
                    return false;
                if (!singleton.lockers[lockerId].chambers[chamberNumber].CooldownAtZero())
                    return false;
                singleton.lockers[lockerId].chambers[chamberNumber].SetCooldown();
                string accessToken = singleton.lockers[lockerId].chambers[chamberNumber].accessToken;
                var itemById = __instance._inv.GetItemByID(__instance._inv.curItem);
                Environment.OnUseLocker(player, singleton.lockers[lockerId], accessToken, string.IsNullOrEmpty(accessToken) || (itemById != null && itemById.permissions.Contains(accessToken)) || __instance._sr.BypassMode, out accessToken, out bool allow);
                if (allow)
                {
                    bool flag = (singleton.openLockers[lockerId] & 1 << chamberNumber) != 1 << chamberNumber;
                    singleton.ModifyOpen(lockerId, chamberNumber, flag);
                    singleton.RpcDoSound(lockerId, chamberNumber, flag);
                    bool anyOpen = true;
                    for (int i = 0; i < singleton.lockers[lockerId].chambers.Length; i++)
                    {
                        if ((singleton.openLockers[lockerId] & 1 << i) == 1 << i)
                        {
                            anyOpen = false;
                            break;
                        }
                    }
                    singleton.lockers[lockerId].LockPickups(!flag, chamberNumber, anyOpen);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        singleton.RpcChangeMaterial(lockerId, chamberNumber, false);
                    }
                }
                else
                {
                    singleton.RpcChangeMaterial(lockerId, chamberNumber, true);
                }
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdUseLocker), e);
                return true;
            }
        }
    }
}
