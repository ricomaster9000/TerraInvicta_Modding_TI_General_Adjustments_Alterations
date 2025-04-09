using System.Collections.Generic;
using System.Linq;
using PavonisInteractive.TerraInvicta;

namespace TI_Augmenter.augmentations.harmonypatches.councilorstate;

// NOT USED
public class TIFactionState_GenerateRecruitableCouncilors_Patch
{
	public static void Postfix(TIFactionState __instance, bool campaignStart)
		{
			bool result = false;
			if (__instance.availableCouncilors.Count > 1 && !campaignStart && __instance.IsActiveHumanFaction)
			{
				for (int i = __instance.availableCouncilors.Count - 1; i >= 0; i--)
				{
					if (UnityEngine.Random.value * 100f < (float)__instance.availableCouncilors[i].age)
					{
						TICouncilorState ticouncilorState = __instance.availableCouncilors[i];
						__instance.availableCouncilors.Remove(ticouncilorState);
						if (ticouncilorState.template.randomized)
						{
							ticouncilorState.ArchiveState();
							GameStateManager.RemoveGameState<TICouncilorState>(ticouncilorState.ID, false);
						}
					}
				}
			}
			if (__instance.maxRecruitableCandidates > 0)
			{
				int num = __instance.IsActiveHumanFaction ? UnityEngine.Random.Range(-2, 2) : 0;
				for (int j = __instance.availableCouncilors.Count; j <= __instance.maxRecruitableCandidates + num; j++)
				{
					List<TICouncilorState> list = new List<TICouncilorState>();
					foreach (TICouncilorState ticouncilorState2 in GameStateManager.IterateByClass<TICouncilorState>(false))
					{
						if (!ticouncilorState2.everBeenAvailable && !ticouncilorState2.template.debugOnly && string.IsNullOrEmpty(ticouncilorState2.template.debugStartingCouncil) && !ticouncilorState2.template.randomized && ticouncilorState2.template.allowedIdeologies.ToList().Contains(__instance.ideology.ideology))
						{
							list.Add(ticouncilorState2);
						}
					}
					if (UnityEngine.Random.value > TemplateManager.global.chanceCouncilorTemplate || list.Count == 0)
					{
						TICouncilorState ticouncilorState3 = GameStateManager.CreateNewGameState<TICouncilorState>();
						if (__instance.IsAlienFaction)
						{
							ticouncilorState3.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedAlienCouncilor2", false));
						}
						else
						{
							ticouncilorState3.InitWithTemplate(TemplateManager.Find<TICouncilorTemplate>("randomizedCouncilor1", false));
						}
						ticouncilorState3.NewCharacterGeneration(null, null, (__instance.IsAlienFaction || campaignStart) ? null : __instance);
						__instance.availableCouncilors.Add(ticouncilorState3);
						result = true;
					}
					else
					{
						int index = UnityEngine.Random.Range(0, list.Count);
						__instance.availableCouncilors.Add(list[index]);
						result = true;
						list[index].everBeenAvailable = true;
					}
				}
			}
		}
}