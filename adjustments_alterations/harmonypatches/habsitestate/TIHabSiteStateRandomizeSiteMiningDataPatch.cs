using System;
using System.Collections.Generic;
using System.Diagnostics;
using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.habsitestate;


public class ResourceSiteTotalInfo
{
    private float totalInitial;
    private float totalRemaining;
    
    public ResourceSiteTotalInfo(float totalInitial)
    {
        this.totalInitial = totalInitial;
        this.totalRemaining = totalInitial;
    }

    public ResourceSiteTotalInfo(float totalInitial, float totalRemaining)
    {
        this.totalInitial = totalInitial;
        this.totalRemaining = totalRemaining;
    }

    public float getTotalInitial()
    {
        return this.totalInitial;
    }

    public void setTotalInitial(float totalInitial)
    {
        this.totalInitial = totalInitial;
    }

    public float getTotalRemaining()
    {
        return this.totalRemaining;
    }

    public void setTotalRemaining(float totalRemaining)
    {
        this.totalRemaining = totalRemaining;
    }
    
    public void substractFromTotalRemaining(float toSubstract)
    {
        this.totalRemaining -= toSubstract;
    }
}

public class TIHabSiteStateRandomizeSiteMiningDataPatch
{

    public static readonly IDictionary<String, ResourceSiteTotalInfo> HabSiteToTotalResources = new Dictionary<string, ResourceSiteTotalInfo>();
    private static readonly Random random = new Random();
    
    public static void RandomizeSiteMiningData_Postfix(ref TIHabSiteState __instance)
    {
        if (__instance.parentBody.template.habSites == null)
        {
            return;
        }
        Main.logDebug("ResourceDepletion -> Setting resource info for hab sites for body " + __instance.parentBody.displayName);
        foreach (FactionResource key in Enum.GetValues(typeof(FactionResource)))
        {
            Main.logDebug("ResourceDepletion -> Working with " + key);
            switch (key) 
            {
                case FactionResource.Water:
                    String resourceTotalKeyWater = __instance.parentBody.displayName + __instance.displayName + FactionResource.Water;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyWater))
                    {
                        Main.logDebug("ResourceDepletion -> Remove existing resource info for hab site");
                        HabSiteToTotalResources.Remove(resourceTotalKeyWater);
                    }

                    HabSiteToTotalResources.Add(
                        resourceTotalKeyWater, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            __instance,
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.water_day,
                            __instance.parentBody.template.habSites.Count,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Volatiles:
                    String resourceTotalKeyVolatiles = __instance.parentBody.displayName + __instance.displayName + FactionResource.Volatiles;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyVolatiles))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyVolatiles);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyVolatiles, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            __instance,
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.volatiles_day,
                            __instance.parentBody.template.habSites.Count,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Metals:
                    String resourceTotalKeyMetals = __instance.parentBody.displayName + __instance.displayName + FactionResource.Metals;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyMetals))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyMetals);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyMetals, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            __instance,
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.metals_day,
                            __instance.parentBody.template.habSites.Count,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.NobleMetals:
                    String resourceTotalKeyNobleMetals = __instance.parentBody.displayName + __instance.displayName + FactionResource.NobleMetals;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyNobleMetals))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyNobleMetals);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyNobleMetals, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            __instance,
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.nobles_day,
                            __instance.parentBody.template.habSites.Count,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Fissiles:
                    String resourceTotalKeyFissiles = __instance.parentBody.displayName + __instance.displayName + FactionResource.Fissiles;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyFissiles))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyFissiles);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyFissiles, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            __instance,
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.fissiles_day,
                            __instance.parentBody.template.habSites.Count,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
            }
        }
    }

    private static float GenerateResourceTotal(TIHabSiteState habSite, FactionResource resourceType, double parentBodyMeanRadiusInKm, float dailyGeneratedIncome, int totalHabSites, SpaceObjectType objectType)
    {
        if (dailyGeneratedIncome <= 0.00f)
        {
            return 0;
        }
        Main.logDebug("ResourceDepletion -> generating total resource for site " + habSite.displayName + ", " +
                      "resourceType = " + resourceType + Environment.NewLine +
                      "parentBodyMeanRadiusInKm = " + parentBodyMeanRadiusInKm + Environment.NewLine +
                      "dailyGeneratedIncome = " + dailyGeneratedIncome + Environment.NewLine + 
                      "totalHabSites = " + totalHabSites + Environment.NewLine +
                      "objectType = " + objectType);
        int minDaysToLast = objectType switch
        {
            SpaceObjectType.Planet => 365*50,
            SpaceObjectType.DwarfPlanet => 365*20,
            SpaceObjectType.PlanetaryMoon => 365*20,
            SpaceObjectType.AsteroidalMoon => 365*2,
            SpaceObjectType.Asteroid => 365*2,
            SpaceObjectType.Comet => (int)(365*1.33333),
            _ => 365*2
        };
        int maxDaysToLast = minDaysToLast * 3;

        double totalResourceAmount = dailyGeneratedIncome*random.Next(minDaysToLast,maxDaysToLast);

        if ((parentBodyMeanRadiusInKm / 250) > totalHabSites)
        {
            Main.logDebug("ResourceDepletion -> total hab sites is less than sensible for celestial body " + habSite.parentBody.displayName + " size, it should be more, applying bonus modifier");
            totalResourceAmount *= 1+(1-(totalHabSites / (parentBodyMeanRadiusInKm / 250)));
        }

        Main.logDebug("ResourceDepletion -> hab site " + habSite.displayName + " for resource " + 
                      resourceType + " will last " + (totalResourceAmount / dailyGeneratedIncome)/365 + " years");
        Main.logDebug("ResourceDepletion -> generated total resource amount of " + totalResourceAmount);
        return (float)totalResourceAmount;
    }
}