using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.ClearTarget))]
    public static class Handcuffs_ClearTarget
    {
        public static bool Prefix(Handcuffs __instance)
        {
            try
            {
                foreach (GameObject player in PlayerManager.players)
                {
                    Player ply = Server.PlayerList.GetPlayer(player);
                    Player myPlayer = Server.PlayerList.GetPlayer(__instance.MyReferenceHub);
                    if (ply == null || myPlayer == null)
                        return true;
                    if (ply.CufferId == myPlayer.PlayerId)
                    {
                        Environment.OnUncuff(ply, myPlayer, true, out bool allow);
                        if (allow)
                            ply.CufferId = -1;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Handcuffs.ClearTarget), e);
                return true;
            }
        }
    }
}
