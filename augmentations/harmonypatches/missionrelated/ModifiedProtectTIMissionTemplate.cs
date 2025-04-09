using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.missionrelated;

public class ModifiedProtectTIMissionTemplate : TIMissionTemplate
{
    public ModifiedProtectTIMissionTemplate(string name) : base(name)
    {
        this.cost = new TIMissionCost_Bonus();
        this.cost.resourceType = FactionResource.Operations;
    }
}