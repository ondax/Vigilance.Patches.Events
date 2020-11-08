using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using Harmony;
using LightContainmentZoneDecontamination;
using Respawning.NamingRules;
using Vigilance.API;
using Vigilance.Extensions;
using Grenades;
using UnityEngine;
using CustomPlayerEffects;
using NorthwoodLib.Pools;
using Mirror;
using System.Text;
using RemoteAdmin;
using PlayableScps;
using PlayableScps.Interfaces;
using Respawning;
using MEC;
using Console = GameCore.Console;
using Scp914;
using Cryptography;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using Vigilance.Events;
using Searching;
using Vigilance.Enums;
using PlayableScps.Messages;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
    public static class Handcuffs_CallCmdFreeTeammate
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                    return false;
                if (__instance.MyReferenceHub.characterClassManager.CurRole.team == Team.SCP)
                    return false;
                Player myPlayer = Server.PlayerList.GetPlayer(__instance.gameObject);
                Player myTarget = Server.PlayerList.GetPlayer(target);
                if (myPlayer == null || myTarget == null)
                    return true;
                Environment.OnUncuff(myTarget, myPlayer, true, out bool allow);
                if (allow)
                    myTarget.CufferId = -1;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Handcuffs.CallCmdFreeTeammate), e);
                return true;
            }
        }
    }
}
