using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using TI_General_Adjustments_Alterations;

namespace TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;

[SuppressMessage("ReSharper", "PossibleLossOfFraction")]
public class AssignCouncilorToMission_Patch
{
    public static Dictionary<String, List<MissionAlteration>> missionDecreaseAdjustmentsToApply = new Dictionary<string, List<MissionAlteration>>();
    public static float mission_related_slider_operations_boost_modifier;

    public static void setConfigVariables()
    {
        mission_related_slider_operations_boost_modifier = Config.GetValueAsFloat("mission_related_slider_operations_boost_modifier");
    }

    public static void Execute_MissionSliderAdjustments_PostFix(AssignCouncilorToMission __instance)
    {
        TICouncilorState councilorExecutingMission = __instance.councilorID.GetState<TICouncilorState>(false);
        TIMissionState activeMission = councilorExecutingMission.activeMission;
        if (activeMission != null &&
            activeMission.missionTemplate != null &&
            activeMission.missionTemplate.dataName == "Protect" &&
            activeMission.resources > 0)
        {
            string key = activeMission.missionTemplate.dataName + "_" + activeMission.targetLocation.ID;
            float decreasyBy = (int)activeMission.resources * mission_related_slider_operations_boost_modifier;
            Main.logDebug("AssignCouncilorToMission_Patch - Execute_MissionSliderAdjustments_PostFix - adding to protectMissionDifficultyIncreases value: " + decreasyBy + " key: " + key);
            if (!missionDecreaseAdjustmentsToApply.ContainsKey(key))
            {
                missionDecreaseAdjustmentsToApply.Add(key, new List<MissionAlteration>());
                missionDecreaseAdjustmentsToApply[key].Add(new MissionAlteration(councilorExecutingMission.ID,councilorExecutingMission.faction.ID,decreasyBy));
            }
            else
            {
                MissionAlteration possibleDuplicate = missionDecreaseAdjustmentsToApply[key].Find(alteration => alteration.getCouncilorWhoExecutedMissionId().Equals(councilorExecutingMission.ID));
                if (possibleDuplicate != null)
                {
                    Main.logDebug("AssignCouncilorToMission_Patch - Execute_MissionSliderAdjustments_PostFix - replacing in protectMissionDifficultyIncreases value: " + decreasyBy);
                    possibleDuplicate.setCouncilorWhoExecutedMissionId(councilorExecutingMission.ID);
                    possibleDuplicate.setFactionWhoInitializedMissionId(councilorExecutingMission.faction.ID);
                    possibleDuplicate.setAlterationValue(decreasyBy);
                }
                else
                {
                    missionDecreaseAdjustmentsToApply[key].Add(new MissionAlteration(activeMission.targetLocation.ID,councilorExecutingMission.faction.ID,decreasyBy));
                }
            }
        }
    }

    public class MissionAlteration
    {
        private GameStateID councilorWhoExecutedMissionId;
        private GameStateID factionWhoInitializedMissionId;
        private float alterationValue;

        public MissionAlteration(GameStateID councilorWhoExecutedMissionId, GameStateID factionWhoInitializedMissionId, float alterationValue)
        {
            this.councilorWhoExecutedMissionId = councilorWhoExecutedMissionId;
            this.factionWhoInitializedMissionId = factionWhoInitializedMissionId;
            this.alterationValue = alterationValue;
        }

        public GameStateID getCouncilorWhoExecutedMissionId()
        {
            return councilorWhoExecutedMissionId;
        }

        public void setCouncilorWhoExecutedMissionId(GameStateID councilorWhoExecutedMissionId)
        {
            this.councilorWhoExecutedMissionId = councilorWhoExecutedMissionId;
        }

        public GameStateID getFactionWhoInitializedMissionId()
        {
            return factionWhoInitializedMissionId;
        }

        public void setFactionWhoInitializedMissionId(GameStateID factionWhoInitializedMissionId)
        {
            this.factionWhoInitializedMissionId = factionWhoInitializedMissionId;
        }

        public float getAlterationValue()
        {
            return alterationValue;
        }

        public void setAlterationValue(float alterationValue)
        {
            this.alterationValue = alterationValue;
        }
    }
}