using System;
using Harmony;
using Vigilance.API;
using Vigilance.Extensions;
using UnityEngine;
using Mirror;
using PlayableScps;
using Vigilance.Enums;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp049), nameof(Scp049.BodyCmd_ByteAndGameObject))]
    public static class Scp049_BodyCmd_ByteAndGameObject
    {
        public static bool Prefix(Scp049 __instance, byte num, GameObject go)
        {
            try
            {
                Player myPlayer = Server.PlayerList.GetPlayer(__instance.Hub);
                Player target = Server.PlayerList.GetPlayer(go);
                if (myPlayer == null || target == null)
                    return true;
                if (num == 0)
                {
                    if (!__instance._interactRateLimit.CanExecute(true))
                        return false;
                    if (go == null)
                        return false;
                    if (Vector3.Distance(target.Position, myPlayer.Position) >= Scp049.AttackDistance * 1.25f)
                        return false;
                    myPlayer.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(4949f, myPlayer.Nick + " (" + myPlayer.UserId + ")", DamageTypes.Scp049, myPlayer.PlayerId), target.GameObject, false);
                    myPlayer.Hub.scpsController.RpcTransmit_Byte(0);
                    return false;
                }
                else
                {
                    if (num != 1)
                    {
                        if (num == 2)
                        {
                            if (!__instance._interactRateLimit.CanExecute(true))
                                return false;
                            if (go == null)
                                return false;
                            Ragdoll component = go.GetComponent<Ragdoll>();
                            if (component == null)
                                return false;
                            Player own = null;

                            foreach (Player player in Server.PlayerList.Players.Values)
                            {
                                if (player.PlayerId == component.owner.PlayerId)
                                {
                                    own = player;
                                    break;
                                }
                            }

                            if (own == null)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; no target found", MessageImportance.LessImportant, false);
                                return false;
                            }

                            if (!__instance._recallInProgressServer || own.Hub.gameObject != __instance._recallObjectServer || __instance._recallProgressServer < 0.85f)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; Debug code: ", MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#1 " + (__instance._recallInProgressServer ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallInProgressServer.ToString())), MessageImportance.LessImportant, true);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#2 " + ((own.Hub == __instance._recallObjectServer) ? "<color=green>PASSED</color>" : string.Concat(new object[]
                                {
                                    "<color=red>ERROR</color> - ",
                                        own.PlayerId,
                                    "-",
                                        (__instance._recallObjectServer == null) ? "null" : ReferenceHub.GetHub(__instance._recallObjectServer).queryProcessor.PlayerId.ToString()
                                })), MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#3 " + ((__instance._recallProgressServer >= 0.85f) ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallProgressServer)), MessageImportance.LessImportant, true);
                                return false;
                            }

                            if (own.Role != RoleType.Spectator)
                                return false;
                            if (!ConfigManager.CanScp049ReviveOther && component.owner.DeathCause.GetDamageInfo() != DamageType.Scp049)
                                return false;
                            Environment.OnRecall(myPlayer, component, true, out bool allow);
                            if (!allow)
                                return false;
                            GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' accepted", MessageImportance.LessImportant, false);
                            RoundSummary.changed_into_zombies++;
                            own.Hub.characterClassManager.SetClassID(RoleType.Scp0492);
                            own.Hub.playerStats.Health = (float)own.Hub.characterClassManager.Classes.Get(RoleType.Scp0492).maxHP;
                            if (component.CompareTag("Ragdoll"))
                                NetworkServer.Destroy(component.gameObject);
                            __instance._recallInProgressServer = false;
                            __instance._recallObjectServer = null;
                            __instance._recallProgressServer = 0f;
                        }
                        return false;
                    }
                    if (!__instance._interactRateLimit.CanExecute(true))
                        return false;
                    if (go == null)
                        return false;
                    Ragdoll component2 = go.GetComponent<Ragdoll>();
                    if (component2 == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object is not a dead body", MessageImportance.LessImportant, false);
                        return false;
                    }

                    if (!component2.allowRecall)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object can't be recalled", MessageImportance.LessImportant, false);
                        return false;
                    }

                    if (component2.CurrentTime > Scp049.ReviveEligibilityDuration)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object has decayed too far", MessageImportance.LessImportant, false);
                        return false;
                    }

                    Player owner = null;
                    foreach (Player pl in Server.PlayerList.Players.Values)
                    {
                        if (pl != null && pl.PlayerId == component2.owner.PlayerId)
                        {
                            owner = pl;
                            break;
                        }
                    }

                    if (owner == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; target not found", MessageImportance.LessImportant, false);
                        return false;
                    }

                    bool flag = false;
                    Rigidbody[] componentsInChildren = component2.GetComponentsInChildren<Rigidbody>();
                    for (int i = 0; i < componentsInChildren.Length; i++)
                    {
                        if (Vector3.Distance(componentsInChildren[i].transform.position, __instance.Hub.PlayerCameraReference.transform.position) <= Scp049.ReviveDistance * 1.3f)
                        {
                            flag = true;
                            owner.Hub.characterClassManager.NetworkDeathPosition = __instance.Hub.playerMovementSync.RealModelPosition;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP - 049 | Request 'start recalling' rejected; Distance was too great.", MessageImportance.LessImportant, false);
                        return false;
                    }

                    GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' accepted", MessageImportance.LessImportant, false);
                    __instance._recallObjectServer = owner.Hub.gameObject;
                    __instance._recallProgressServer = 0f;
                    __instance._recallInProgressServer = true;
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp049.BodyCmd_ByteAndGameObject), e);
                return true;
            }
        }
    }
}
