using System;
using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    public static class SetClassPatch
    {
        public static void Postfix(CharacterClassManager __instance, RoleType classid, GameObject ply, bool lite = false, bool escape = false)
        {
            try
            {
                Player player = Server.PlayerList.GetPlayer(ply);
                if (ply == null)
                    return;
                Environment.OnSetClass(player, classid, true, out RoleType roleType, out bool allow);
                if (roleType != player.Role)
                    player.SetRole(roleType, true, false);
                if (!allow)
                    player.Kill();
            }
            catch (Exception e)
            {
                Log.Add(nameof(CharacterClassManager.SetPlayersClass), e);
            }
        }
    }
}
