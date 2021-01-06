using System;
using System.Collections.Generic;
using GameCore;
using LightContainmentZoneDecontamination;
using Mirror;
using UnityEngine;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static class PocketDimensionTeleport_OnTriggerEnter
    {
        public static bool Prefix(PocketDimensionTeleport __instance, Collider other)
        {
            return true;
        }
    }
}
