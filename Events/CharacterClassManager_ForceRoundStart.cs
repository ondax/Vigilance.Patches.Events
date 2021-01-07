using Harmony;
using GameCore;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ForceRoundStart))]
    public static class CharacterClassManager_ForceRoundStart
    {
        public static bool Prefix(CharacterClassManager __instance, ref bool __result)
        {
			Environment.OnRoundStart();
			OneOhSixContainer.used = false;
			ServerLogs.AddLog(ServerLogs.Modules.Logger, "Round has been started.", ServerLogs.ServerLogType.GameEvent, false);
			ServerConsole.AddLog("New round has been started.", System.ConsoleColor.Gray);
			RoundStart.singleton.NetworkTimer = -1;
			RoundStart.RoundStartTimer.Restart();
			__result = true;
			return false;
		}
    }
}
