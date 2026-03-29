using System;
using System.Collections.Generic;
using System.Linq;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace TI_Augmenter.augmentations.harmonypatches.regionstate
{
	class ApplyDamageToRegionPatch
    {
	    public static bool Prefix(TIRegionState __instance, float strength, TIFactionState applyingFaction, TINationState applyingNation, bool includeArmies, bool includeCouncilors, bool forceAttackSpaceAssets, bool nuclear)
        {
	        if (strength <= 0f || !nuclear) return true;

			bool flag = applyingNation == null || applyingNation.enemies.Contains(__instance.nation);
			double num;
			float num2;
			num = -1.0 * __instance.nationalGDPShareValue * (double)strength * (double)(0.75f + UnityEngine.Random.Range(0f, 0.5f)) * (flag ? 0.7 : 0.20000000298023224);
	        num += (double)TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, (float)num, null);
	        num *= Config.GetValueAsFloat("nuclear_GDP_damage_to_target_nation_multiplier");
	        
	        num2 = -1f * __instance.populationInMillions * strength * ((0.75f + UnityEngine.Random.Range(0f, 0.5f)) * (flag ? 0.25f : 0.025f));
	        num2 += TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, num2, null);
	        num2 *= Config.GetValueAsFloat("nuclear_population_damage_to_target_nation_multiplier");
	        
	        
	        __instance.nation.AddToSustainability(__instance.NationalGDPProportion() * strength * (0.075f + UnityEngine.Random.Range(0f, 0.05f)) * (flag ? 1f : 0.05f));
	        __instance.nation.ModifyGDP(num, TINationState.GDPChangeReason.GDPReason_RegionDamage);
			__instance.ChangePopulation_Millions(num2, true);
			if (applyingFaction != null)
			{
				int num3 = (flag && applyingNation != null && !__instance.nation.alienNation && applyingNation.defensiveWarStates.None((TIWarState x) => x.attackingAlliance.Contains(__instance.nation))) ? 10 : 1;
				applyingFaction.CommitAtrocity((int)Mathf.Clamp(-num2 * 10f * (float)num3, 1f, 20f), TIFactionState.AtrocityCause.MassCasualtiesfromRegionDamage, false, 0.333f);
			}
			if (strength >= 0.9f)
			{
				float num4 = __instance.GlobalGDPProportion() * (flag ? 1f : 0.2f) * 0.25f;
				num4 *= Config.GetValueAsFloat("nuclear_GDP_damage_global_multiplier");
				
				foreach (TINationState tinationState in GameStateManager.AllExtantHumanNations())
				{
					tinationState.GDPPctChange(-1f * (num4 + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f), TINationState.GDPChangeReason.GDPReason_RegionDamage);
				}
				foreach (TIFactionState tifactionState in GameStateManager.AllHumanFactions())
				{
					foreach (TICouncilorState ticouncilorState in tifactionState.councilors)
					{
						if (ticouncilorState.homeRegion == __instance)
						{
							TITraitTemplate.ProcessLoyaltyChangeFromTraits(ticouncilorState, SpecialTraitRule.LoyaltyLossOnHomeRegionNuked, (applyingFaction == tifactionState) ? 2 : 1);
						}
					}
				}
				if (flag)
				{
					if (__instance.coreEconomicRegion)
					{
						__instance.coreEconomicRegion = false;
						GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[]
						{
							__instance
						});
						using (IEnumerator<TINationState> enumerator = GameStateManager.AllExtantHumanNations().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								TINationState tinationState2 = enumerator.Current;
								float globalGDPDamageBecauseOfCoreRegionModifier = -1f * (0.025f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f);
								globalGDPDamageBecauseOfCoreRegionModifier *= Config.GetValueAsFloat("nuclear_GDP_damage_global_because_of_core_region_multiplier");
								tinationState2.GDPPctChange(globalGDPDamageBecauseOfCoreRegionModifier, TINationState.GDPChangeReason.GDPReason_GlobalCoreEconomicRegionDestroyed);
							}
							goto IL_457;
						}
					}
					if (__instance.coreResourceRegion)
					{
						__instance.resourceRegion = false;
						__instance.oilRegion = false;
						GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[]
						{
							__instance
						});
						foreach (TINationState tinationState3 in GameStateManager.AllExtantHumanNations())
						{
							float globalGDPDamageBecauseOfCoreRegionModifier = -1f * (0.015f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f);
							globalGDPDamageBecauseOfCoreRegionModifier *= Config.GetValueAsFloat("nuclear_GDP_damage_global_because_of_core_region_multiplier");
							tinationState3.GDPPctChange(globalGDPDamageBecauseOfCoreRegionModifier, TINationState.GDPChangeReason.GDPReason_GlobalCoreResourceRegionDestroyed);
						}
					}
					IL_457:
					__instance.accumulatedCoreEconomyRegionTriggers = 0;
					__instance.accumulatedCoreMiningRegionTriggers = 0;
					__instance.accumulatedCoreOilRegionTriggers = 0;
					__instance.accumulatedDecolonizeTriggers = 0;
					__instance.accumulatedDecontaminateTriggers = 0;
				}
				foreach (PriorityType priorityType in Enums.PriorityTypes)
				{
					if (priorityType - PriorityType.Unity > 1 && priorityType != PriorityType.Spoils)
					{
						__instance.nation.ModifyAccumulatedInvestment(priorityType, 1f - strength, true, false);
					}
				}
				__instance.nation.SetDataDirty();
			}
			else if (UnityEngine.Random.value < strength * 5f)
			{
				__instance.nation.ModifyAccumulatedInvestment(__instance.nation.GetRandomPriorityToDamage(), __instance.colonyRegion ? (1f - strength * 0.5f) : (1f - strength), true, true);
			}
			if (applyingNation != __instance.nation)
			{
				__instance.nation.ChangeAnnualSpaceFundingValue(-1f * (__instance.NationalGDPProportion() * __instance.nation.spaceFunding_year * strength * 0.5f));
				if (strength >= 0.75f)
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
			}
			if (includeArmies)
			{
				List<TIArmyState> list = __instance.armies.Where(delegate(TIArmyState army)
				{
					if (army.homeNation != applyingNation && !army.atSea)
					{
						TINationState applyingNation2 = applyingNation;
						if (applyingNation2 == null || !applyingNation2.allies.Contains(army.homeNation))
						{
							return army.faction != applyingFaction || applyingFaction == null;
						}
					}
					return false;
				}).ToList<TIArmyState>();
				TIFactionState applyingFaction2 = applyingFaction;
				if (applyingFaction2 == null || !applyingFaction2.IsAlienFaction)
				{
					list.AddRange(__instance.MegafaunaArmiesPresent());
				}
				list = (from x in list
				orderby x.strength * x.techLevel descending
				select x).ToList<TIArmyState>();
				for (int j = list.Count - 1; j >= 0; j--)
				{
					if (j > 0)
					{
						float num5 = strength;
						if (list[j].AlienRegularArmy || (Mathd.d100() < 50 && list[j].techLevel >= 3.8f))
						{
							float num6 = Mathf.Max(list[j].techLevel - 3.79f, 0f) * UnityEngine.Random.Range(1f, 5f);
							num5 -= num6 / 100f;
						}
						num5 = Mathf.Max(num5, 0f);
						num5 += TIEffectsState.SumEffectsModifiers(Context.ArmyNuclearHardening, list[j].faction, num5, null);
						list[j].TakeDamage(num5, applyingFaction, applyingNation, false);
					}
					else
					{
						list[j].TakeDamage(strength, applyingFaction, applyingNation, !nuclear);
					}
				}
				TIArmyState[] array2 = (from x in __instance.armies
				where !x.atSea
				select x).Except(list).ToArray<TIArmyState>();
				for (int k = array2.Length - 1; k >= 0; k--)
				{
					array2[k].TakeDamage(strength / (48f + UnityEngine.Random.Range(0f, 4f)), applyingFaction, applyingNation, false);
				}
			}
			if (includeCouncilors)
			{
				foreach (TICouncilorState ticouncilorState2 in __instance.GetCouncilorsInRegion())
				{
					if (ticouncilorState2.traits.None((TITraitTemplate x) => x.specialTraitRule == SpecialTraitRule.Survivor) && UnityEngine.Random.Range(0f, 2f) < strength)
					{
						TINotificationQueueState.LogCouncilorKilledInAttack(ticouncilorState2, ticouncilorState2.location);
						ticouncilorState2.KillCouncilor(true, applyingFaction);
					}
				}
			}
			__instance.xenoforming.SetXenoformingLevel(0f);
			TIGlobalValuesState.GlobalValues.TriggerNuclearDetonationEffect(true, applyingNation, __instance, __instance.nation);
			GameControl.eventManager.TriggerEvent(new RegionNuked(__instance), null, new object[]
			{
				__instance
			});
			GameControl.eventManager.TriggerEvent(new RegionDamaged(__instance), null, new object[] { __instance });
			GameControl.eventManager.TriggerEvent(new RegionDataUpdated(__instance), null, new object[] { __instance });
			return false;
        }
	    
	    public static void Postfix()
	    {}
    }
}