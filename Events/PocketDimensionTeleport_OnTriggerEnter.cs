using System;
using System.Collections.Generic;
using GameCore;
using LightContainmentZoneDecontamination;
using Mirror;
using UnityEngine;
using Harmony;
using Vigilance.API;
using MapGeneration;

namespace Vigilance.Patches.Events
{
	[HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
	public static class PocketDimensionTeleport_OnTriggerEnter
	{
		public static bool Prefix(PocketDimensionTeleport __instance, Collider other)
		{
			try
			{
				if (!NetworkServer.active)
					return false;
				NetworkIdentity component = other.GetComponent<NetworkIdentity>();
				if (component != null)
				{
					if (__instance.type == PocketDimensionTeleport.PDTeleportType.Killer || BlastDoor.OneDoor.isClosed)
					{
						component.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), other.gameObject, true);
					}
					else if (__instance.type == PocketDimensionTeleport.PDTeleportType.Exit)
					{
						Environment.OnPocketEscape(Server.PlayerList.GetPlayer(component.gameObject), Vector3.zero, true, out Vector3 escape, out bool allow);
						__instance.tpPositions.Clear();
						bool flag = false;
						DecontaminationController.DecontaminationPhase[] decontaminationPhases = DecontaminationController.Singleton.DecontaminationPhases;
						if (DecontaminationController.GetServerTime > (double)decontaminationPhases[decontaminationPhases.Length - 2].TimeTrigger)
						{
							flag = true;
						}
						List<string> stringList = ConfigFile.ServerConfig.GetStringList(flag ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
						if (stringList.Count > 0)
						{
							foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("RoomID"))
							{
								Rid component2 = gameObject.GetComponent<Rid>();
								if (component2 != null && stringList.Contains(component2.id, StringComparison.Ordinal))
								{
									__instance.tpPositions.Add(gameObject.transform.position);
								}
							}
							if (stringList.Contains("PORTAL"))
							{
								foreach (Scp106PlayerScript scp106PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp106PlayerScript>())
								{
									if (scp106PlayerScript.portalPosition != Vector3.zero)
									{
										__instance.tpPositions.Add(scp106PlayerScript.portalPosition);
									}
								}
							}
						}
						if (__instance.tpPositions == null || __instance.tpPositions.Count == 0)
						{
							foreach (GameObject gameObject2 in GameObject.FindGameObjectsWithTag("PD_EXIT"))
							{
								__instance.tpPositions.Add(gameObject2.transform.position);
							}
						}
						Vector3 pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
						pos.y += 2f;
						PlayerMovementSync component3 = other.GetComponent<PlayerMovementSync>();
						component3.AddSafeTime(2f);
						component3.OverridePosition(pos, 0f, false);
						__instance.RemoveCorrosionEffect(other.gameObject);
						PlayerManager.localPlayer.GetComponent<PlayerStats>().TargetAchieve(component.connectionToClient, "larryisyourfriend");
						return false;
					}
					if (PocketDimensionTeleport.RefreshExit)
					{
						ImageGenerator.pocketDimensionGenerator.GenerateRandom();
					}
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(PocketDimensionTeleport.OnTriggerEnter), e);
				return true;
			}
		}
	}
}
