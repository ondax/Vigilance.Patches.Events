using System;
using Harmony;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    public static class AlphaWarheadController_Detonate
    {
        public static bool Prefix(AlphaWarheadController __instance)
        {
            try
            {
                Environment.OnWarheadDetonate();
                __instance.detonated = true;
                __instance.RpcShake(true);
                GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
                foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
                    instance.lockedDoors.Clear();

                foreach (ReferenceHub player in ReferenceHub.GetAllHubs().Values)
                {
                    foreach (GameObject gameObject in array)
                    {
                        if (player.playerStats.Explode(Vector3.Distance(gameObject.transform.position, player.playerMovementSync.RealModelPosition) < 3.5f))
                        {
                            __instance.warheadKills++;
                        }
                    }
                }

                Door[] array2 = UnityEngine.Object.FindObjectsOfType<Door>();
                foreach (Door door in array2)
                    door.OpenWarhead(true, door.blockAfterDetonation, true);
                ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(AlphaWarheadController.Detonate), e);
                return true;
            }
        }
    }
}
