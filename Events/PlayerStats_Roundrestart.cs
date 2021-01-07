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
        public static bool Prefix(PlayerStats __instance) => Restart.DoRestart(__instance, ConfigManager.FastRestart);
    }

    public static class Restart
    {
        public static bool RestartMessageShown = false;

        public static void ServerRestart()
        {
            Log.Add("RESTART", $"The server is about to restart!", ConsoleColor.Red);
            Environment.Cache.LocalStats.Roundrestart();
            Timing.CallDelayed(1.5f, () => Application.Quit());
        }

        public static bool DoRestart(PlayerStats ps, bool fast)
        {
            try
            {
                if (!RestartMessageShown)
                {
                    Log.Add("RESTART", $"The round is restarting! Using a {(fast ? "fast" : "normal")} restart.", ConsoleColor.Magenta);
                    RestartMessageShown = true;
                }

                Round.CurrentState = Enums.RoundState.Restarting;
                Environment.OnRoundRestart();
                CustomLiteNetLib4MirrorTransport.DelayConnections = true;
                IdleMode.PauseIdleMode = true;
                foreach (MirrorIgnorancePlayer ig in Map.FindObjects<MirrorIgnorancePlayer>())
                    ig?.OnDisable();
                if (fast)
                    return DoFastRestart(ps);
                else
                {
                    SendRestartRpc(ps);
                    ChangeScene();
                    Clear();
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Add("RESTART", "An error occured while restarting the round!", LogType.Error);
                Log.Add("RESTART", e);
                return true;
            }
        }

        public static bool DoFastRestart(PlayerStats ps)
        {
            try
            {
                CustomLiteNetLib4MirrorTransport.UserIdFastReload.Clear();
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
                PlayerStats.StaticChangeLevel(false);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("RESTART", "An exception occured while trying to do a fast restart.", LogType.Error);
                Log.Add("RESTART", e);
                return true;
            }
        }

        public static void ChangeScene()
        {
            Log.Add("RESTART", $"Changing scene to {NetworkManager.singleton.onlineScene}", ConsoleColor.Magenta);
            Environment.Cache.CollectGarbage();
            PlayerStats._rrTime = DateTime.Now;
            PlayerStats.UptimeRounds += 1U;
            NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
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

        public static void SendRestartRpc(PlayerStats ps)
        {
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WriteSingle(1f);
            writer.WriteBoolean(true);
            ps.SendRPCInternal(typeof(PlayerStats), "RpcRoundrestart", writer, 0);
            NetworkWriterPool.Recycle(writer);
        }
    }
}
