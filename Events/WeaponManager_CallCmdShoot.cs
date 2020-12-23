using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Vigilance.Extensions;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdShoot))]
    public static class WeaponManager_CallCmdShoot
    {
        public static bool Prefix(WeaponManager __instance, GameObject target, HitBoxType hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
        {
            try
            {
				Player player = Server.PlayerList.GetPlayer(__instance._hub);
				if (player == null)
					return true;
				if (!__instance._iawRateLimit.CanExecute(true))
					return false;
				int itemIndex = __instance._hub.inventory.GetItemIndex();
				if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count)
					return false;
				if (__instance.curWeapon < 0 || ((__instance._reloadCooldown > 0f || __instance._fireCooldown > 0f) && !__instance.isLocalPlayer))
					return false;
				if (__instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID)
					return false;
				if (__instance._hub.inventory.items[itemIndex].durability <= 0f)
					return false;

				if (Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, sourcePos) > 5.5f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.6 (difference between real source position and provided source position is too big)", "gray");
					return false;
				}

				if (sourcePos.y - __instance._hub.playerMovementSync.LastSafePosition.y > 1.78f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.7 (Y axis difference between last safe position and provided source position is too big)", "gray");
					return false;
				}

				if (Math.Abs(sourcePos.y - __instance._hub.playerMovementSync.RealModelPosition.y) > 2.7f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.8 (|Y| axis difference between real position and provided source position is too big)", "gray");
					return false;
				}

				Environment.OnShoot(player, target, player.ItemInHand.GetWeaponType(), true, dir, sourcePos, targetPos, hitboxType, out bool allow);
				if (!allow)
					return false;
				__instance._hub.inventory.items.ModifyDuration(itemIndex, __instance._hub.inventory.items[itemIndex].durability - 1f);
				__instance.scp268.ServerDisable();
				__instance._fireCooldown = 1f / (__instance.weapons[__instance.curWeapon].shotsPerSecond * __instance.weapons[__instance.curWeapon].allEffects.firerateMultiplier) * 0.9f;
				float num = __instance.weapons[__instance.curWeapon].allEffects.audioSourceRangeScale;
				num = num * num * 70f;
				__instance.GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp(num, 5f, 100f));
				bool flag = target != null;
				RaycastHit raycastHit2;
				if (targetPos == Vector3.zero)
				{
					RaycastHit raycastHit;
					if (Physics.Raycast(sourcePos, dir, out raycastHit, 500f, __instance.raycastMask))
					{
						HitboxIdentity component = raycastHit.collider.GetComponent<HitboxIdentity>();
						if (component != null)
						{
							WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
							if (componentInParent != null)
							{
								flag = false;
								target = componentInParent.gameObject;
								hitboxType = component.id;
								targetPos = componentInParent.transform.position;
							}
						}
					}
				}
				else if (Physics.Linecast(sourcePos, targetPos, out raycastHit2, __instance.raycastMask))
				{
					HitboxIdentity component2 = raycastHit2.collider.GetComponent<HitboxIdentity>();
					if (component2 != null)
					{
						WeaponManager componentInParent2 = component2.GetComponentInParent<WeaponManager>();
						if (componentInParent2 != null)
						{
							if (componentInParent2.gameObject == target)
							{
								flag = false;
							}
							else if (componentInParent2.scp268.Enabled)
							{
								flag = false;
								target = componentInParent2.gameObject;
								hitboxType = component2.id;
								targetPos = componentInParent2.transform.position;
							}
						}
					}
				}
				ReferenceHub referenceHub = null;
				if (target != null)
					referenceHub = ReferenceHub.GetHub(target);
				if (referenceHub != null && __instance.GetShootPermission(referenceHub.characterClassManager, false))
				{
					if (Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - referenceHub.playerMovementSync.RealModelPosition.y) > 35f)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.1 (too big Y-axis difference between source and target)", "gray");
						return false;
					}

					if (Vector3.Distance(referenceHub.playerMovementSync.RealModelPosition, targetPos) > 5f)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.2 (difference between real target position and provided target position is too big)", "gray");
						return false;
					}

					if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, sourcePos, __instance.raycastServerMask))
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.3 (collision between source positions detected)", "gray");
						return false;
					}

					if (flag && Physics.Linecast(sourcePos, targetPos, __instance.raycastServerMask))
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.4 (collision on shot line detected)", "gray");
						return false;
					}

					if (referenceHub.gameObject == __instance.gameObject)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.5 (target is itself)", "gray");
						return false;
					}

					Vector3 vector = referenceHub.playerMovementSync.RealModelPosition - __instance._hub.playerMovementSync.RealModelPosition;
					if (Math.Abs(vector.y) < 10f && vector.sqrMagnitude > 0.25f)
					{
						float num2 = Math.Abs(Misc.AngleIgnoreY(vector, __instance.transform.forward));
						if (num2 > 45f)
						{
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.12 (too big angle)", "gray");
							return false;
						}

						if (__instance._lastAngleReset > 0f && num2 > 25f && Math.Abs(Misc.AngleIgnoreY(vector, __instance._lastAngle)) > 60f)
						{
							__instance._lastAngle = vector;
							__instance._lastAngleReset = 0.4f;
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.13 (too big angle v2)", "gray");
							return false;
						}

						__instance._lastAngle = vector;
						__instance._lastAngleReset = 0.4f;
					}

					if (__instance._lastRotationReset > 0f && (__instance._hub.playerMovementSync.Rotations.x < 68f || __instance._hub.playerMovementSync.Rotations.x > 295f))
					{
						float num3 = __instance._hub.playerMovementSync.Rotations.x - __instance._lastRotation;
						if (num3 >= 0f && num3 <= 0.0005f)
						{
							__instance._lastRotation = __instance._hub.playerMovementSync.Rotations.x;
							__instance._lastRotationReset = 0.35f;
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.9 (no recoil)", "gray");
							return false;
						}
					}

					__instance._lastRotation = __instance._hub.playerMovementSync.Rotations.x;
					__instance._lastRotationReset = 0.35f;
					float num4 = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
					float num5 = __instance.weapons[__instance.curWeapon].damageOverDistance.Evaluate(num4);
					RoleType curClass = referenceHub.characterClassManager.CurClass;

					if (curClass != RoleType.Scp173)
					{
						switch (curClass)
						{
							case RoleType.Scp106:
								num5 /= 10f;
								goto IL_7D3;
							case RoleType.NtfScientist:
							case RoleType.Scientist:
							case RoleType.ChaosInsurgency:
								break;
							case RoleType.Scp049:
							case RoleType.Scp079:
							case RoleType.Scp096:
								goto IL_7D3;
							default:
								if (curClass - RoleType.Scp93953 <= 1)
								{
									goto IL_7D3;
								}
								break;
						}

						if (hitboxType > HitBoxType.ARM)
						{
							if (hitboxType == HitBoxType.HEAD)
							{
								num5 *= 4f;
								float num6 = 1f / (__instance.weapons[__instance.curWeapon].shotsPerSecond * __instance.weapons[__instance.curWeapon].allEffects.firerateMultiplier);
								__instance._headshotsL += 1U;
								__instance._headshotsS += 1U;
								__instance._headshotsResetS = num6 * 1.86f;

								__instance._headshotsResetL = num6 * 2.9f;
								if (__instance._headshotsS >= 3U)
								{
									__instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window A\n(debug code: W.10)", "W.10");
									return false;
								}

								if (__instance._headshotsL >= 4U)
								{
									__instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window B\n(debug code: W.11)", "W.11");
									return false;
								}
							}
						}
						else
						{
							num5 /= 2f;
						}
					}
				IL_7D3:
					Environment.OnLateShoot(player, target, player.ItemInHand.GetWeaponType(), true, out bool allow2);
					if (!allow2)
						return false;
					num5 *= __instance.weapons[__instance.curWeapon].allEffects.damageMultiplier;
					num5 *= __instance.overallDamagerFactor;
					__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(num5, __instance._hub.LoggedNameFromRefHub(), DamageTypes.FromWeaponId(__instance.curWeapon), __instance._hub.queryProcessor.PlayerId), referenceHub.gameObject, false, true);
					__instance.RpcConfirmShot(true, __instance.curWeapon);
					__instance.PlaceDecal(true, new Ray(__instance.camera.position, dir), (int)referenceHub.characterClassManager.CurClass, num4);
					return false;
				}
				else
				{
					if (target != null && hitboxType == HitBoxType.WINDOW && target.GetComponent<BreakableWindow>() != null)
					{
						float time = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
						float damage = __instance.weapons[__instance.curWeapon].damageOverDistance.Evaluate(time);
						target.GetComponent<BreakableWindow>().ServerDamageWindow(damage);
						__instance.RpcConfirmShot(true, __instance.curWeapon);
						return false;
					}
					__instance.PlaceDecal(false, new Ray(__instance.camera.position, dir), __instance.curWeapon, 0f);
					__instance.RpcConfirmShot(false, __instance.curWeapon);
					return false;
				}
            }
            catch (Exception e)
            {
                Log.Add(nameof(WeaponManager.CallCmdShoot), e);
                return true;
            }
        }
    }
}
