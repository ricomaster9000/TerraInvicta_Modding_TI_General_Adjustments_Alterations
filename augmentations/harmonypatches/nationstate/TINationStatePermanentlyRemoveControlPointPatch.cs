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
                    if (ticontrolPoint.faction == faction && faction.permaAbandonedNations.Contains(__instance))
                    {
                        ControlPointsToRemove.Add(ticontrolPoint);
                        Main.logDebug(ticontrolPoint.displayName + " added for permanent removal at next mission start phase");
                    }
                }

                return false;
            }
            return true;
        }
    }
}