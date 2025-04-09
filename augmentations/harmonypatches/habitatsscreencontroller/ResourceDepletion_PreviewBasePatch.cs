using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TI_Augmenter.augmentations.harmonypatches.habsitestate;

public class ResourceDepletion_DetailDisplayNamePatch
{
    public static void PreviewBasePostfix(ref HabitatsScreenController __instance)
    { 
        // === Harmony-based access for private fields ===
        var noHabSelected = (GameObject)AccessTools.Field(typeof(HabitatsScreenController), "noHabSelected").GetValue(__instance);
        var baseDisplayCanvas = (Canvas)AccessTools.Field(typeof(HabitatsScreenController), "baseDisplayCanvas").GetValue(__instance);
        var baseSurfaceImage = (Image)AccessTools.Field(typeof(HabitatsScreenController), "baseSurfaceImage").GetValue(__instance);
        var stationDisplayCanvas = (Canvas)AccessTools.Field(typeof(HabitatsScreenController), "stationDisplayCanvas").GetValue(__instance);
        var baseCellDictionary = (Dictionary<string, BaseGridCell>)AccessTools.Field(typeof(HabitatsScreenController), "baseCellDictionary").GetValue(__instance);
        var habSiteProductivityPanel = (GameObject)AccessTools.Field(typeof(HabitatsScreenController), "habSiteProductivityPanel").GetValue(__instance);

        // === Harmony-based access for private methods ===
        var setModulesInteractable = AccessTools.Method(typeof(HabitatsScreenController), "SetModulesInteractable");
        var previewBaseModule = AccessTools.Method(typeof(HabitatsScreenController), "PreviewBaseModule");
        var onHabZoomSliderSet = AccessTools.Method(typeof(HabitatsScreenController), "OnHabZoomSliderSet");
        var baseModuleResourceNameGet = AccessTools.Method(typeof(HabitatsScreenController), "BaseModuleResourceName");

        // === Begin logic ===
        noHabSelected.SetActive(false);
        baseDisplayCanvas.enabled = true;
        GameControl.assetLoader.LoadAssetForImageAssignment(__instance.habToDisplay.habSite.template.backgroundPath, baseSurfaceImage);
        baseSurfaceImage.enabled = true;
        stationDisplayCanvas.enabled = false;

        setModulesInteractable.Invoke(__instance, new object[] { __instance.habToDisplay.habType, null });

        for (int i = 0; i < 5; i++)
        {
            if (__instance.habToDisplay.sectors[i].faction == null)
            {
                for (int j = 0; j < 4; j++)
                {
                    previewBaseModule.Invoke(__instance, new object[] { "blank", i, j, __instance.habToDisplay.IsAlien() });
                }
            }
            else
            {
                TISectorState sector = __instance.habToDisplay.sectors[i];

                for (int num = 0; num < 5 && sector.habModules.Count > num; num++)
                {
                    previewBaseModule.Invoke(__instance, new object[] {
                        baseModuleResourceNameGet.Invoke(__instance,new object[]{sector.habModules[num], i, num}),
                        i, num, __instance.activePlayer == sector.faction
                    });
                }
            }
        }

        string[] connKeys = { "C_42", "C_16", "C_36", "C_46", "C_56", "C_76" };
        foreach (var key in connKeys)
        {
            bool connected = !(bool)__instance.habToDisplay.connStruct.GetType().GetField(key).GetValue(__instance.habToDisplay.connStruct);
            baseCellDictionary[key].UpdateConnections(connected);
            baseCellDictionary[key].SetAllConnectionSprites(__instance.habToDisplay.IsAlien());
        }

        // === Depletion-aware resource display ===
        if (__instance.activePlayer.Prospected(__instance.habToDisplay.habSite))
        {
            string siteId = __instance.habToDisplay.habSite.parentBody.displayName + __instance.habToDisplay.habSite.displayName;
            var resourceDict = TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources;

            float GetRemaining(FactionResource res) => resourceDict.ContainsKey(siteId + res) ? resourceDict[siteId + res].getTotalRemaining() : 0f;
            bool IsDepleted(FactionResource res) => GetRemaining(res) <= 0f;

            if (__instance.habToDisplay.mine != null && __instance.habToDisplay.mine.active)
            {
                string emptyText = "0 - " + Loc.T("UI.TI_General_Adjustments_Alterations.EMPTY");
                
                __instance.siteWater.SetText(IsDepleted(FactionResource.Water) ? emptyText:
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.mine.moduleTemplate.GetMiningIncome_Month(__instance.activePlayer, __instance.habToDisplay.habSite, FactionResource.Water), 7, 0, true, false));

                __instance.siteVolatiles.SetText(IsDepleted(FactionResource.Volatiles) ? emptyText :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.mine.moduleTemplate.GetMiningIncome_Month(__instance.activePlayer, __instance.habToDisplay.habSite, FactionResource.Volatiles), 7, 0, true, false));

                __instance.siteMetals.SetText(IsDepleted(FactionResource.Metals) ? emptyText :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.mine.moduleTemplate.GetMiningIncome_Month(__instance.activePlayer, __instance.habToDisplay.habSite, FactionResource.Metals), 7, 0, true, false));

