using System;
using Harmony;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
    public static class RagdollManager_SpawnRagdoll
    {
        public static Dictionary<Ragdoll, Player> Owners = new Dictionary<Ragdoll, Player>();
        public static Dictionary<Player, List<Ragdoll>> Ragdolls = new Dictionary<Player, List<Ragdoll>>();

        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId)
        {
            try
            {
                if (!ConfigManager.SpawnRagdolls)
                    return false;
                Player player = Server.PlayerList.GetPlayer(__instance.hub);
                if (player == null)
                    return true;
                Role role = __instance.hub.characterClassManager.Classes[classId];
                if (role.model_ragdoll == null)
                    return false;
                GameObject gameObject = UnityEngine.Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
                NetworkServer.Spawn(gameObject);
                Ragdoll component = gameObject.GetComponent<Ragdoll>();
                if (!Owners.ContainsKey(component))
                    Owners.Add(component, player);
                if (!Ragdolls.ContainsKey(player))
                    Ragdolls.Add(player, new List<Ragdoll>());
                if (!Ragdolls[player].Contains(component))
                    Ragdolls[player].Add(component);
                component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
                component.NetworkallowRecall = allowRecall;
                component.NetworkPlayerVelo = velocity;
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(RagdollManager.SpawnRagdoll), e);
                return true;
            }
        }
    }
}
