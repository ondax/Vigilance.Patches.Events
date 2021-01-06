using System;
using Harmony;
using Vigilance.API;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.AllowInteracting))]
    public static class BreakableDoor_AllowInteracting
    {
        public static bool Prefix(BreakableDoor __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (!API.Door.TryGetDoor(__instance, out API.Door door)) return true;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null) return true;
                if (player.PlayerLock) return false;
                Environment.OnDoorInteract(true, door, player, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AirlockController), nameof(AirlockController.OnDoorAction))]
    public static class AirlockController_OnDoorAction
    {
        public static bool Prefix(AirlockController __instance, DoorVariant door, DoorAction action, ReferenceHub ply)
        {
            try
            {
                if (!API.Door.TryGetDoor(door, out API.Door d)) return true;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null) return true;
                if (player.PlayerLock) return false;
                Environment.OnDoorInteract(true, d, player, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(BasicDoor), nameof(BasicDoor.AllowInteracting))]
    public static class BasicDoor_AllowInteracting
    {
        public static bool Prefix(BasicDoor __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (!API.Door.TryGetDoor(__instance, out API.Door door)) return true;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null) return true;
                if (player.PlayerLock) return false;
                Environment.OnDoorInteract(true, door, player, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CheckpointDoor), nameof(CheckpointDoor.AllowInteracting))]
    public static class CheckpointDoor_AllowInteracting
    {
        public static bool Prefix(CheckpointDoor __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (!API.Door.TryGetDoor(__instance, out API.Door door)) return true;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null) return true;
                if (player.PlayerLock) return false;
                Environment.OnDoorInteract(true, door, player, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PryableDoor), nameof(PryableDoor.AllowInteracting))]
    public static class PryableDoor_AllowInteracting
    {
        public static bool Prefix(PryableDoor __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (!API.Door.TryGetDoor(__instance, out API.Door door)) return true;
                Player player = Server.PlayerList.GetPlayer(ply);
                if (player == null) return true;
                if (player.PlayerLock) return false;
                Environment.OnDoorInteract(true, door, player, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }
}
