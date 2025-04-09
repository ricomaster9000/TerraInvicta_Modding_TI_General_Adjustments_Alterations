using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.missionrelated
{
    public class FollowUpFailureModifier : TIMissionModifier
    {
        static readonly Dictionary<String, short> followUpFailuresCountHolder = new Dictionary<String, short>();
        static readonly Dictionary<String, TIDateTime> followUpFailuresLastFailedDateHolder = new Dictionary<String, TIDateTime>();
        private float modifier = 0.00f;

        public FollowUpFailureModifier(float modifier)
        {
            this.modifier = modifier;
        }

        public override string displayName
        {
            get
            {
                return Loc.T("UI.TI_General_Adjustments_Alterations.FollowUpFailureModifier");
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
        
        public static Dictionary<String, short> GetFollowUpFailuresHolder()
        {
            return followUpFailuresCountHolder;
        }
        
        public static Dictionary<String, TIDateTime> GetFollowUpFailuresLastFailedDateHolder()
        {
            return followUpFailuresLastFailedDateHolder;
        }
    }
}