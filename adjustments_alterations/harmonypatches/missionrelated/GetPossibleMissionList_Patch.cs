using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.missionrelated;

public class GetPossibleMissionList_Patch
{
    public static void GetPossibleMissionList_MissionSliderAdjustments_Postfix(List<TIMissionTemplate> __result)
    {
        TIMissionTemplate protectMission = __result?.Find(template => template.dataName == "Protect");
        if (protectMission != null)
        {
            Main.logDebug("GetPossibleMissionList_Patch - GetPossibleMissionList_MissionSliderAdjustments_Postfix - adding slider to protect mission");
            protectMission.cost = new TIMissionCost_Bonus();
            protectMission.cost.resourceType = FactionResource.Operations;
        }
    }
}