using System;
using Harmony;
using Vigilance.API;
using Vigilance.Extensions;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static class DoorVariant_ServerInteract
    {
        public static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
        {
			try
			{
				API.Door door = __instance.GetDoor();
				Player player = Server.PlayerList.GetPlayer(ply);
				if (door == null || player == null)
					return true;
				if (__instance.ActiveLocks > 0)
				{
					DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);
					if ((!mode.HasFlagFast(DoorLockMode.CanClose) || !mode.HasFlagFast(DoorLockMode.CanOpen)) && (!mode.HasFlagFast(DoorLockMode.ScpOverride) || ply.characterClassManager.CurRole.team != Team.SCP) && (mode == DoorLockMode.FullLock || (__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanClose)) || (!__instance.TargetState && !mode.HasFlagFast(DoorLockMode.CanOpen))))
					{
						__instance.LockBypassDenied(ply, colliderId);
						return false;
					}
				}

				if (ConfigManager.RemoteCard)
                {
					if (door.IsAllowed(player, true, colliderId) || ply.characterClassManager.CurClass == RoleType.Scp079)
                    {
						Environment.OnDoorInteract(true, door, player, out bool allow);
						if (!allow)
							return false;
						door.ChangeState();
						__instance._triggerPlayer = ply;
						return false;
                    }
                }

				if (__instance.AllowInteracting(ply, colliderId))
				{
					if (ply.characterClassManager.CurClass == RoleType.Scp079 || door.IsAllowed(player, false, colliderId))
					{
						Environment.OnDoorInteract(true, door, player, out bool allow);
						if (!allow)
							return false;
						__instance.NetworkTargetState = !__instance.TargetState;
						__instance._triggerPlayer = ply;
						return false;
					}

					__instance.PermissionsDenied(ply, colliderId);
					DoorEvents.TriggerAction(__instance, DoorAction.AccessDenied, ply);
				}

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
