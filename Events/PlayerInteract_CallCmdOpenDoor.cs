using System;
using System.Linq;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor))]
    public static class PlayerInteract_CallCmdOpenDoor
    {
        public static bool Prefix(PlayerInteract __instance, GameObject doorId)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute() || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract) || doorId == null || __instance._ccm.CurClass == RoleType.None || __instance._ccm.CurClass == RoleType.Spectator || !doorId.TryGetComponent(out Door component) || ((component.Buttons.Count == 0) ? (!__instance.ChckDis(doorId.transform.position)) : component.Buttons.All((Door.DoorButton item) => !__instance.ChckDis(item.button.transform.position))))
                    return false;
                __instance.OnInteract();
                Player player = Server.PlayerList.GetPlayer(__instance._hub);
                if (player == null)
                    return true;
                Environment.OnDoorInteract(true, component, player, out bool allow);
                if (!allow || player.PlayerLock)
                    return false;

                if (player.BypassMode)
                {
                    component.ChangeState(true);
                    return false;
                }

                if (component.PermissionLevels.HasPermission(Door.AccessRequirements.Checkpoints) && __instance._ccm.CurRole.team == Team.SCP)
                {
                    component.ChangeState();
                    return false;
                }

                try
                {
                    if (component.PermissionLevels == (Door.AccessRequirements)0)
                    {
                        if (!component.locked)
                        {
                            component.ChangeState();
                        }
                    }
                    else if (!component.RequireAllPermissions)
                    {
                        string[] permissions = __instance._inv.GetItemByID(__instance._inv.curItem).permissions;
                        foreach (string key in permissions)
                        {
                            if (Door.backwardsCompatPermissions.TryGetValue(key, out Door.AccessRequirements value) && component.PermissionLevels.HasPermission(value))
                            {
                                if (!component.locked)
                                    component.ChangeState();
                                return false;
                            }
                        }
                        __instance.RpcDenied(doorId);
                    }
                    else
                    {
                        __instance.RpcDenied(doorId);
                    }
                }
                catch (Exception)
                {
                    __instance.RpcDenied(doorId);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdOpenDoor), e);
                return true;
            }
        }
    }
}
