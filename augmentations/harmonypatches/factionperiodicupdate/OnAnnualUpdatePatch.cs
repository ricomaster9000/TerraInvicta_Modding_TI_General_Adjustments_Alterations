using PavonisInteractive.TerraInvicta.Systems.PeriodicUpdates;

namespace TI_Augmenter.augmentations.core.missionrelated
{
    public class OnAnnualUpdatePatch
    {
        public static void OnAnnualUpdate_Postfix(
            ref FactionPeriodicUpdate __instance
        ) {
            Main.logDebug("OnAnnualUpdatePatch - adjusting maximum debt");
        }

    }
}