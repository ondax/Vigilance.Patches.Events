using System;
using Harmony;
using UnityEngine;
using LightContainmentZoneDecontamination;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    public static class AlphaWarheadController_Detonate
    {
        public static bool Prefix(AlphaWarheadController __instance)
        {
            try
            {
				if (AlphaWarheadController.AutoWarheadBroadcastEnabled && !__instance.detonated && __instance._broadcaster != null)
					__instance._broadcaster.RpcAddElement(AlphaWarheadController.WarheadExplodedBroadcastMessage, AlphaWarheadController.WarheadExplodedBroadcastMessageTime, Broadcast.BroadcastFlags.Normal);
				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent, false);
				if (!DecontaminationController.Singleton.disableDecontamination)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, "LCZ decontamination has been disabled by detonation of the Alpha Warhead.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
					DecontaminationController.Singleton.disableDecontamination = true;
				}
				__instance.detonated = true;
				__instance.RpcShake(true);
				GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
				foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
				{
					scp079PlayerScript.lockedDoors.Clear();
				}
				foreach (GameObject gameObject in PlayerManager.players)
				{
					foreach (GameObject gameObject2 in array)
					{
						if (gameObject.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject2.transform.position, gameObject.transform.position) < 3.5f))
						{
							__instance.warheadKills++;
						}
					}
				}
				DoorNametagExtension doorNametagExtension;
				if (DoorNametagExtension.NamedDoors.TryGetValue("SURFACE_NUKE", out doorNametagExtension))
				{
					doorNametagExtension.TargetDoor.ServerChangeLock(DoorLockReason.SpecialDoorFeature, true);
					doorNametagExtension.TargetDoor.NetworkTargetState = true;
				}
				Environment.OnWarheadDetonate();
				return true;
            }
            catch (Exception e)
            {
                Log.Add(nameof(AlphaWarheadController.Detonate), e);
                return true;
            }
        }
    }
}
