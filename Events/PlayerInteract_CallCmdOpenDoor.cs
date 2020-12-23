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
				if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract))
					return false;
				if (doorId == null)
					return false;
				if (__instance._ccm.CurClass == RoleType.None || __instance._ccm.CurClass == RoleType.Spectator)
					return false;
				Door door;
				if (!doorId.TryGetComponent(out door))
					return false;
				if ((door.Buttons.Count == 0) ? (!__instance.ChckDis(doorId.transform.position)) : door.Buttons.All((Door.DoorButton item) => !__instance.ChckDis(item.button.transform.position)))
					return false;
				Player myPlayer = Server.PlayerList.GetPlayer(__instance._hub);
				if (myPlayer == null)
					return true;
				Environment.OnDoorInteract(true, door, myPlayer, out bool allow);
				if (!allow)
					return false;
				if (myPlayer.PlayerLock)
					return false;
				__instance.OnInteract();
				if (__instance._sr.BypassMode)
				{
					door.ChangeState(true);
					return false;
				}

				if (door.PermissionLevels.HasPermission(Door.AccessRequirements.Checkpoints) && __instance._ccm.CurRole.team == Team.SCP)
				{
					door.ChangeState(false);
					return false;
				}

				try
				{
					if (door.PermissionLevels == (Door.AccessRequirements)0)
					{
						if (!door.locked)
						{
							door.ChangeState(false);
						}
					}
					else if (!door.RequireAllPermissions)
					{
						foreach (string key in __instance._inv.GetItemByID(__instance._inv.curItem).permissions)
						{
							Door.AccessRequirements flag;
							if (Door.backwardsCompatPermissions.TryGetValue(key, out flag) && door.PermissionLevels.HasPermission(flag))
							{
								if (!door.locked)
								{
									door.ChangeState(false);
								}
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
				catch
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
