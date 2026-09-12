using PavonisInteractive.TerraInvicta;
using TI_Augmenter.augmentations.harmonypatches.habsitestate;
using TMPro;

namespace TI_Augmenter.augmentations.harmonypatches.habitatsscreencontroller;

public class ResourceDepletion_PreviewBasePatch
{
    // Runs after the vanilla HabitatsScreenController.PreviewBase() has filled in the hab site productivity
    // panel, and only overwrites the labels of resources whose pool has run dry.
    public static void PreviewBase_Postfix(HabitatsScreenController __instance)
    {
        TIHabState hab = __instance.habToDisplay;
        if (hab == null || hab.habSite == null || !__instance.activePlayer.Prospected(hab.habSite))
        {
            return;
        }

        bool mineActive = hab.mine != null && hab.mine.active;
        string emptyText = mineActive ? "0 - " + Loc.T("UI.TI_Augmenter.EMPTY") : "0";

        SetIfDepleted(hab.habSite, FactionResource.Water, hab.habSite.water_day, __instance.siteWater, emptyText);
        SetIfDepleted(hab.habSite, FactionResource.Volatiles, hab.habSite.volatiles_day, __instance.siteVolatiles, emptyText);
        SetIfDepleted(hab.habSite, FactionResource.Metals, hab.habSite.metals_day, __instance.siteMetals, emptyText);
        SetIfDepleted(hab.habSite, FactionResource.NobleMetals, hab.habSite.nobles_day, __instance.siteNobles, emptyText);
        SetIfDepleted(hab.habSite, FactionResource.Fissiles, hab.habSite.fissiles_day, __instance.siteFissiles, emptyText);
    }

    private static void SetIfDepleted(TIHabSiteState site, FactionResource resource, float dailyRate, TMP_Text label, string emptyText)
    {
        // A resource the site never produced also has an empty pool; only flag ones that actually ran out.
        if (dailyRate <= 0f)
        {
            return;
        }
        string key = site.parentBody.displayName + site.displayName + resource;
        if (TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources.TryGetValue(key, out ResourceSiteTotalInfo info)
            && info.getTotalRemaining() <= 0f)
        {
            Main.logDebug("ResourceDepletion -> showing " + resource + " as depleted for " + key);
            label.SetText(emptyText);
        }
    }
}
