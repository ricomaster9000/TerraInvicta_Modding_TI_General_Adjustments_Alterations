using System;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.factionstate
{
    public class MissionControlContributionFromHabs_Patch
    {
        public static void GetMissionControlContributionFromHabs_Postfix(ref int __result)
        {
            __result *= (int)Math.Round(Config.GetValueAsFloat("faction_hab_mission_point_generation_multiplier"));
        }

        public static bool MonthlyResourceIncome_Prefix(FactionResource resource, TIGameState location, TIFactionState faction, TIHabModuleTemplate __instance, ref float __result)
        {
            if (resource == FactionResource.MissionControl)
            {
                __result = __instance.missionControl * Config.GetValueAsFloat("faction_hab_mission_point_generation_multiplier");
                return false;
            }
            return true;
        }
    }
}