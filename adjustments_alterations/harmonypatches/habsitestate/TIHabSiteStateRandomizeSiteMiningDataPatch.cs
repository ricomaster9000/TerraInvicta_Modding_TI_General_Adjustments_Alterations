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

    private static float GenerateResourceTotal(FactionResource resourceType, double parentBodyMeanRadiusInKm, float dailyGeneratedIncome, int totalHabSites, SpaceObjectType objectType)
    {
        if (dailyGeneratedIncome <= 0.00f)
        {
            return 0;
        }
        Main.logDebug("ResourceDepletion -> generating total resource for site, " +
                      "resourceType = " + resourceType + Environment.NewLine +
                      "parentBodyMeanRadiusInKm = " + parentBodyMeanRadiusInKm + Environment.NewLine +
                      "dailyGeneratedIncome = " + dailyGeneratedIncome + Environment.NewLine + 
                      "totalHabSites = " + totalHabSites + Environment.NewLine +
                      "objectType = " + objectType);
        Random rnd = new Random();
        float mineableRatio = 0f;
        switch(objectType)
        {
            case SpaceObjectType.Planet:
                mineableRatio = 0.075f;
            break;
            case SpaceObjectType.DwarfPlanet:
                mineableRatio = 0.10f;
            break;
            case SpaceObjectType.Asteroid:
            case SpaceObjectType.AsteroidalMoon:
                mineableRatio = 0.30f;
            break;
            case SpaceObjectType.Comet:
                mineableRatio = 0.35f;
            break;
            case SpaceObjectType.PlanetaryMoon:
                mineableRatio = 0.090f;
            break;
        }

        switch (objectType)
        {
            case SpaceObjectType.Planet:
                mineableRatio += (resourceType == FactionResource.Water || resourceType == FactionResource.Volatiles) ? mineableRatio+0.05f : mineableRatio;
                break;
            case SpaceObjectType.DwarfPlanet:
                mineableRatio += (resourceType == FactionResource.Metals) ? mineableRatio+0.075f : mineableRatio;
                break;
            case SpaceObjectType.Asteroid:
                mineableRatio += (resourceType == FactionResource.NobleMetals) ? mineableRatio+0.20f : mineableRatio;
                break;
            case SpaceObjectType.Comet:
                mineableRatio += (resourceType == FactionResource.Volatiles) ? mineableRatio+0.10f : mineableRatio;
                break;
            case SpaceObjectType.PlanetaryMoon:
                mineableRatio += (resourceType == FactionResource.Fissiles) ? mineableRatio+0.03F : mineableRatio;
                break;
            case SpaceObjectType.AsteroidalMoon:
                mineableRatio += (resourceType == FactionResource.Metals) ? mineableRatio+0.175f : mineableRatio;
                break;
        }
        int monthsToLast = objectType switch
        {
            SpaceObjectType.Planet => 600,
            SpaceObjectType.DwarfPlanet => 240,
            SpaceObjectType.PlanetaryMoon => 240,
            SpaceObjectType.AsteroidalMoon => 24,
            SpaceObjectType.Asteroid => 24,
            SpaceObjectType.Comet => 16,
            _ => 24
        };
        

        // Adjust ranges based on resource type and ensure it lasts for at least 3 years
        float totalResourceAmount = 0.00f;
        int minTotalResourceAmount = (int)((parentBodyMeanRadiusInKm*2)*mineableRatio*((parentBodyMeanRadiusInKm/1000)/totalHabSites));

        if (minTotalResourceAmount < dailyGeneratedIncome * monthsToLast)
        {
            Main.logDebug("ResourceDepletion -> min possible resources won't last " + monthsToLast + " months");
        }
        
        int maxTotalResourceAmount = minTotalResourceAmount*2;
        switch (resourceType) 
        {
            case FactionResource.Water:
            case FactionResource.Volatiles:
            case FactionResource.Metals:
            case FactionResource.NobleMetals:
            case FactionResource.Fissiles:
                totalResourceAmount = rnd.Next(minTotalResourceAmount, maxTotalResourceAmount);
                break;
            default:
                throw new Exception("ResourceTypeNotSupported");
        }
        Main.logDebug("ResourceDepletion -> generated total resource amount of " + totalResourceAmount);
        return totalResourceAmount;
    }

    private static int ReturnGreatest(int a, int b)
    {
        return a > b ? a : b;
    }
}