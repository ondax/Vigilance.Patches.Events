using System;
using Harmony;
using Respawning.NamingRules;
using Vigilance.API;
using NorthwoodLib.Pools;
using System.Text;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement))]
    public static class NineTailedFoxNamingRule_PlayEntranceAnnouncement
    {
        public static string LastUnit { get; set; }

        private static bool Prefix(NineTailedFoxNamingRule __instance, string regular)
        {
            try
            {
                string cassieUnitName = __instance.GetCassieUnitName(regular);
                int num = Round.Info.TotalSCPs;
                string[] args = cassieUnitName.Replace("-", " ").Split(' ');
                string u = args[0];
                int n = int.Parse(args[1]);
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
                Environment.OnAnnounceNTFEntrace(num, u, n, true, out num, out string unit, out int number, out bool allow);
                cassieUnitName = $"{unit}-{number}";
                if (!allow)
                    return false;
                LastUnit = cassieUnitName;
                if (ClutterSpawner.IsHolidayActive(Holidays.Christmas))
                {
                    stringBuilder.Append("XMAS_EPSILON11 ");
                    stringBuilder.Append(cassieUnitName);
                    stringBuilder.Append("XMAS_HASENTERED ");
                    stringBuilder.Append(num);
                    stringBuilder.Append(" XMAS_SCPSUBJECTS");
                }
                else
                {
                    stringBuilder.Append("MTFUNIT EPSILON 11 DESIGNATED ");
                    stringBuilder.Append(cassieUnitName);
                    stringBuilder.Append(" HASENTERED ALLREMAINING ");
                    if (num == 0)
                    {
                        stringBuilder.Append("NOSCPSLEFT");
                    }
                    else
                    {
                        stringBuilder.Append("AWAITINGRECONTAINMENT ");
                        stringBuilder.Append(num);
                        if (num == 1)
                        {
                            stringBuilder.Append(" SCPSUBJECT");
                        }
                        else
                        {
                            stringBuilder.Append(" SCPSUBJECTS");
                        }
                    }
                }
                __instance.ConfirmAnnouncement(ref stringBuilder);
                StringBuilderPool.Shared.Return(stringBuilder);
                return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement), e);
                return true;
            }
        }
    }
}
