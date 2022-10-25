using System;
using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated
{
    public class TIMissionStatePatch
    {
        public static short agent_follow_up_failures_decrease_chances_of_success_max_count = 0;
        public static short agent_follow_up_successes_decrease_chances_of_success_max_count = 0;
        
        public static void setConfigVariables()
        {
            agent_follow_up_failures_decrease_chances_of_success_max_count = Config.GetValueAsShort("agent_follow_up_failures_decrease_chances_of_success_max_count");
            agent_follow_up_successes_decrease_chances_of_success_max_count = Config.GetValueAsShort("agent_follow_up_successes_decrease_chances_of_success_max_count");
        }

        public static void ResolveMission_Postfix(MissionResult __result)
        {
            if (__result.councilor != null && __result.missionTemplate != null && __result.target != null)
            {
                String keyName = __result.councilor.ID + "_" + __result.missionTemplate.dataName + "_" + __result.target.ID;
                if (__result.Failed)
                {
                    Main.logDebug("TIMissionStatePatch - ResolveMission_Postfix: Handling mission failure of agent, AgentDetails: " + keyName + " " + __result.councilor.displayName);
                    if (!FollowUpFailureModifier.GetFollowUpFailuresLastFailedDateHolder().ContainsKey(keyName))
                    {
                        FollowUpFailureModifier.GetFollowUpFailuresLastFailedDateHolder().Add(keyName, TITimeState.Now());
                    }
                    if (!FollowUpFailureModifier.GetFollowUpFailuresHolder().ContainsKey(keyName))
                    {
                        FollowUpFailureModifier.GetFollowUpFailuresHolder().Add(keyName, 0);
                    }
                    if (TITimeState.Now().DifferenceInDays(FollowUpFailureModifier.GetFollowUpFailuresLastFailedDateHolder()[keyName]) > 56)
                    {
                        Main.logDebug("TIMissionStatePatch - ResolveMission_Postfix: follow-up-failures-change - Resetting agent's success decrease modifier to 0. AgentDetails: " + keyName + " " + __result.councilor.displayName);
                        FollowUpFailureModifier.GetFollowUpFailuresHolder()[keyName] = 0;
                    }
                    if (FollowUpFailureModifier.GetFollowUpFailuresHolder()[keyName] < agent_follow_up_failures_decrease_chances_of_success_max_count)
                    {
                        FollowUpFailureModifier.GetFollowUpFailuresHolder()[keyName]++;
                        FollowUpFailureModifier.GetFollowUpFailuresLastFailedDateHolder()[keyName] = TITimeState.Now();
                    }
                }
                if (__result.Success)
                {
                    Main.logDebug("TIMissionStatePatch - ResolveMission_Postfix: Handling mission success of agent, AgentDetails: " + keyName + " " + __result.councilor.displayName);
                    if (!FollowUpSuccessModifier.GetFollowUpSuccessesLastSucceededDateHolder().ContainsKey(keyName))
                    {
                        FollowUpSuccessModifier.GetFollowUpSuccessesLastSucceededDateHolder().Add(keyName, TITimeState.Now());
                    }
                    if (!FollowUpSuccessModifier.GetFollowUpSuccessHolder().ContainsKey(keyName))
                    {
                        FollowUpSuccessModifier.GetFollowUpSuccessHolder().Add(keyName, 0);
                    }
                    if (TITimeState.Now().DifferenceInDays(FollowUpSuccessModifier.GetFollowUpSuccessesLastSucceededDateHolder()[keyName]) > 56)
                    {
                        Main.logDebug("TIMissionStatePatch - ResolveMission_Postfix: follow-up-successes-change - Resetting agent's success decrease modifier to 0. AgentDetails: " + keyName + " " + __result.councilor.displayName);
                        FollowUpSuccessModifier.GetFollowUpSuccessHolder()[keyName] = 0;
                    }
                    if (FollowUpSuccessModifier.GetFollowUpSuccessHolder()[keyName] < agent_follow_up_successes_decrease_chances_of_success_max_count)
                    {
                        FollowUpSuccessModifier.GetFollowUpSuccessHolder()[keyName]++;
                        FollowUpSuccessModifier.GetFollowUpSuccessesLastSucceededDateHolder()[keyName] = TITimeState.Now();
                    }
                }
            }
        }
    }
}