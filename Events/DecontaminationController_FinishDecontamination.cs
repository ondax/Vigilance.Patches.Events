using System;
using Harmony;
using LightContainmentZoneDecontamination;
using UnityEngine;
using Vigilance.API;
using Mirror;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    public static class DecontaminationController_FinishDecontamination
    {
        public static bool Prefix(DecontaminationController __instance)
        {
            try
            {
                if (NetworkServer.active)
                {
                    Environment.OnDecontamination(true, out bool allow);
                    if (!allow)
                        return false;
                    foreach (Lift lift in Lift.Instances) 
                        lift?.Lock();

                    foreach (GameObject gameObject in __instance.LczGenerator.doors)
                    {
                        if (gameObject != null && gameObject.gameObject.activeSelf)
                            gameObject.GetComponent<Door>()?.CloseDecontamination();
                    }

                    foreach (DecontaminationEvacuationDoor decontaminationEvacuationDoor in DecontaminationEvacuationDoor.Instances)
                    {
                        if (decontaminationEvacuationDoor != null && decontaminationEvacuationDoor.transform != null)
                            decontaminationEvacuationDoor.Close();
                    }

                    if (DecontaminationController.AutoDeconBroadcastEnabled && !__instance._decontaminationBegun)
                        Map.Broadcast(DecontaminationController.DeconBroadcastDeconMessage, (int)DecontaminationController.DeconBroadcastDeconMessageTime);

                    __instance._decontaminationBegun = true;
                    DecontaminationController.KillPlayers();
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(DecontaminationController.FinishDecontamination), e);
                return true;
            }
        }
    }
}
