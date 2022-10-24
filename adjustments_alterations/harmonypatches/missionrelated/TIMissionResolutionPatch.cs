using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

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
                    if (Config.IsDebugModeActive())
                    {
                        Console.WriteLine("TIMissionResolutionPatch - GetAllModifiers_Postfix: Handling mission success change decrease for agent because of failures, AgentDetails: " + keyName + " " + councilor.displayName);
                    }
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
                    if (Config.IsDebugModeActive())
                    {
                        Console.WriteLine("TIMissionResolutionPatch - GetAllModifiers_Postfix: decreaseBy -> " + decreaseBy);
                    }
                    __result.Add(new FollowUpFailureModifier(decreaseBy));
                }
                if (FollowUpSuccessModifier.GetFollowUpSuccessHolder().ContainsKey(keyName))
                {
                    if (Config.IsDebugModeActive())
                    {
                        Console.WriteLine("TIMissionResolutionPatch - GetAllModifiers_Postfix: Handling mission success change decrease for agent because of successes, AgentDetails: " + keyName + " " + councilor.displayName);
                    }
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
                    if (Config.IsDebugModeActive())
                    {
                        Console.WriteLine("TIMissionResolutionPatch - GetAllModifiers_Postfix: decreaseBy -> " + decreaseBy);
                    }
                    __result.Add(new FollowUpSuccessModifier(decreaseBy));
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
            if (Config.IsDebugModeActive())
            {
                Console.WriteLine("TIMissionResolutionPatch - getTotalModifierFromModifiers(): result: " + result);
            }
            return result;
        }

    }
}