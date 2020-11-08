using System;
using Harmony;
using Vigilance.API;
using UnityEngine;
using Mirror;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), new Type[] { typeof(GameObject) })]
    public static class AlphaWarheadController_CancelDetonation
    {
        public static bool Prefix(AlphaWarheadController __instance, GameObject disabler)
        {
            try
            {
                if (!__instance.inProgress || !(__instance.timeToDetonation > 10f) || __instance._isLocked)
                    return false;
                Player player = Server.PlayerList.GetPlayer(disabler);
                if (player == null)
                    player = new Player(ReferenceHub.HostHub);
                Environment.OnWarheadCancel(player, __instance.timeToDetonation, true, out __instance.timeToDetonation, out bool allow);
                if (!allow)
                    return false;
                if (__instance.timeToDetonation <= 15f && disabler != null)
                    player.Achieve(Enums.Achievement.ThatWasClose);
                for (sbyte b = 0; b < __instance.scenarios_resume.Length; b = (sbyte)(b + 1))
                    if (__instance.scenarios_resume[b].SumTime() > __instance.timeToDetonation && __instance.scenarios_resume[b].SumTime() < __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime())
                        __instance.NetworksyncResumeScenario = b;
                __instance.NetworktimeToDetonation = ((AlphaWarheadController._resumeScenario < 0) ? __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime() : __instance.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime()) + __instance.cooldown;
                __instance.NetworkinProgress = false;
                Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
                foreach (Door obj in array)
                {
                    obj.warheadlock = false;
                    obj.CheckpointLockOpenWarhead = false;
                    obj.UpdateLock();
                }
                if (NetworkServer.active)
                    __instance._autoDetonate = false;
                ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(AlphaWarheadController.CancelDetonation), e);
                return true;
            }
        }
    }
}
