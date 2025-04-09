using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.councilorstate;

public class TICouncilorState_HireRecruitCost_Patch
{
	private static float agent_attributes_cost_factor_multiplier = 1.00f;
	
	public static void setConfigVariables()
	{
		if (Config.isKeySet("agent_attributes_cost_factor_multiplier"))
		{
			agent_attributes_cost_factor_multiplier = Config.GetValueAsFloat("agent_attributes_cost_factor_multiplier");
		}
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
		foreach (KeyValuePair<CouncilorAttribute,int> attribute in councilor.attributes)
		{
			result += attribute.Value * agent_attributes_cost_factor_multiplier;
		}
		return result;
	}

	private static List<T> GetEnumList<T>()
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		List<T> list = new List<T>(array);
		return list;
	}
}