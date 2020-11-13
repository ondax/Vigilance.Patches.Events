using System;
using System.Collections.Generic;
using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
    public static class NineTailedFoxAnnouncer_AnnounceScpTermination
    {
        public static Role LastRole { get; set; }
        public static PlayerStats.HitInfo LastInfo { get; set; }
        public static string LastGroupId { get; set; }

        public static bool Prefix(Role scp, PlayerStats.HitInfo hit, string groupId)
        {
            try
            {
                if (scp == null)
                    return true;
                Player attacker = null;
                foreach (Player ply in Server.PlayerList.Players.Values)
                    if (ply.Nick == hit.Attacker)
                        attacker = ply;
                if (attacker == null)
                    return true;
                Environment.OnAnnounceSCPTermination(attacker, scp, hit, string.IsNullOrEmpty(hit.Attacker) ? "NONE" : hit.Attacker, true, out bool allow);
                if (!allow)
                    return false;
                if (NineTailedFoxAnnouncer.singleton == null)
                    return false;
                LastRole = scp;
                LastInfo = hit;
                LastGroupId = groupId;
                NineTailedFoxAnnouncer.singleton.scpListTimer = 0f;
                if (!string.IsNullOrEmpty(groupId))
                {
                    foreach (NineTailedFoxAnnouncer.ScpDeath scpDeath in NineTailedFoxAnnouncer.scpDeaths)
                    {
                        if (scpDeath.group == groupId)
                        {
                            scpDeath.scpSubjects.Add(scp);
                            return false;
                        }
                    }
                }
                NineTailedFoxAnnouncer.scpDeaths.Add(new NineTailedFoxAnnouncer.ScpDeath
                {
                    scpSubjects = new List<Role>(new Role[]
                    {
                        scp
                    }),
                    group = groupId,
                    hitInfo = hit
                });
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(NineTailedFoxAnnouncer.AnnounceScpTermination), e);
                return true;
            }
        }
    }
}
