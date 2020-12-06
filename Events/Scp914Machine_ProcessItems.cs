using System;
using Harmony;
using UnityEngine;
using Mirror;
using Scp914;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.ProcessItems))]
    public static class Scp914Machine_ProcessItems
    {
        public static bool Prefix(Scp914Machine __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    return false;
                Collider[] array = Physics.OverlapBox(__instance.intake.position, __instance.inputSize / 2f);
                __instance.players.Clear();
                __instance.items.Clear();
                if (array == null)
                    return false;
                foreach (Collider collider in array)
                {
                    CharacterClassManager component = collider.GetComponent<CharacterClassManager>();
                    if (component != null)
                        __instance.players.Add(component);
                    else
                    {
                        Pickup component2 = collider.GetComponent<Pickup>();
                        if (component2 != null)
                            __instance.items.Add(component2);
                    }
                }
                Environment.OnSCP914Ugrade(__instance.players, __instance.items, __instance.knobState, true, out __instance.knobState, out bool allow);
                if (!allow)
                    return false;
                foreach (CharacterClassManager ccm in __instance.players)
                {
                    Player player = Server.PlayerList.GetPlayer(ccm._hub);
                    if (player != null)
                    {
                        Environment.OnScp914UpgradePlayer(player, true, out bool allow2);
                        if (!allow2)
                            return false;
                    }
                }
                __instance.MoveObjects(__instance.items, __instance.players);
                __instance.UpgradeObjects(__instance.items, __instance.players);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp914Machine.ProcessItems), e);
                return true;
            }
        }
    }
}
