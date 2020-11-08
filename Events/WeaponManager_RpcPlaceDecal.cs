using System;
using Harmony;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.RpcPlaceDecal))]
    public static class WeaponManager_RpcPlaceDecal
    {
        public static Vector3 LastPosition { get; set; }
        public static int LastType { get; set; }
        public static Quaternion LastRotation { get; set; }
        public static bool LastIsBlood { get; set; }

        public static bool Prefix(WeaponManager __instance, bool isBlood, ref int type, ref Vector3 pos, ref Quaternion rot)
        {
            try
            {
                if (isBlood)
                {
                    LastPosition = pos;
                    LastType = type;
                    LastRotation = rot;
                    LastIsBlood = isBlood;
                    Environment.OnPlaceBlood(type, pos, true, out pos, out bool allow); 
                    return allow && ConfigManager.SpawnBlood;
                }
                else
                {
                    LastPosition = pos;
                    LastType = type;
                    LastRotation = rot;
                    LastIsBlood = isBlood;
                    Environment.OnPlaceDecal(pos, true, out pos, out bool allow);
                    return allow && ConfigManager.SpawnDecal;
                }
            }
            catch (Exception e)
            {
                Log.Add(nameof(WeaponManager.RpcPlaceDecal), e);
                return true;
            }
        }
    }
}
