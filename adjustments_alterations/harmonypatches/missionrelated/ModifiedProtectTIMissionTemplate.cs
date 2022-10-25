using PavonisInteractive.TerraInvicta;

namespace TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;

public class ModifiedProtectTIMissionTemplate : TIMissionTemplate
{
    public ModifiedProtectTIMissionTemplate(string name) : base(name)
    {
        this.cost = new TIMissionCost_Bonus();
        this.cost.resourceType = FactionResource.Operations;
    }
}