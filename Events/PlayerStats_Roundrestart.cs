using System;
using GameCore;
using Harmony;
using Vigilance.API;
using Dissonance.Integrations.MirrorIgnorance;
using Mirror;
using MEC;
using UnityEngine;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class PlayerStats_Roundrestart
    {
        public static bool Prefix(PlayerStats __instance) => Restart.Commit(__instance, ConfigManager.FastRestart ? Restart.RestartType.Fast : Restart.RestartType.Normal);
    }

    public static class Restart
    {
        public static bool RestartMessageShown = false;

        public enum RestartType
        {
            Normal,
            Fast
        }

        public static void ServerRestart()
        {
            Log.Add("RESTART", $"The server is about to restart!", ConsoleColor.Red);
            Environment.Cache.LocalStats.Roundrestart();
            Timing.CallDelayed(1.5f, () => Application.Quit());
        }

        public static bool Commit(PlayerStats ps, RestartType type)
        {
            try
            {
                if (!RestartMessageShown)
                {
                    Log.Add("RESTART", $"The round is restarting! Using a {type} restart.", ConsoleColor.Magenta);
                    RestartMessageShown = true;
                }

                Round.CurrentState = Enums.RoundState.Restarting;
                Environment.OnRoundRestart();
                PauseAndDelay();

                if (type == RestartType.Fast)
                {
                    DoFastRestart(ps);
                }
                else
                {
                    DoNormalRestart(ps);
                }

                Clear();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("RESTART", "An error occured while restarting the round!", LogType.Error);
                Log.Add("RESTART", e);
                return true;
            }
        }

        public static void PauseAndDelay()
        {
            CustomLiteNetLib4MirrorTransport.DelayConnections = true;
            CustomLiteNetLib4MirrorTransport.UserIdFastReload.Clear();
            foreach (MirrorIgnorancePlayer pl in Map.FindObjects<MirrorIgnorancePlayer>())
                pl?.OnDisable();
        }

        public static void DoNormalRestart(PlayerStats ps)
        {
            SendRestartRpc(ps.ccm._hub);
            ChangeScene(ps);
        }

        public static bool DoFastRestart(PlayerStats ps)
        {
            try
            {
                foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
                {
                    if (!referenceHub.isDedicatedServer)
                    {
                        try
                        {
                            CustomLiteNetLib4MirrorTransport.UserIdFastReload.Add(referenceHub.characterClassManager.UserId);
                        }
                        catch (Exception e)
                        {
                            Log.Add("RESTART", "An exception occured while trying to do a fast restart.", LogType.Error);
                            Log.Add("RESTART", e);
                        }
                    }
                }

                ps?.RpcFastRestart();
                ChangeScene(ps);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("RESTART", "An exception occured while trying to do a fast restart.", LogType.Error);
                Log.Add("RESTART", e);
                return true;
            }
        }

        public static void ChangeScene(PlayerStats ps)
        {
            ps.Invoke("ChangeLevel", 2.5f);
        }

        public static void Clear()
        {
            RagdollManager_SpawnRagdoll.Owners.Clear();
            RagdollManager_SpawnRagdoll.Ragdolls.Clear();
            Inventory_CallCmdDropItem.Pickups.Clear();

            if (ConfigManager.DisableLocksOnRestart)
            {
                RoundSummary.RoundLock = false;
                RoundStart.LobbyLock = false;
            }

            Server.PlayerList.Reset();
        }

        public static void SendRestartRpc(ReferenceHub hub)
        {
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WriteSingle(PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000f);
            writer.WriteBoolean(true);
            hub.playerStats.SendRPCInternal(typeof(PlayerStats), "RpcRoundrestart", writer, 0);
            NetworkWriterPool.Recycle(writer);
        }
    }
}
