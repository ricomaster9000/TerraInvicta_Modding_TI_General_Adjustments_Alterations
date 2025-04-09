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

			bool isHostileNuke = nuclear && (applyingNation == null || applyingNation.enemies.Contains(__instance.nation));

			double gdpDamage;
			float populationDamage;
			
			// ===== NUCLEAR DAMAGE START =====
			gdpDamage = -1.0 * __instance.nationalGDPShareValue * strength * (0.75f + UnityEngine.Random.Range(0f, 0.5f)) * (isHostileNuke ? 0.7 : 0.2);
			gdpDamage += TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, (float)gdpDamage);
			gdpDamage *= Config.GetValueAsFloat("nuclear_GDP_damage_to_target_nation_multiplier");

			populationDamage = -1f * __instance.populationInMillions * strength * ((0.75f + UnityEngine.Random.Range(0f, 0.5f)) * (isHostileNuke ? 0.25f : 0.025f));
			populationDamage += TIEffectsState.SumEffectsModifiers(Context.NuclearStrikeDamageReduction, __instance, populationDamage);
			//populationDamage *= Config.GetValueAsFloat("nuclear_population_damage_to_target_nation_multiplier");
			__instance.nation.AddToSustainability(__instance.NationalGDPProportion() * strength * (0.075f + UnityEngine.Random.Range(0f, 0.05f)) * (isHostileNuke ? 1f : 0.05f));
			// ===== NUCLEAR DAMAGE END =====

				__instance.nation.ModifyGDP(gdpDamage, TINationState.GDPChangeReason.GDPReason_RegionDamage);
			__instance.ChangePopulation_Millions(populationDamage * Config.GetValueAsFloat("nuclear_population_damage_to_target_nation_multiplier"));

			if (-populationDamage > 0.1f && applyingFaction != null)
			{
				// ===== NUCLEAR DAMAGE START =====
				int multiplier = (isHostileNuke && applyingNation != null && !__instance.nation.alienNation &&
					applyingNation.defensiveWarStates.None((TIWarState x) => x.attackingAlliance.Contains(__instance.nation))) ? 10 : 1;

				applyingFaction.CommitAtrocity((int)Mathf.Clamp(-populationDamage * 10f * multiplier, 1f, 20f),
					TIFactionState.AtrocityCause.MassCasualtiesfromRegionDamage);
				// ===== NUCLEAR DAMAGE END =====
			}

			if (strength >= 0.9f)
			{
				if (nuclear)
				{
					// ===== NUCLEAR DAMAGE START =====
					float globalGDPDamage = __instance.GlobalGDPProportion() * (isHostileNuke ? 1f : 0.2f) * 0.25f;
					globalGDPDamage *= Config.GetValueAsFloat("nuclear_GDP_damage_global_multiplier");
					foreach (TINationState nation in GameStateManager.AllExtantHumanNations())
					{
						nation.GDPPctChange(-1f * (globalGDPDamage + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f),
							TINationState.GDPChangeReason.GDPReason_RegionDamage);
					}

					foreach (TIFactionState faction in GameStateManager.AllHumanFactions())
					{
						foreach (TICouncilorState councilor in faction.councilors)
						{
							if (councilor.homeRegion == __instance)
							{
								TITraitTemplate.ProcessLoyaltyChangeFromTraits(councilor, SpecialTraitRule.LoyaltyLossOnHomeRegionNuked,
									(applyingFaction == faction) ? 2 : 1);
							}
						}
					}
					// ===== NUCLEAR DAMAGE END =====
				}

				if (__instance.coreEconomicRegion && isHostileNuke)
				{
					// ===== NUCLEAR DAMAGE START =====
					__instance.coreEconomicRegion = false;
					GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[] { __instance });
					foreach (TINationState nation in GameStateManager.AllExtantHumanNations())
					{
						float globalGDPDamageBecauseOfCoreRegionModifier = -1f * (0.025f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f);
						globalGDPDamageBecauseOfCoreRegionModifier *= Config.GetValueAsFloat("nuclear_GDP_damage_global_because_of_core_region_multiplier");
						nation.GDPPctChange(globalGDPDamageBecauseOfCoreRegionModifier, TINationState.GDPChangeReason.GDPReason_GlobalCoreEconomicRegionDestroyed);
					}
					// ===== NUCLEAR DAMAGE END =====
				}
				else if (__instance.coreResourceRegion && isHostileNuke)
				{
					// ===== NUCLEAR DAMAGE START =====
					__instance.resourceRegion = false;
					__instance.oilRegion = false;
					GameControl.eventManager.TriggerEvent(new MajorRegionStatusChange(__instance), null, new object[] { __instance });
					foreach (TINationState nation in GameStateManager.AllExtantHumanNations())
					{
						float globalGDPDamageBecauseOfCoreRegionModifier = -1f * (0.015f + (UnityEngine.Random.value + UnityEngine.Random.value) / 100f);
						globalGDPDamageBecauseOfCoreRegionModifier *= Config.GetValueAsFloat("nuclear_GDP_damage_global_because_of_core_region_multiplier");
						nation.GDPPctChange(globalGDPDamageBecauseOfCoreRegionModifier, TINationState.GDPChangeReason.GDPReason_GlobalCoreResourceRegionDestroyed);
					}
					// ===== NUCLEAR DAMAGE END =====
				}
			}

			if (strength >= 0.75f && applyingNation != __instance.nation)
			{
				__instance.DestroySpaceAssets(true);
			}
			else
			{
				__instance.nation.ChangeAnnualSpaceFundingValue(-1f * (__instance.NationalGDPProportion() * __instance.nation.spaceFunding_year * strength * (nuclear ? 0.5f : 0.1f)));

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
				List<TIArmyState> armiesToDamage = __instance.armies.Where(army =>
					(army.homeNation != applyingNation) &&
					(applyingNation == null || !applyingNation.allies.Contains(army.homeNation)) &&
					(applyingFaction == null || army.faction != applyingFaction)).ToList();

				if (applyingFaction == null || !applyingFaction.IsAlienFaction)
				{
					armiesToDamage.AddRange(__instance.MegafaunaArmiesPresent());
				}

				armiesToDamage = armiesToDamage.OrderByDescending(x => x.strength * x.techLevel).ToList();

				for (int j = armiesToDamage.Count - 1; j >= 0; j--)
				{
					if (nuclear && j > 0)
					{
						// ===== NUCLEAR DAMAGE START =====
						float nuclearStrength = strength;

						if (armiesToDamage[j].AlienRegularArmy || (Mathd.d100() < 50 && armiesToDamage[j].techLevel >= 3.8f))
						{
							float techReduction = Mathf.Max(armiesToDamage[j].techLevel - 3.79f, 0f) * UnityEngine.Random.Range(1f, 5f);
							nuclearStrength -= techReduction / 100f;
						}

						nuclearStrength = Mathf.Max(nuclearStrength, 0f);
						nuclearStrength += TIEffectsState.SumEffectsModifiers(Context.ArmyNuclearHardening, armiesToDamage[j].faction, nuclearStrength);

						armiesToDamage[j].TakeDamage(nuclearStrength, applyingFaction, applyingNation);
						// ===== NUCLEAR DAMAGE END =====
					}
					else
					{
						armiesToDamage[j].TakeDamage(strength, applyingFaction, applyingNation);
					}
				}

				if (nuclear)
				{
					// ===== NUCLEAR DAMAGE START =====
					TIArmyState[] others = __instance.armies.Except(armiesToDamage).ToArray();
					for (int k = others.Length - 1; k >= 0; k--)
					{
						others[k].TakeDamage(strength / (48f + UnityEngine.Random.Range(0f, 4f)), applyingFaction, applyingNation);
					}
					// ===== NUCLEAR DAMAGE END =====
				}
			}

			if (includeCouncilors)
			{
				foreach (TICouncilorState councilor in __instance.GetCouncilorsInRegion())
				{
					if (councilor.traits.None(trait => trait.specialTraitRule == SpecialTraitRule.Survivor) &&
						UnityEngine.Random.Range(0f, 2f) < strength)
					{
						TINotificationQueueState.LogCouncilorKilledInAttack(councilor, councilor.location);
						councilor.KillCouncilor(true, applyingFaction);
					}
				}
			}

			if (nuclear)
			{
				// ===== NUCLEAR DAMAGE START =====
				__instance.xenoforming.SetXenoformingLevel(0f);
				TIGlobalValuesState.GlobalValues.TriggerNuclearDetonationEffect(true, applyingNation, __instance, __instance.nation);
				// ===== NUCLEAR DAMAGE END =====
			}
			else if (applyingFaction == null || (!applyingFaction.IsAlienFaction && !applyingFaction.IsAlienProxy))
			{
				__instance.xenoforming.ChangeXenoformingLevel(-(__instance.xenoforming.xenoformingLevel * strength));
			}

			GameControl.eventManager.TriggerEvent(new RegionDamaged(__instance), null, new object[] { __instance });
			GameControl.eventManager.TriggerEvent(new RegionDataUpdated(__instance), null, new object[] { __instance });

			return false;
        }
	    
	    public static void Postfix()
	    {}
    }
}