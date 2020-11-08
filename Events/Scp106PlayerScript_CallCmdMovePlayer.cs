using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using CustomPlayerEffects;
using RemoteAdmin;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    public static class Scp106PlayerScript_CallCmdMovePlayer
    {
        public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;
                if (ply == null)
                    return false;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null)
                    return true;
                if (player.GodMode || player.IsAnySCP)
                    return false;
                if (!ServerTime.CheckSynchronization(t) || !__instance.iAm106 || Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, ply.transform.position) >= 3f || player.IsAnySCP)
                    return false;

                __instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
                __instance.TargetHitMarker(__instance.connectionToClient);

                if (Scp106PlayerScript._blastDoor.isClosed)
                {
                    __instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
                    __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(500f, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false);
                }
                else
                {
                    Environment.OnPocketEnter(player, true, ConfigManager.Scp106PocketEnterDamage, true, out bool hurt, out float damage, out bool allow);
                    if (!allow)
                        return false;
                    if (hurt)
                        __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false);
                    player.Hub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);
                    foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
                    {
                        Scp079Interactable.ZoneAndRoom otherRoom = player.Hub.scp079PlayerScript.GetOtherRoom();
                        Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
                        {
                            Scp079Interactable.InteractableType.Door,
                            Scp079Interactable.InteractableType.Light,
                            Scp079Interactable.InteractableType.Lockdown,
                            Scp079Interactable.InteractableType.Tesla,
                            Scp079Interactable.InteractableType.ElevatorUse
                        };

                        foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
                        {
                            foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
                            {
                                if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
                                {
                                    scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, player.Role);
                                }
                            }
                        }
                    }
                }

                PlayerEffectsController playerEffectsController = player.Hub.playerEffectsController;
                playerEffectsController.GetEffect<Corroding>().IsInPd = true;
                playerEffectsController.EnableEffect<Corroding>(0f, false);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp106PlayerScript.CallCmdMovePlayer), e);
                return true;
            }
        }
    }
}
