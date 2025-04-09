using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.councilorstate;

public class TICouncilorState_RandomizeStats_Patch
{
	private static float agent_attributes_all_range_recruit_pool_modifier = 1.00f;
	private static Dictionary<CouncilorAttribute,float> agent_attributes_range_recruit_pool_modifier_specific = new Dictionary<CouncilorAttribute, float>();
	
	public static void setConfigVariables()
	{
		if (Config.isKeySet("agent_attributes_all_range_recruit_pool_modifier"))
		{
			agent_attributes_all_range_recruit_pool_modifier = Config.GetValueAsFloat("agent_attributes_all_range_recruit_pool_modifier");
		}

		foreach (CouncilorAttribute councilorAttribute in GetEnumList<CouncilorAttribute>())
		{
			if (Config.isKeySet("agent_attributes_" + councilorAttribute.ToString().ToLower() + "_range_recruit_pool_modifier"))
			{
				agent_attributes_range_recruit_pool_modifier_specific.Add(councilorAttribute, Config.GetValueAsFloat("agent_attributes_" + councilorAttribute.ToString().ToLower() + "_range_recruit_pool_modifier"));
			}
		}
	}
	
	public static bool Prefix(TICouncilorState __instance)
	{
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
		float result = agent_attributes_all_range_recruit_pool_modifier;
		if (agent_attributes_range_recruit_pool_modifier_specific.ContainsKey(attribute))
		{
			Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix: applying new specific attribute maximum modifier that can be reached when recruiting specific-attribute-value-to-use: " + agent_attributes_range_recruit_pool_modifier_specific[attribute]);
			result *= agent_attributes_range_recruit_pool_modifier_specific[attribute];
		}
		Main.logDebug("TICouncilorState_RandomizeStats_Patch - Prefix: applying new maximum modifier attribute can be when recruiting agents: " + attribute + " result: " + result);
		return (int) result;
	}
	private static List<T> GetEnumList<T>()
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		List<T> list = new List<T>(array);
		return list;
	}
}