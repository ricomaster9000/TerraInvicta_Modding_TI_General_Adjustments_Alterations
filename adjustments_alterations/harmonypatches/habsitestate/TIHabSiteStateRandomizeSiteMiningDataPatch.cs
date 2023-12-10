using System;
using System.Collections.Generic;
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

    public static void RandomizeSiteMiningData_Postfix(ref TIHabSiteState __instance)
    {
        Dictionary<FactionResource, float> dictionary = __instance.SampleProductivityPerDay();
        Main.logDebug("ResourceDepletion -> Setting resource info for hab sites");

        foreach (FactionResource key in dictionary.Keys)
        {
            switch (key)
            {
                case FactionResource.Water:
                    String resourceTotalKeyWater = __instance.parentBody.displayName + __instance.detailDisplayName + FactionResource.Water;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyWater))
                    {
                        Main.logDebug("ResourceDepletion -> Remove existing resource info for hab site");
                        HabSiteToTotalResources.Remove(resourceTotalKeyWater);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyWater, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.water_day,
                            __instance.parentBody.habSites.Length,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Volatiles:
                    String resourceTotalKeyVolatiles = __instance.parentBody.displayName + __instance.detailDisplayName + FactionResource.Volatiles;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyVolatiles))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyVolatiles);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyVolatiles, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.volatiles_day,
                            __instance.parentBody.habSites.Length,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Metals:
                    String resourceTotalKeyMetals = __instance.parentBody.displayName + __instance.detailDisplayName + FactionResource.Metals;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyMetals))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyMetals);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyMetals, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.metals_day,
                            __instance.parentBody.habSites.Length,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.NobleMetals:
                    String resourceTotalKeyNobleMetals = __instance.parentBody.displayName + __instance.detailDisplayName + FactionResource.NobleMetals;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyNobleMetals))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyNobleMetals);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyNobleMetals, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.nobles_day,
                            __instance.parentBody.habSites.Length,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
                case FactionResource.Fissiles:
                    String resourceTotalKeyFissiles = __instance.parentBody.displayName + __instance.detailDisplayName + FactionResource.Fissiles;
                    if (HabSiteToTotalResources.ContainsKey(resourceTotalKeyFissiles))
                    {
                        HabSiteToTotalResources.Remove(resourceTotalKeyFissiles);
                    }
                    HabSiteToTotalResources.Add(
                        resourceTotalKeyFissiles, 
                        new ResourceSiteTotalInfo(GenerateResourceTotal(
                            key,
                            __instance.parentBody.meanRadius_km,
                            __instance.fissiles_day,
                            __instance.parentBody.habSites.Length,
                            __instance.parentBody.objectType
                        ))
                    );
                    break;
            }
        }
    }

    private static float GenerateResourceTotal(FactionResource resourceType, double parentBodyMeanRadiusInKm, float dailyGeneratedIncome, int totalHabSites, SpaceObjectType objectType)
    {
        Main.logDebug("ResourceDepletion -> generating total resource for site, " +
                      "resourceType = " + resourceType + Environment.NewLine +
                      "parentBodyMeanRadiusInKm = " + parentBodyMeanRadiusInKm + Environment.NewLine +
                      "dailyGeneratedIncome = " + dailyGeneratedIncome + Environment.NewLine + 
                      "totalHabSites = " + totalHabSites + Environment.NewLine +
                      "objectType = " + objectType);
        Random rnd = new Random();
        float radiusFactor = (float)parentBodyMeanRadiusInKm / totalHabSites;
        int daysToLast = 1095; // 3 years in days

        // Resource multipliers for each celestial body type
        float planetMultiplier = 1.5f;
        float dwarfPlanetMultiplier = 1.4f;
        float asteroidMultiplier = 2.0f;
        float cometMultiplier = 1.8f;
        float planetaryMoonMultiplier = 1.3f;
        float asteroidalMoonMultiplier = 1.7f;

        // Adjust multipliers based on resource type and celestial body type
        float resourceMultiplier = 1.0f; // Default
        switch (objectType)
        {
            case SpaceObjectType.Planet:
                resourceMultiplier = (resourceType == FactionResource.Water || resourceType == FactionResource.Volatiles) ? planetMultiplier : 1.2f;
                break;
            case SpaceObjectType.DwarfPlanet:
                resourceMultiplier = (resourceType == FactionResource.Metals) ? dwarfPlanetMultiplier : 1.1f;
                break;
            case SpaceObjectType.Asteroid:
                resourceMultiplier = (resourceType == FactionResource.NobleMetals) ? asteroidMultiplier : 1.5f;
                break;
            case SpaceObjectType.Comet:
                resourceMultiplier = (resourceType == FactionResource.Volatiles) ? cometMultiplier : 1.0f;
                break;
            case SpaceObjectType.PlanetaryMoon:
                resourceMultiplier = (resourceType == FactionResource.Fissiles) ? planetaryMoonMultiplier : 1.2f;
                break;
            case SpaceObjectType.AsteroidalMoon:
                resourceMultiplier = (resourceType == FactionResource.Metals) ? asteroidalMoonMultiplier : 1.4f;
                break;
        }

        // Adjust ranges based on resource type and ensure it lasts for at least 3 years
        float totalResourceAmount = 0.00f;
        switch (resourceType) 
        {
            case FactionResource.Water:
            case FactionResource.Volatiles:
                totalResourceAmount = rnd.Next((int)(dailyGeneratedIncome * daysToLast * radiusFactor * resourceMultiplier), (int)(dailyGeneratedIncome * 36500 * radiusFactor * resourceMultiplier));
                break;
            case FactionResource.Metals:
                totalResourceAmount = rnd.Next((int)(dailyGeneratedIncome * daysToLast * radiusFactor * resourceMultiplier), (int)(dailyGeneratedIncome * 36500 * radiusFactor * resourceMultiplier));
                break;
            case FactionResource.NobleMetals:
                totalResourceAmount = rnd.Next((int)(dailyGeneratedIncome * daysToLast * radiusFactor * resourceMultiplier), (int)(dailyGeneratedIncome * 36500 * radiusFactor * resourceMultiplier));
                break;
            case FactionResource.Fissiles:
                totalResourceAmount = rnd.Next((int)(dailyGeneratedIncome * daysToLast * radiusFactor * resourceMultiplier), (int)(dailyGeneratedIncome * 36500 * radiusFactor * resourceMultiplier));
                break;
            default:
                throw new Exception("ResourceTypeNotSupported");
        }
        Main.logDebug("ResourceDepletion -> generated total resource amount of " + totalResourceAmount);
        return totalResourceAmount;
    }
}