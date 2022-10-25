using System;
using PavonisInteractive.TerraInvicta;
using TI_General_Adjustments_Alterations;

namespace TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;

public class StartMissionPhase_Patch
{
    public static bool Prefix(CouncilorMissionCanvasController __instance)
    {
        Main.logDebug("StartMissionPhase_Patch - Prefix - missionPhaseStart triggered");
        AssignCouncilorToMission_Patch.missionDecreaseAdjustmentsToApply.Clear();
        return true;
    }
}