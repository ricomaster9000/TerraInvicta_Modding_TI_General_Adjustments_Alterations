﻿using System;
using System.Collections.Generic;
using System.Linq;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.regionstate
{
	class ApplyDamageToRegionPatch
    {
	    public static bool Prefix(TIRegionState __instance, float strength, TIFactionState applyingCouncilState, TINationState applyingNation, bool includeArmies, bool includeCouncilors, bool forceAttackSpaceAssets, bool nuclear)
        {
	        if (strength > 0f && nuclear)
			{
				__instance.nation.ChangeAnnualSpaceFundingValue(-1f * (__instance.NationalGDPProportion() * __instance.nation.spaceFunding_year * strength * (nuclear ? 0.5f : 0.1f)));
				double gdpChangeToTargetNation;
				float populationChangeToTargetNation;
				gdpChangeToTargetNation = -Config.GetValueAsFloat("nuclear_GDP_damage_to_target_nation_factor") * __instance.nationalGDPShareValue * strength * (0.75f + UnityEngine.Random.Range(0f, 0.5f)) * (applyingNation == null || applyingNation.enemies.Contains(__instance.nation) ? 0.7 : 0.20000000298023224);
				gdpChangeToTargetNation += TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, (float)gdpChangeToTargetNation);
					
				populationChangeToTargetNation = -Config.GetValueAsFloat("nuclear_population_damage_to_target_nation_factor") * __instance.populationInMillions * strength * (UnityEngine.Random.Range(0f, 0.5f) * (applyingNation == null || applyingNation.enemies.Contains(__instance.nation) ? 0.25f : 0.025f));
				populationChangeToTargetNation += TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, populationChangeToTargetNation);
				
				__instance.nation.ModifyGDP(gdpChangeToTargetNation);
				__instance.ChangePopulation_Millions(populationChangeToTargetNation);
				float otherNationGdpDamageToApply = 0.00f;
				if (strength >= 0.9f)
				{
					if (applyingCouncilState != null)
					{
						applyingCouncilState.CommitAtrocity(Mathf.Clamp((int)populationChangeToTargetNation, 1, 10));
					}
					if (__instance.populationInMillions >= 1f)
					{
						otherNationGdpDamageToApply = (applyingNation != __instance.nation) ? 0.005f : 0.001f;
						otherNationGdpDamageToApply = -Config.GetValueAsFloat("nuclear_GDP_damage_to_all_nations_if_above_certain_million_deaths_factor") * (otherNationGdpDamageToApply + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f);
						foreach (TINationState tinationState in GameStateManager.AllExtantHumanNations())
						{
							tinationState.GDPPctChange(otherNationGdpDamageToApply);
						}
					}
					if (__instance.coreEconomicRegion && applyingNation != __instance.nation)
					{
						__instance.coreEconomicRegion = false;
						GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[]
						{
							__instance
						});
						foreach (TINationState tinationState2 in GameStateManager.AllExtantHumanNations())
						{
							tinationState2.GDPPctChange(-Config.GetValueAsFloat("nuclear_GDP_damage_to_all_nations_if_above_certain_million_deaths_factor_2") * (0.025f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f));
						}
					}
					if (__instance.resourceRegion && applyingNation != __instance.nation)
					{
						__instance.resourceRegion = false;
						GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[]
						{
							__instance
						});
						foreach (TINationState tinationState3 in GameStateManager.AllExtantHumanNations())
						{
							tinationState3.GDPPctChange(-Config.GetValueAsFloat("nuclear_GDP_damage_to_all_nations_if_above_certain_million_deaths_factor_2") * (0.015f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f));
						}
					}
					foreach (object obj in Enum.GetValues(typeof(PriorityType)))
					{
						PriorityType priorityType = (PriorityType)obj;
						if (priorityType != PriorityType.Unity && priorityType != PriorityType.Spoils)
						{
							__instance.nation.ModifyAccumulatedInvestment(priorityType, strength, true, false);
						}
					}
				}
				if (UnityEngine.Random.value < strength)
				{
					__instance.nation.ModifyAccumulatedInvestment(__instance.nation.GetRandomPriorityToDamage(), __instance.colonyRegion ? (strength * 0.5f) : strength, true, true);
				}
				if (strength >= 0.75f && applyingNation != __instance.nation)
				{
					__instance.DestroySpaceAssets(true);
				}
				else
				{
					if (__instance.boostPerMonth_dekatons > 0f && (UnityEngine.Random.value < strength || forceAttackSpaceAssets))
					{
						__instance.ChangeSpaceFacilityValue(SpaceFacilityType.launchFacility, -(__instance.boostPerYear_dekatons * strength), false, true);
					}
					if (__instance.missionControl > 0 && (UnityEngine.Random.value < strength || forceAttackSpaceAssets))
					{
						__instance.ChangeSpaceFacilityValue(SpaceFacilityType.missionControlFacility, -1f, false, true);
					}
					if (__instance.antiSpaceDefenses && (UnityEngine.Random.value < strength || forceAttackSpaceAssets))
					{
						__instance.ChangeSpaceFacilityValue(SpaceFacilityType.spaceDefenseFacility, 0f, false, true);
					}
				}
				if (includeArmies)
				{
					List<TIArmyState> list = __instance.armies.Where(delegate(TIArmyState army)
					{
						if (army.homeNation != applyingNation)
						{
							TINationState applyingNation2 = applyingNation;
							if (applyingNation2 == null || !applyingNation2.allies.Contains(army.homeNation))
							{
								return army.faction != applyingCouncilState || applyingCouncilState == null;
							}
						}
						return false;
					}).ToList<TIArmyState>();
					TIFactionState applyingCouncilState2 = applyingCouncilState;
					if (applyingCouncilState2 == null || !applyingCouncilState2.IsAlienFaction)
					{
						list.AddRange(__instance.MegafaunaArmiesPresent());
					}
					list = (from x in list
					orderby x.strength * x.techLevel descending
					select x).ToList<TIArmyState>();
					for (int i = list.Count - 1; i >= 0; i--)
					{
						if (nuclear && i > 0)
						{
							float num4 = strength;
							if (Mathd.d100() < 50 && list[i].techLevel >= 3.8f)
							{
								float num5 = Mathf.Max(list[i].techLevel - 3.79f, 0f) * UnityEngine.Random.Range(1f, 5f);
								num4 -= num5 / 100f;
							}
							num4 += TIEffectsState.SumEffectsModifiers(Context.ArmyNuclearHardening, list[i].faction, num4);
							list[i].TakeDamage(num4, applyingCouncilState, applyingNation);
						}
						else
						{
							list[i].TakeDamage(strength, applyingCouncilState, applyingNation);
						}
					}
					if (nuclear)
					{
						TIArmyState[] array = __instance.armies.Except(list).ToArray<TIArmyState>();
						for (int j = array.Length - 1; j >= 0; j--)
						{
							array[j].TakeDamage(strength / (48f + UnityEngine.Random.Range(0f, 4f)), applyingCouncilState, applyingNation);
						}
					}
				}
				if (includeCouncilors)
				{
					foreach (TICouncilorState ticouncilorState in __instance.GetCouncilorsInRegion())
					{
						if (ticouncilorState.traits.None((TITraitTemplate x) => x.specialTraitRule == SpecialTraitRule.Survivor) && UnityEngine.Random.Range(0f, 2f) < strength)
						{
							TINotificationQueueState.LogCouncilorKilledInAttack(ticouncilorState, ticouncilorState.location);
							ticouncilorState.KillCouncilor(true,applyingCouncilState);
						}
					}
				}
				if (nuclear)
				{
					__instance.xenoforming.SetXenoformingLevel(0f);
					TIGlobalValuesState.GlobalValues.TriggerNuclearDetonationEffect(true, applyingNation, __instance, __instance.nation);
				}
				GameControl.eventManager.TriggerEvent(new RegionDamaged(__instance), null, new object[]
				{
					__instance
				});
				GameControl.eventManager.TriggerEvent(new RegionDataUpdated(__instance), null, new object[]
				{
					__instance
				});
				return false;
			}
	        else
	        {
		        return true;
	        }
        }
	    
	    public static void Postfix()
	    {}
    }
}