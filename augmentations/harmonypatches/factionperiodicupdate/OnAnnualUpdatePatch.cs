using PavonisInteractive.TerraInvicta.Systems.PeriodicUpdates;

namespace TI_Augmenter.augmentations.core.missionrelated
{
    public class OnAnnualUpdatePatch
    {

        public static void setConfigVariables()
        {
            //agent_follow_up_failures_decrease_chances_of_success_factor = Config.GetValueAsFloat("agent_follow_up_failures_decrease_chances_of_success_factor"); 
        }

        public static void OnAnnualUpdate_Postfix(
            ref FactionPeriodicUpdate __instance
        ) {
            Main.logDebug("OnAnnualUpdatePatch - adjusting maximum debt");
        }

    }
}