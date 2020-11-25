using Harmony;
using Vigilance.API;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUsePanel))]
    public static class PlayerInteract_CallCmdUsePanel
    {
		public static bool Prefix(PlayerInteract __instance, PlayerInteract.AlphaPanelOperations n)
		{
			try
			{
				if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract))
					return false;
				Player player = Server.PlayerList.GetPlayer(__instance._hub);
				if (player == null || Map.NukesitePanel == null || Map.NukesitePanel.transform == null)
					return true;
				if (!__instance.ChckDis(Map.NukesitePanel.transform.position))
					return false;
				if (n == PlayerInteract.AlphaPanelOperations.Cancel)
				{
					Environment.OnWarheadCancel(player, Map.Warhead.TimeToDetonation, true, out AlphaWarheadController.Host.timeToDetonation, out bool allow);
					if (!allow)
						return false;
					__instance.OnInteract();
					AlphaWarheadController.Host.CancelDetonation(player.GameObject);
					ServerLogs.AddLog(ServerLogs.Modules.Warhead, player.Hub.LoggedNameFromRefHub() + " cancelled the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent, false);
					return false;
				}

				if (n == PlayerInteract.AlphaPanelOperations.Lever)
				{
					if (!Map.NukesitePanel.AllowChangeLevelState())
						return false;
					bool state;
					if (Map.NukesitePanel.Networkenabled)
						state = false;
					else
						state = true;
					Environment.OnSwitchLever(player, Map.NukesitePanel.enabled, state, true, out state, out bool allow);
					if (!allow)
						return false;
					__instance.OnInteract();
					Map.NukesitePanel.Networkenabled = state;
					__instance.RpcLeverSound();
					ServerLogs.AddLog(ServerLogs.Modules.Warhead, player.Hub.LoggedNameFromRefHub() + " set the Alpha Warhead status to " + Map.NukesitePanel.enabled.ToString() + ".", ServerLogs.ServerLogType.GameEvent, false);
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add(nameof(PlayerInteract.CallCmdUsePanel), e);
				return true;
            }
		}
    }
}
