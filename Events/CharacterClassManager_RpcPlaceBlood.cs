using System;
using Harmony;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcPlaceBlood))]
    public static class CharacterClassManager_RpcPlaceBlood
    {
        public static Vector3 LastPosition { get; set; }
        public static int LastType { get; set; }

        public static bool Prefix(CharacterClassManager __instance, ref Vector3 pos, ref int type, ref float f)
        {
            try
            {
                LastPosition = pos;
                LastType = type;
                Environment.OnPlaceBlood(type, pos, true, out pos, out bool allow);
                return allow && ConfigManager.SpawnBlood;
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.RpcPlaceBlood), e);
                return true;
            }
        }
    }
}
