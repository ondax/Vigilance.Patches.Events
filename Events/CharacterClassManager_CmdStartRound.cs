using System;
using Harmony;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CmdStartRound))]
    public static class CharacterClassManager_CmdStartRound
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                Environment.OnRoundStart();
                GameObject.Find("MeshDoor173").GetComponentInChildren<Door>().ForceCooldown(ConfigManager.Scp173DoorCooldown);
                __instance.NetworkRoundStarted = true;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.CmdStartRound), e);
                return true;
            }
        }
    }
}
