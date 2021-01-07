using System;
using Harmony;
using LightContainmentZoneDecontamination;
using UnityEngine;
using Mirror;
using Interactables.Interobjects.DoorUtils;
using Vigilance.Enums;
using Vigilance.Extensions;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    public static class DecontaminationController_FinishDecontamination
    {
        public static bool Prefix(DecontaminationController __instance)
        {
            try
            {
				Environment.OnDecontamination(true, out bool allow);
				if (!allow)
					return false;
				if (NetworkServer.active)
				{

					foreach (Lift lift in Lift.Instances)
					{
						if (lift != null)
						{
							lift.Lock();
						}
					}

					foreach (GameObject gameObject in __instance.LczGenerator.doors)
					{
						if (gameObject != null && gameObject.gameObject.activeSelf)
						{
							DoorVariant component = gameObject.GetComponent<DoorVariant>();
							if (component != null)
							{
								component.NetworkTargetState = false;
								component.ServerChangeLock(DoorLockReason.DecontLockdown, true);
							}
						}
					}

					DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.DeconFinish);
					if (DecontaminationController.AutoDeconBroadcastEnabled && !__instance._decontaminationBegun && __instance._broadcaster != null)
					{
						__instance._broadcaster.RpcAddElement(DecontaminationController.DeconBroadcastDeconMessage, DecontaminationController.DeconBroadcastDeconMessageTime, Broadcast.BroadcastFlags.Normal);
					}
					__instance._decontaminationBegun = true;
					DecontaminationController.KillPlayers();
				}
				return false;
			}
            catch (Exception e)
            {
                Log.Add(nameof(DecontaminationController.FinishDecontamination), e);
                return true;
            }
        }
    }
}
