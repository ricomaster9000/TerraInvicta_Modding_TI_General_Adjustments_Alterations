using System.Collections.Generic;
using System.Reflection;
using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustments_alterations.notifications;

public class HabSiteResourcesDepleted
{
    public HabSiteResourcesDepleted(TIHabSiteState habSiteState)
    {
        /*NotificationQueueItem notificationQueueItem = TINotificationQueueState.InitItem(MethodBase.GetCurrentMethod().Name);
        notificationQueueItem.primaryFactions = new List<TIFactionState>(habSiteState.ref_factions);
        notificationQueueItem.relevantFactions = TINotificationQueueState.AllFactions;
        notificationQueueItem.icon = habSiteState.iconResource;
        notificationQueueItem.itemHeadline = Loc.T("UI.Notifications.HabDecommissioned.Hed", new object[]
        {
            habSiteState.GetDisplayName(GameControl.control.activePlayer)
        });
        notificationQueueItem.itemSummary = Loc.T("UI.Notifications.HabDecommissioned.Summary", new object[]
        {
            habSiteState.GetDisplayName(GameControl.control.activePlayer)
        });
        notificationQueueItem.itemDetail = Loc.T("UI.Notifications.HabDecommissioned.Detail", new object[]
        {
            TIUtilities.GetLocationString(habSiteState, true, true)
        });
        TINotificationQueueState.AddItem(notificationQueueItem, false);*/
    }
}