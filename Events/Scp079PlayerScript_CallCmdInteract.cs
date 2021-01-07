using System;
using System.Collections.Generic;
using GameCore;
using Harmony;
using Vigilance.API;
using UnityEngine;
using NorthwoodLib.Pools;
using Interactables.Interobjects.DoorUtils;
using System.Linq;
using Vigilance.Extensions;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
    public static class Scp079PlayerScript_CallCmdInteract
    {
        public static bool Prefix(Scp079PlayerScript __instance, string command, GameObject target)
        {
            try
            {
				if (!__instance._interactRateLimit.CanExecute() || !__instance.iAm079)
					return false;
				GameCore.Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant);
				if (!command.Contains(":"))
					return false;
				string[] array = command.Split(':');
				__instance.RefreshCurrentRoom();
				if (!__instance.CheckInteractableLegitness(__instance.currentRoom, __instance.currentZone, target, allowNull: true))
					return false;
				DoorVariant component = null;
				bool flag = target != null && target.TryGetComponent<DoorVariant>(out component);
				List<string> list = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
				API.Door door = component?.GetDoor();
				Player player = Server.PlayerList.GetPlayer(__instance.gameObject);
				if (door == null || player == null)
					return true;
				switch (array[0])
				{
					case "DOOR":
						if (AlphaWarheadController.Host.inProgress)
							break;
						if (target == null)
							GameCore.Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant);
						else
						{
							if (!flag)
								break;
							if (component.TryGetComponent<DoorNametagExtension>(out var component5) && list != null && list.Count > 0 && list != null && list.Contains(component5.GetName))
							{
								GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant);
								break;
							}
							string text3 = component.RequiredPermissions.RequiredPermissions.ToString();
							float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (text3.Contains(",") ? text3.Split(',')[0] : text3), __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Door, target, manaFromLabel, true, out manaFromLabel, out bool allow);
							if (!allow)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								GameCore.Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant);
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								break;
							}
							bool targetState = component.TargetState;
							component.ServerInteract(ReferenceHub.GetHub(__instance.gameObject), 0);
							if (targetState != component.TargetState)
							{
								__instance.Mana -= manaFromLabel;
								__instance.AddInteractionToHistory(target, array[0], addMana: true);
								GameCore.Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant);
							}
							else
							{
								GameCore.Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant);
							}
						}
						break;
					case "DOORLOCK":
						if (AlphaWarheadController.Host.inProgress)
							break;
						if (target == null)
							GameCore.Console.AddDebugLog("SCP079", "The door lock command requires a target.", MessageImportance.LessImportant);
						else
						{
							if (component == null)
								break;
							if (component.TryGetComponent<DoorNametagExtension>(out var component4) && list != null && list.Count > 0 && list != null && list.Contains(component4.GetName))
							{
								GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant);
								break;
							}

							if (((DoorLockReason)component.ActiveLocks).HasFlag(DoorLockReason.Regular079))
							{
								if (__instance.lockedDoors.Contains(component.netId))
								{
									__instance.lockedDoors.Remove(component.netId);
									component.ServerChangeLock(DoorLockReason.Regular079, newState: false);
								}
								break;
							}

							float manaFromLabel = __instance.GetManaFromLabel("Door Lock Minimum", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Door, target, manaFromLabel, true, out manaFromLabel, out bool allow2);
							if (!allow2)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								break;
							}

							if (!__instance.lockedDoors.Contains(component.netId))
								__instance.lockedDoors.Add(component.netId);
							component.ServerChangeLock(DoorLockReason.Regular079, newState: true);
							__instance.AddInteractionToHistory(component.gameObject, array[0], addMana: true);
							__instance.Mana -= __instance.GetManaFromLabel("Door Lock Start", __instance.abilities);
						}
						break;
					case "SPEAKER":
						{
							string text2 = __instance.currentZone + "/" + __instance.currentRoom + "/Scp079Speaker";
							GameObject gameObject3 = GameObject.Find(text2);
							float manaFromLabel = __instance.GetManaFromLabel("Speaker Start", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Speaker, target, manaFromLabel, true, out manaFromLabel, out bool allow3);
							if (!allow3)
								return false;
							if (manaFromLabel * 1.5f > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
							}
							else if (gameObject3 != null)
							{
								__instance.Mana -= manaFromLabel;
								__instance.Speaker = text2;
								__instance.AddInteractionToHistory(gameObject3, array[0], addMana: true);
							}
							break;
						}
					case "STOPSPEAKER":
						Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Speaker, target, 0f, true, out float cost, out bool allow4);
						if (!allow4)
							return false;
						if (cost > 0f)
							__instance.Mana -= cost;
						__instance.Speaker = string.Empty;
						break;
					case "ELEVATORTELEPORT":
						{
							float manaFromLabel = __instance.GetManaFromLabel("Elevator Teleport", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.ElevatorTeleport, target, manaFromLabel, true, out manaFromLabel, out bool allow5);
							if (!allow5)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								break;
							}
							Camera079 camera = null;
							foreach (Scp079Interactable nearbyInteractable in __instance.nearbyInteractables)
							{
								if (nearbyInteractable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
								{
									camera = nearbyInteractable.optionalObject.GetComponent<Camera079>();
								}
							}
							if (camera != null)
							{
								__instance.RpcSwitchCamera(camera.cameraId, lookatRotation: false);
								__instance.Mana -= manaFromLabel;
								__instance.AddInteractionToHistory(target, array[0], addMana: true);
							}
							else
							{
								if (!ConsoleDebugMode.CheckImportance("SCP079", MessageImportance.LeastImportant, out var _))
								{
									break;
								}
								Scp079Interactable scp079Interactable2 = null;
								Dictionary<Scp079Interactable.InteractableType, byte> dictionary = new Dictionary<Scp079Interactable.InteractableType, byte>();
								foreach (Scp079Interactable nearbyInteractable2 in __instance.nearbyInteractables)
								{
									if (dictionary.ContainsKey(nearbyInteractable2.type))
									{
										dictionary[nearbyInteractable2.type]++;
									}
									else
									{
										dictionary[nearbyInteractable2.type] = 1;
									}
									if (nearbyInteractable2.type == Scp079Interactable.InteractableType.ElevatorTeleport)
									{
										scp079Interactable2 = nearbyInteractable2;
									}
								}
								string text;
								if (scp079Interactable2 == null)
								{
									text = "None of the " + __instance.nearbyInteractables.Count + " were an ElevatorTeleport, found: ";
									foreach (KeyValuePair<Scp079Interactable.InteractableType, byte> item in dictionary)
									{
										text = text + item.Value + "x" + item.Key.ToString().Substring(item.Key.ToString().Length - 4) + " ";
									}
								}
								else if (scp079Interactable2.optionalObject == null)
								{
									text = "Optional object is missing.";
								}
								else if (scp079Interactable2.optionalObject.GetComponent<Camera079>() == null)
								{
									string str = "";
									Transform transform = scp079Interactable2.optionalObject.transform;
									for (int j = 0; j < 5; j++)
									{
										str = transform.name + str;
										if (!(transform.parent != null))
										{
											break;
										}
										transform = transform.parent;
									}
									text = "Camera is missing at " + str;
								}
								else
								{
									text = "Unknown error";
								}
								GameCore.Console.AddDebugLog("SCP079", "Could not find the second elevator: " + text, MessageImportance.LeastImportant);
							}
							break;
						}
					case "ELEVATORUSE":
						{
							float manaFromLabel = __instance.GetManaFromLabel("Elevator Use", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.ElevatorUse, target, manaFromLabel, true, out manaFromLabel, out bool allow6);
							if (!allow6)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								break;
							}
							string b = string.Empty;
							if (array.Length > 1)
							{
								b = array[1];
							}
							Lift[] array2 = UnityEngine.Object.FindObjectsOfType<Lift>();
							foreach (Lift lift in array2)
							{
								if (lift.elevatorName == b && lift.UseLift())
								{
									__instance.Mana -= manaFromLabel;
									bool flag3 = false;
									Lift.Elevator[] elevators = lift.elevators;
									for (int k = 0; k < elevators.Length; k++)
									{
										Lift.Elevator elevator = elevators[k];
										__instance.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, array[0], !flag3);
										flag3 = true;
									}
								}
							}
							break;
						}
					case "TESLA":
						{
							float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Tesla, target, manaFromLabel, true, out manaFromLabel, out bool allow7);
							if (!allow7)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								break;
							}
							GameObject gameObject3 = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom + "/Gate");
							if (gameObject3 != null)
							{
								gameObject3.GetComponent<TeslaGate>().RpcInstantBurst();
								__instance.AddInteractionToHistory(gameObject3, array[0], addMana: true);
								__instance.Mana -= manaFromLabel;
							}
							break;
						}
					case "LOCKDOWN":
						{
							if (AlphaWarheadController.Host.inProgress)
							{
								GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, Warhead in progress.", MessageImportance.LessImportant);
								break;
							}
							float manaFromLabel = __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
							Environment.OnSCP079Interact(player, Scp079Interactable.InteractableType.Lockdown, target, manaFromLabel, true, out manaFromLabel, out bool allow8);
							if (!allow8)
								return false;
							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								GameCore.Console.AddDebugLog("SCP079", "Lockdown cannot commence, not enough mana.", MessageImportance.LessImportant);
								break;
							}
							GameCore.Console.AddDebugLog("SCP079", "Attempting lockdown...", MessageImportance.LeastImportant);
							GameObject gameObject = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom);
							if (gameObject != null)
							{
								List<Scp079Interactable> list2 = ListPool<Scp079Interactable>.Shared.Rent();
								try
								{
									Scp079Interactable[] allInteractables = Interface079.singleton.allInteractables;
									foreach (Scp079Interactable scp079Interactable in allInteractables)
									{
										if (!(scp079Interactable != null))
										{
											continue;
										}
										foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in scp079Interactable.currentZonesAndRooms)
										{
											if (currentZonesAndRoom.currentRoom == __instance.currentRoom && currentZonesAndRoom.currentZone == __instance.currentZone && scp079Interactable.transform.position.y - 100f < __instance.currentCamera.transform.position.y && !list2.Contains(scp079Interactable))
											{
												list2.Add(scp079Interactable);
											}
										}
									}
									GameCore.Console.AddDebugLog("SCP079", "Loaded all interactables", MessageImportance.LeastImportant);
								}
								catch
								{
									GameCore.Console.AddDebugLog("SCP079", "Failed to load interactables.", MessageImportance.LeastImportant);
								}
								GameObject gameObject2 = null;
								foreach (Scp079Interactable item2 in list2)
								{
									switch (item2.type)
									{
										case Scp079Interactable.InteractableType.Door:
											{
												IDamageableDoor damageableDoor;
												if (item2.TryGetComponent<DoorVariant>(out var component2) && (damageableDoor = component2 as IDamageableDoor) != null && damageableDoor.IsDestroyed)
												{
													GameCore.Console.AddDebugLog("SCP079", "Lockdown can't initiate, one of the doors were destroyed.", MessageImportance.LessImportant);
													return false;
												}
												break;
											}
										case Scp079Interactable.InteractableType.Lockdown:
											gameObject2 = item2.gameObject;
											break;
									}
								}
								if (list2.Count == 0 || gameObject2 == null || __instance._scheduledUnlocks.Count > 0)
								{
									GameCore.Console.AddDebugLog("SCP079", "This room can't be locked down.", MessageImportance.LessImportant);
									break;
								}
								HashSet<DoorVariant> hashSet = new HashSet<DoorVariant>();
								GameCore.Console.AddDebugLog("SCP079", "Looking for doors to lock...", MessageImportance.LeastImportant);
								foreach (Scp079Interactable item3 in list2)
								{
									if (!item3.TryGetComponent<DoorVariant>(out var component3))
									{
										continue;
									}
									bool flag2 = component3.ActiveLocks == 0;
									if (!flag2)
									{
										DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)component3.ActiveLocks);
										flag2 = mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.ScpOverride);
									}
									if (flag2)
									{
										if (component3.TargetState)
										{
											component3.NetworkTargetState = false;
										}
										component3.ServerChangeLock(DoorLockReason.Lockdown079, newState: true);
										hashSet.Add(component3);
									}
								}
								if (hashSet.Count > 0)
								{
									__instance._scheduledUnlocks.Add(Time.realtimeSinceStartup + 10f, hashSet);
									GameCore.Console.AddDebugLog("SCP079", "Locking " + hashSet.Count + " doors", MessageImportance.LeastImportant);
								}
								else
								{
									GameCore.Console.AddDebugLog("SCP079", "No doors to lock found, code " + list2.Where((Scp079Interactable x) => x.type == Scp079Interactable.InteractableType.Door).Count(), MessageImportance.LessImportant);
								}
								ListPool<Scp079Interactable>.Shared.Return(list2);
								FlickerableLightController[] componentsInChildren = gameObject.GetComponentsInChildren<FlickerableLightController>();
								foreach (FlickerableLightController flickerableLightController in componentsInChildren)
								{
									if (flickerableLightController != null)
									{
										flickerableLightController.ServerFlickerLights(8f);
									}
								}
								GameCore.Console.AddDebugLog("SCP079", "Lockdown initiated.", MessageImportance.LessImportant);
								__instance.AddInteractionToHistory(gameObject2, array[0], addMana: true);
								__instance.Mana -= __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);
							}
							else
							{
								GameCore.Console.AddDebugLog("SCP079", "Room couldn't be specified.", MessageImportance.Normal);
							}
							break;
						}
				}
				return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp079PlayerScript.CallCmdInteract), e);
                return true;
            }
        }
    }
}
