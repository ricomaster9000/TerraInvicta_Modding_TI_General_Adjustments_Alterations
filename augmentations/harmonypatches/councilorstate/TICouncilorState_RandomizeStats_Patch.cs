using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.councilorstate;

public class TICouncilorState_RandomizeStats_Patch
{
	private static readonly Random Random = new Random();

	public static bool Prefix(TICouncilorState __instance, bool forceBestStats)
	{
		// Random.Next upper bound is exclusive: Next(0,2) yields 0 or 1, so roughly half of the pool keeps vanilla stats
		if (forceBestStats || Random.Next(0,2) == 1)
		{
			return true;
		}
		Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix: modifying the maximum value attributes can be when recruiting agents");
		__instance.attributes[CouncilorAttribute.Persuasion] = __instance.typeTemplate.basePersuasion +
		                                                       UnityEngine.Random.Range(0, (__instance.typeTemplate.randPersuasion + 1)*getModifierForAttribute(CouncilorAttribute.Persuasion));
		__instance.attributes[CouncilorAttribute.Espionage] = __instance.typeTemplate.baseEspionage +
		                                                      UnityEngine.Random.Range(0, (__instance.typeTemplate.randEspionage + 1)*getModifierForAttribute(CouncilorAttribute.Espionage));
		__instance.attributes[CouncilorAttribute.Command] = __instance.typeTemplate.baseCommand +
		                                                    UnityEngine.Random.Range(0, (__instance.typeTemplate.randCommand + 1)*getModifierForAttribute(CouncilorAttribute.Command));
		__instance.attributes[CouncilorAttribute.Investigation] = __instance.typeTemplate.baseInvestigation +
		                                                          UnityEngine.Random.Range(0, (__instance.typeTemplate.randInvestigation + 1)*getModifierForAttribute(CouncilorAttribute.Investigation));
		__instance.attributes[CouncilorAttribute.Science] = __instance.typeTemplate.baseScience +
		                                                    UnityEngine.Random.Range(0, (__instance.typeTemplate.randScience + 1)*getModifierForAttribute(CouncilorAttribute.Science));
		__instance.attributes[CouncilorAttribute.Administration] = __instance.typeTemplate.baseAdministration +
		                                                           UnityEngine.Random.Range(0, (__instance.typeTemplate.randAdministration + 1)*getModifierForAttribute(CouncilorAttribute.Administration));
		__instance.attributes[CouncilorAttribute.Security] = __instance.typeTemplate.baseSecurity +
		                                                     UnityEngine.Random.Range(0, (__instance.typeTemplate.randSecurity + 1)*getModifierForAttribute(CouncilorAttribute.Security));
		__instance.attributes[CouncilorAttribute.Loyalty] = __instance.typeTemplate.baseLoyalty +
		                                                    UnityEngine.Random.Range(0, (__instance.typeTemplate.randLoyalty + 1)*getModifierForAttribute(CouncilorAttribute.Loyalty));
		__instance.attributes[CouncilorAttribute.ApparentLoyalty] = __instance.attributes[CouncilorAttribute.Loyalty] - 2 +
		                                                            UnityEngine.Random.Range(0, 4*getModifierForAttribute(CouncilorAttribute.ApparentLoyalty));

		Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix - name: " + __instance.displayName);
		foreach (var keyValuePair in __instance.attributes)
		{
			Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix - " + keyValuePair.Key + " value " + keyValuePair.Value);
		}
		
		return false;
	}

	private static int getModifierForAttribute(CouncilorAttribute attribute)
	{
		float result = Config.GetValueAsFloat("agent_attributes_all_range_recruit_pool_modifier");
		// optional per-attribute override, e.g. agent_attributes_investigation_range_recruit_pool_modifier
		float specificModifier = Config.GetValueAsFloat("agent_attributes_" + attribute.ToString().ToLower() + "_range_recruit_pool_modifier", 1.00f);
		if (specificModifier != 1.00f)
		{
			Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix: applying new specific attribute maximum modifier that can be reached when recruiting specific-attribute-value-to-use: " + specificModifier);
			result *= specificModifier;
		}
		Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix: applying new maximum modifier attribute can be when recruiting agents: " + attribute + " result: " + result);
		return (int) result;
	}
}