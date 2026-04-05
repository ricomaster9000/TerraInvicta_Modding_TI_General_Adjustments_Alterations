using System;
using PavonisInteractive.TerraInvicta;
using TI_Augmenter.augmentations.harmonypatches.habsitestate;
using UnityEngine;

namespace TI_Augmenter.augmentations.harmonypatches.factionstate;

public class AddToCurrentResource_Patch
{
    public static void AddToCurrentResourcePostfix(
        ref TIFactionState __instance,
        float amountToAdd,
        FactionResource resourceType,
        bool suppressFactionResourcesUpdatedEvent)
    {
        foreach (TIHabState hab in __instance.habs)
        {
            if (hab.HasMine)
            {
                String resourceTotalKey = hab.habSite.parentBody.displayName + hab.habSite.displayName + resourceType;
                if (TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources.ContainsKey(resourceTotalKey))
                {
                    ResourceSiteTotalInfo resourceSiteInfo = TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources[resourceTotalKey];
                    Main.logDebug("ResourceDepletion -> Subtracting resources of resourceType " + resourceType + " for site " +
                                  hab.habSite.displayName + ", total remaining = " + resourceSiteInfo.getTotalRemaining());
                    float substractAmount = 0.00f;
                    switch (resourceType)
                    {
                        case FactionResource.Fissiles:
                            substractAmount = hab.habSite.fissiles_day;
                            break;
                        case FactionResource.Metals:
                            substractAmount = hab.habSite.metals_day;
                            break;
                        case FactionResource.Water:
                            substractAmount = hab.habSite.water_day;
                            break;
                        case FactionResource.NobleMetals:
                            substractAmount = hab.habSite.nobles_day;
                            break;
                        case FactionResource.Volatiles:
                            substractAmount = hab.habSite.volatiles_day;
                            break;
                    }
                    resourceSiteInfo.substractFromTotalRemaining(substractAmount);
                    Main.logDebug("ResourceDepletion -> Subtracted " + substractAmount + ", total remaining = " + resourceSiteInfo.getTotalRemaining());

                    if (resourceSiteInfo.getTotalRemaining() <= 0 && !resourceSiteInfo.getDepleted())
                    {
                        Main.logDebug("ResourceDepletion -> Vastly reducing hab site potential income for resourceType " + resourceType + ", recycling mode activated");
                        switch (resourceType)
                        {
                            case FactionResource.Fissiles:
                                hab.habSite.fissiles_day = (float)Math.Round(hab.habSite.fissiles_day * 0.08f,3);
                                break;
                            case FactionResource.Metals:
                                hab.habSite.metals_day = (float)Math.Round(hab.habSite.metals_day * 0.25f,3);
                                break;
                            case FactionResource.Water:
                                hab.habSite.water_day = (float)Math.Round(hab.habSite.water_day * 0.20f,3);
                                break;
                            case FactionResource.NobleMetals:
                                hab.habSite.nobles_day = (float)Math.Round(hab.habSite.nobles_day * 0.04f,3);
                                break;
                            case FactionResource.Volatiles:
                                hab.habSite.volatiles_day = (float)Math.Round(hab.habSite.volatiles_day * 0.06f,3);
                                break;
                        }
                        resourceSiteInfo.setDepleted(true);
                        EventManager.LogAndNotifyHabResourcesDepleted(hab.faction,hab);
                    }
                }
            }
        }
    }
}