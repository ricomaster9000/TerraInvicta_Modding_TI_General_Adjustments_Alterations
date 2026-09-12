using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.councilorstate;

public class TICouncilorState_HireRecruitCost_Patch
{
	public static bool MaxRecruitableCandidatesPrefix(ref int __result)
	{
		__result = 12;
		return false;
	}
	
	public static bool Prefix(TICouncilorState __instance, TIFactionState faction, ref TIResourcesCost __result)
	{
		TIResourcesCost tiresourcesCost = new TIResourcesCost();
		float resourceAmount = 0f;
		if (!faction.ideology.alien && !__instance.template.alien)
		{
			if (__instance.typeTemplate.affinities.IndexOf(faction.ideology.ideology) != -1)
			{
				resourceAmount = (float)TemplateManager.global.affinityCouncilorRecruitCost_influence;
			}
			else
			{
				resourceAmount = (float)TemplateManager.global.baseCouncilorRecruitCost_influence;
			}
		}
		resourceAmount += getTotalAttributeValuesContributingToCost(__instance);
		Main.logDebug("Agent Recruit Pool Hire Cost Adjuster -> Adjusted hire cost to " + resourceAmount + " for agent " + __instance.personalName);
		tiresourcesCost.AddCost(FactionResource.Influence, resourceAmount);
		__result = tiresourcesCost;
		return false;
	}

	private static float getTotalAttributeValuesContributingToCost(TICouncilorState councilor)
	{
		float result = 0;
		float costFactorMultiplier = Config.GetValueAsFloat("agent_attributes_cost_factor_multiplier");
		foreach (KeyValuePair<CouncilorAttribute,int> attribute in councilor.attributes)
		{
			result += attribute.Value * costFactorMultiplier;
		}
		return result;
	}
}