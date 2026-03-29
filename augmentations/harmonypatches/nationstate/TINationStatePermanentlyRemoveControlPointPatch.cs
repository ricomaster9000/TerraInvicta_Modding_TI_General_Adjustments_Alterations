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
                        if (faction.permaAbandonedNations.Contains(__instance))
                        {
                            ControlPointsToRemove.Add(ticontrolPoint);
                            Main.logDebug(ticontrolPoint.displayName + " added for permanent removal at next mission start phase");
                        }
                        else
                        {
                            Main.logDebug("applied normal crackdown effect for " + ticontrolPoint.displayName);
                            ticontrolPoint.ResolveCrackdownEffect(TemplateManager.global.selfDisableControlPointDuration_months, faction, true, false, 0f);
                        }
                    }
                }

                return false;
            }
            return true;
        }
    }
}