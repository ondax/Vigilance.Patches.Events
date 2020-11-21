using System;
using System.Collections.Generic;
using GameCore;
using Harmony;
using Vigilance.API;
using UnityEngine;
using NorthwoodLib.Pools;
using Console = GameCore.Console;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
    public static class Scp079PlayerScript_CallCmdInteract
    {
        public static bool Prefix(Scp079PlayerScript __instance, string command, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute())
                    return false;
                if (!__instance.iAm079)
                    return false;
                if (!command.Contains(":"))
                    return false;
                Player myPlayer = Server.PlayerList.GetPlayer(__instance.gameObject);
                if (myPlayer == null)
                    return true;
                string[] array = command.Split(':');
                __instance.RefreshCurrentRoom();
                if (!__instance.CheckInteractableLegitness(__instance.currentRoom, __instance.currentZone, target, true))
                    return false;
                List<string> list = ListPool<string>.Shared.Rent();
                ConfigFile.ServerConfig.GetStringCollection("scp079_door_blacklist", list);
                bool result = true;
                switch (array[0])
                {
                    case "TESLA":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);
                            Environment.OnSCP079Interact(myPlayer, Scp079Interactable.InteractableType.Tesla, target, manaFromLabel, true, out manaFromLabel, out bool allow);
                            if (!allow)
                                return false;
                            if (manaFromLabel > __instance.curMana)
                            {
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                result = false;
                                break;
                            }

                            GameObject gameObject = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom + "/Gate");
                            if (gameObject != null)
                            {
                                gameObject.GetComponent<TeslaGate>().RpcInstantBurst();
                                __instance.AddInteractionToHistory(gameObject, array[0], true);
                                __instance.Mana -= manaFromLabel;
                            }

                            result = false;
                            break;
                        }
                    case "DOOR":
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                result = false;
                                break;
                            }

                            if (target == null)
                            {
                                Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant);
                                result = false;
                                break;
                            }

                            Door component = target.GetComponent<Door>();
                            if (component == null)
                            {
                                result = false;
                                break;
                            }

                            if (list != null && list.Count > 0 && list != null && list.Contains(component.DoorName))
                            {
                                Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant);
                                result = false;
                                break;
                            }

                            float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (string.IsNullOrEmpty(component.permissionLevel) ? "DEFAULT" : component.permissionLevel), __instance.abilities);
                            Environment.OnSCP079Interact(myPlayer, Scp079Interactable.InteractableType.Door, target, manaFromLabel, true, out manaFromLabel, out bool allow);
                            if (!allow)
                                return false;
                            if (manaFromLabel > __instance.curMana)
                            {
                                Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant);
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                result = false;
                                break;
                            }

                            if (component != null && component.ChangeState079())
                            {
                                __instance.Mana -= manaFromLabel;
                                __instance.AddInteractionToHistory(target, array[0], true);
                                Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant);
                                result = true;
                                break;
                            }

                            Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant);
                            result = false;
                            break;
                        }
                    default:
                        result = true;
                        break;
                }

                ListPool<string>.Shared.Return(list);
                return result;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp079PlayerScript.CallCmdInteract), e);
                return true;
            }
        }
    }
}
