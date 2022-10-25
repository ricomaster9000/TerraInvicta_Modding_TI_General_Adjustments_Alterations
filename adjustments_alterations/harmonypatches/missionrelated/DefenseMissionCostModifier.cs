using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated
{
    public class DefenseMissionCostModifier : TIMissionModifier
    {
        private float modifier = 0.00f;

        public DefenseMissionCostModifier(float modifier)
        {
            this.modifier = modifier;
        }

        public override string displayName
        {
            get
            {
                return "Defense-Bonus-Because-Of-Resources-Spent";
            }
        }

        public float GetModifierCustom()
        {
            return modifier;
        }
        
        public void SetModifierCustom(float modifier)
        {
            this.modifier = modifier;
        }

        public override float GetModifier(
            TICouncilorState attackingCouncilor,
            TIGameState target = null,
            float resourcesSpent = 0.0f,
            FactionResource resource = FactionResource.None)
        {
            return modifier;
        }
    }
}