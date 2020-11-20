using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseElevator))]
    public static class PlayerInteract_CallCmdUseElevator
    {
        public static bool Prefix(PlayerInteract __instance, GameObject elevator)
        {
            if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                return false;
            if (elevator == null)
                return false;
            Lift component = elevator.GetComponent<Lift>();
            if (component == null)
                return false;
            Player player = Server.PlayerList.GetPlayer(__instance._hub);
            if (player == null)
                return true;
            try
            {
                foreach (Lift.Elevator elevator2 in component.elevators)
                {
                    if (__instance.ChckDis(elevator2.door.transform.position))
                    {
                        Environment.OnElevatorInteract(component, player, true, out bool allow);
                        component.UseLift();
                        __instance.OnInteract();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(PlayerInteract.CallCmdUseElevator), e);
                return true;
            }
        }
    }
}