                __instance.siteNobles.SetText(IsDepleted(FactionResource.NobleMetals) ? emptyText :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.mine.moduleTemplate.GetMiningIncome_Month(__instance.activePlayer, __instance.habToDisplay.habSite, FactionResource.NobleMetals), 7, 0, true, false));

                __instance.siteFissiles.SetText(IsDepleted(FactionResource.Fissiles) ? emptyText :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.mine.moduleTemplate.GetMiningIncome_Month(__instance.activePlayer, __instance.habToDisplay.habSite, FactionResource.Fissiles), 7, 0, true, false));
            }
            else
            {
                __instance.siteWater.SetText(IsDepleted(FactionResource.Water) ? "0" :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.habSite.GetMonthlyProduction(FactionResource.Water), 7, 0, true, false));

                __instance.siteVolatiles.SetText(IsDepleted(FactionResource.Volatiles) ? "0" :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.habSite.GetMonthlyProduction(FactionResource.Volatiles), 7, 0, true, false));

                __instance.siteMetals.SetText(IsDepleted(FactionResource.Metals) ? "0" :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.habSite.GetMonthlyProduction(FactionResource.Metals), 7, 0, true, false));

                __instance.siteNobles.SetText(IsDepleted(FactionResource.NobleMetals) ? "0" :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.habSite.GetMonthlyProduction(FactionResource.NobleMetals), 7, 0, true, false));

                __instance.siteFissiles.SetText(IsDepleted(FactionResource.Fissiles) ? "0" :
                    TIUtilities.FormatSmallNumber(__instance.habToDisplay.habSite.GetMonthlyProduction(FactionResource.Fissiles), 7, 0, true, false));
            }
        }
        else
        {
            __instance.siteWater.SetText(HabSiteController.GetInlineResourceOutputIcon(FactionResource.Water, __instance.habToDisplay.habSite.miningProfile, __instance.habToDisplay.habSite.GetHabSiteExpectedProductivity_day(FactionResource.Water)));
            __instance.siteVolatiles.SetText(HabSiteController.GetInlineResourceOutputIcon(FactionResource.Volatiles, __instance.habToDisplay.habSite.miningProfile, __instance.habToDisplay.habSite.GetHabSiteExpectedProductivity_day(FactionResource.Volatiles)));
            __instance.siteMetals.SetText(HabSiteController.GetInlineResourceOutputIcon(FactionResource.Metals, __instance.habToDisplay.habSite.miningProfile, __instance.habToDisplay.habSite.GetHabSiteExpectedProductivity_day(FactionResource.Metals)));
            __instance.siteNobles.SetText(HabSiteController.GetInlineResourceOutputIcon(FactionResource.NobleMetals, __instance.habToDisplay.habSite.miningProfile, __instance.habToDisplay.habSite.GetHabSiteExpectedProductivity_day(FactionResource.NobleMetals)));
            __instance.siteFissiles.SetText(HabSiteController.GetInlineResourceOutputIcon(FactionResource.Fissiles, __instance.habToDisplay.habSite.miningProfile, __instance.habToDisplay.habSite.GetHabSiteExpectedProductivity_day(FactionResource.Fissiles)));
        }

        habSiteProductivityPanel.SetActive(true);
        onHabZoomSliderSet.Invoke(__instance, null);
    }

}