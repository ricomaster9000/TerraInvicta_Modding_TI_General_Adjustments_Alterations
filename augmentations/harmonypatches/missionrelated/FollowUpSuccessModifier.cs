using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.missionrelated
{
    public class FollowUpSuccessModifier : TIMissionModifier
    {
        static readonly Dictionary<String, short> followUpSuccessesCountHolder = new Dictionary<String, short>();
        static readonly Dictionary<String, TIDateTime> followUpSucessesLastSuccessDateHolder = new Dictionary<String, TIDateTime>();
        private float modifier = 0.00f;

        public FollowUpSuccessModifier(float modifier)
        {
            this.modifier = modifier;
        }

        public override string displayName
        {
            get
            {
                return Loc.T("UI.TI_Augmenter.FollowUpSuccessModifier");
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
        
        public static Dictionary<String, short> GetFollowUpSuccessHolder()
        {
            return followUpSuccessesCountHolder;
        }
        
        public static Dictionary<String, TIDateTime> GetFollowUpSuccessesLastSucceededDateHolder()
        {
            return followUpSucessesLastSuccessDateHolder;
        }
    }
}