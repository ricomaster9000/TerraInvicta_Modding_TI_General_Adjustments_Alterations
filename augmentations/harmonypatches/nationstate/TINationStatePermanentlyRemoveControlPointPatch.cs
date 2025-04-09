using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace TI_Augmenter.augmentations.harmonypatches.nationstate
{
    public class TINationStatePermanentlyRemoveControlPointPatch
    {
        public static ISet<TIControlPoint> ControlPointsToRemove = new HashSet<TIControlPoint>();
        
        public static bool SelfDisableControlPointsPrefix(TINationState __instance, TIFactionState faction)
        {
            if (faction.isActivePlayer)
            {
                foreach (TIControlPoint ticontrolPoint in __instance.controlPoints)
                {
                    if (ticontrolPoint.faction == faction)
                    {
                        ControlPointsToRemove.Add(ticontrolPoint);
                    }
                }

                return false;
            }
            return true;
        }
    }
}