using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated
{
    public class TIMissionResolutionPatch
    {
        public static float agent_follow_up_failures_decrease_chances_of_success_factor = 0.00f;
        public static float agent_follow_up_successes_decrease_chances_of_success_factor = 0.00f;

        public static void setConfigVariables()
        {
            agent_follow_up_failures_decrease_chances_of_success_factor = Config.GetValueAsFloat("agent_follow_up_failures_decrease_chances_of_success_factor"); 
            agent_follow_up_successes_decrease_chances_of_success_factor = Config.GetValueAsFloat("agent_follow_up_successes_decrease_chances_of_success_factor");
        }

        public static void GetAllModifiers_Postfix(ref List<TIMissionModifier> __result, TIMissionTemplate mission, bool attacking, TICouncilorState councilor, TIGameState target, float resourcesSpent)
        {
            if (councilor != null && mission != null && target != null && !attacking) {
                String keyName = councilor.ID + "_" + mission.dataName + "_" +target.ID;
                float totalSumFromModifiers;
                float decreaseBy;

                if (FollowUpFailureModifier.GetFollowUpFailuresHolder().ContainsKey(keyName))
                {
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix: Handling mission success change decrease for agent because of failures, AgentDetails: " + keyName + " " + councilor.displayName);
                    totalSumFromModifiers = GetTotalModifierFromModifiers(__result, mission, attacking, councilor, target, resourcesSpent);
                    if (totalSumFromModifiers > 0)
                    {
                        decreaseBy = FollowUpFailureModifier.GetFollowUpFailuresHolder().GetValueSafe(keyName) * agent_follow_up_failures_decrease_chances_of_success_factor;
                        decreaseBy = totalSumFromModifiers * decreaseBy;
                    }
                    else
                    {
                        decreaseBy = 5 * agent_follow_up_failures_decrease_chances_of_success_factor;
                    }
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix: decreaseBy -> " + decreaseBy);
                    __result.Add(new FollowUpFailureModifier(decreaseBy));
                }
                if (FollowUpSuccessModifier.GetFollowUpSuccessHolder().ContainsKey(keyName))
                {
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix: Handling mission success change decrease for agent because of successes, AgentDetails: " + keyName + " " + councilor.displayName);
                    totalSumFromModifiers = GetTotalModifierFromModifiers(__result, mission, attacking, councilor, target, resourcesSpent);
                    if (totalSumFromModifiers > 0)
                    {
                        decreaseBy = FollowUpSuccessModifier.GetFollowUpSuccessHolder().GetValueSafe(keyName) * agent_follow_up_successes_decrease_chances_of_success_factor;
                        decreaseBy = totalSumFromModifiers * decreaseBy;
                    }
                    else
                    {
                        decreaseBy = 5 * agent_follow_up_successes_decrease_chances_of_success_factor;
                    }
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix: decreaseBy -> " + decreaseBy);
                    __result.Add(new FollowUpSuccessModifier(decreaseBy));
                }
                string keyDefenseMissionModifierAdjustment = "Protect_" + councilor.location.ID;
                if (AssignCouncilorToMission_Patch.missionDecreaseAdjustmentsToApply.ContainsKey(keyDefenseMissionModifierAdjustment))
                {
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix - found key, adding DefenseMissionCostModifier bonus");
                    List<AssignCouncilorToMission_Patch.MissionAlteration> missionAlterationsToApply =
                        AssignCouncilorToMission_Patch.missionDecreaseAdjustmentsToApply[keyDefenseMissionModifierAdjustment].FindAll(alteration => !alteration.getFactionWhoInitializedMissionId().Equals(councilor.faction.ID));

                    float applyTotalDecrease = 0.00f;
                    foreach (AssignCouncilorToMission_Patch.MissionAlteration missionAlteration in missionAlterationsToApply)
                    {
                        applyTotalDecrease += missionAlteration.getAlterationValue();
                    }
                    Main.logDebug("TIMissionResolutionPatch - GetAllModifiers_Postfix - DefenseMissionCostModifier bonus: " + applyTotalDecrease);
                    __result.Add(new DefenseMissionCostModifier(applyTotalDecrease));
                }
            }
        }

        static float GetTotalModifierFromModifiers(List<TIMissionModifier> modifiers, TIMissionTemplate mission, bool attacking, TICouncilorState councilor, TIGameState target, float resourcesSpent)
        {
            float result = 0.00f;
            foreach (TIMissionModifier modifier in modifiers)
            {
                result += modifier.GetModifier(councilor,target,resourcesSpent,(mission.cost != null) ? mission.cost.resourceType : FactionResource.None);
            }
            Main.logDebug("TIMissionResolutionPatch - getTotalModifierFromModifiers(): result: " + result);
            return result;
        }

    }
}