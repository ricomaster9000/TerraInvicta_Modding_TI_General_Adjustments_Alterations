using PavonisInteractive.TerraInvicta;
using TI_Augmenter.augmentations.harmonypatches.nationstate;

namespace TI_Augmenter.augmentations.harmonypatches.missionrelated;

public class StartMissionPhase_Patch
{
    public static bool Prefix(CouncilorMissionCanvasController __instance)
    {
        Main.logDebug("StartMissionPhase_Patch - Prefix - missionPhaseStart triggered");
        AssignCouncilorToMission_Patch.missionDecreaseAdjustmentsToApply.Clear();
        return true;
    }
    
    public static void Postfix(CouncilorMissionCanvasController __instance)
    {
        Main.logDebug("Remove-Control-Points-On-Abandon-Nation -> removing control points from nation");
        foreach (TIControlPoint controlPoint in TINationStatePermanentlyRemoveControlPointPatch.ControlPointsToRemove)
        {
            controlPoint.SetFaction(null, false);
        }

        TINationStatePermanentlyRemoveControlPointPatch.ControlPointsToRemove.Clear();
    }
}