using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated
{
    public class ResourceCostCanAffordPatch
    {

        public static void setConfigVariables()
        {
            //agent_follow_up_failures_decrease_chances_of_success_factor = Config.GetValueAsFloat("agent_follow_up_failures_decrease_chances_of_success_factor"); 
        }

        public static bool CanAfford_Postfix(
            ref TIResourcesCost __instance,
            ref bool __result,
            TIFactionState faction,
            float maxFractionCanSpend = 1f,
            List<FactionResource> resourcesToPreserve = null,
            float maxDays = float.PositiveInfinity
        ) {
            Main.logDebug("ResourceCostCanAffordPatch - checking if with allowed debt if resource cost can be afforded");
            if (!__result)
            {
                foreach (ResourceValue resourceValue in __instance.resourceCosts)
                {
                    if (resourceValue.value > 0f)
                    {
                        float totalAvailableResource = 0.00f;
                        if (resourcesToPreserve != null && resourcesToPreserve.Contains(resourceValue.resource))
                        {
                            totalAvailableResource = faction.GetCurrentResourceAmount(resourceValue.resource) * maxFractionCanSpend;
                        }
                        else
                        {
                            totalAvailableResource = faction.GetCurrentResourceAmount(resourceValue.resource);
                        }

                        Main.logDebug("ResourceCostCanAffordPatch - adding available debt");
                        totalAvailableResource += faction.GetYearlyIncome(resourceValue.resource);

                        if (totalAvailableResource < resourceValue.value || __instance.completionTime_days > maxDays)
                        {
                            __result = false;
                        }
                    }
                }
            }
            return __result;
        }

    }
}