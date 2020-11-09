using System;
using System.Collections.Generic;
using GameCore;
using Harmony;
using LightContainmentZoneDecontamination;
using Vigilance.API;
using Vigilance.Enums;
using UnityEngine;
using Mirror;
using CustomPlayerEffects;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static class PocketDimensionTeleport_OnTriggerEnter
    {
        public static bool Prefix(PocketDimensionTeleport __instance, Collider other)
        {
            try
            {
                if (other == null)
                    return false;
                NetworkIdentity component = other.GetComponent<NetworkIdentity>();
                if (component == null)
                    return false;
                if (component != null)
                {
                    ReferenceHub hub = component.GetComponent<ReferenceHub>();
                    if (hub == null)
                        return true;
                    Player player = Server.PlayerList.GetPlayer(hub);
                    if (player == null)
                        return true;
                    if (__instance.type == PocketDimensionTeleport.PDTeleportType.Killer || BlastDoor.OneDoor.isClosed)
                    {
                        hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), hub.gameObject, true);
                        return false;
                    }
                    else if (__instance.type == PocketDimensionTeleport.PDTeleportType.Exit)
                    {
                        __instance.tpPositions.Clear();
                        bool flag = false;
                        DecontaminationController.DecontaminationPhase[] decontaminationPhases = Map.Decontamination.Controller.DecontaminationPhases;
                        if (DecontaminationController.GetServerTime > decontaminationPhases[decontaminationPhases.Length - 2].TimeTrigger)
                            flag = true;
                        List<string> stringList = ConfigFile.ServerConfig.GetStringList(flag ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
                        if (stringList.Count > 0)
                        {
                            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("RoomID"))
                            {
                                Rid component2 = gameObject.GetComponent<Rid>();
                                if (component2 != null && stringList.Contains(component2.id, StringComparison.Ordinal))
                                {
                                    if (gameObject != null && gameObject.transform != null)
                                        __instance.tpPositions.Add(gameObject.transform.position);
                                }
                            }

                            try
                            {
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
                            catch (Exception)
                            {

                            }
                        }

                        if (__instance.tpPositions == null || __instance.tpPositions.Count == 0)
                        {
                            foreach (GameObject gameObject2 in GameObject.FindGameObjectsWithTag("PD_EXIT"))
                            {
                                if (gameObject2 != null && gameObject2.transform != null)
                                    __instance.tpPositions.Add(gameObject2.transform.position);
                            }
                        }

                        Vector3 pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
                        pos.y += 1.5f;
                        Environment.OnPocketEscape(player, pos, true, out Vector3 newPos, out bool allow);
                        player.DisableEffect<Corroding>();
                        player.Hub.playerMovementSync.AddSafeTime(5f);
                        player.Teleport(pos);
                        player.Achieve(Achievement.LarryIsYourFriend);
                    }
                    if (PocketDimensionTeleport.RefreshExit)
                        ImageGenerator.pocketDimensionGenerator.GenerateRandom();
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
