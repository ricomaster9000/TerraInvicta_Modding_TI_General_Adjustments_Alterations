using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems.Camera;
using Unity.Entities;

namespace TI_Augmenter.augmentations;

public class EventManager
{
	
	private static readonly Dictionary<SummaryCategory, int> maxSummaryQueueSize = new Dictionary<SummaryCategory, int>()
	{
		{
			SummaryCategory.CouncilorSightings,
			60
		},
		{
			SummaryCategory.EarthEvents,
			60
		},
		{
			SummaryCategory.Missions,
			60
		},
		{
			SummaryCategory.SpaceEvents,
			60
		},
		{
			SummaryCategory.Bombardment,
			120
		},
		{
			SummaryCategory.None,
			0
		}
	};
	
    public static void LogAndNotifyHabResourcesDepleted(TIFactionState detectingFaction, TIHabState hab)
    {
        NotificationQueueItem notificationQueueItem = new NotificationQueueItem()
        {
            relevantFactions = new List<TIFactionState>(),
            primaryFactions = new List<TIFactionState>(),
            alertBlockFaction = (TIFactionState) null,
            templateName = "HabResourcesDepleted"
        };

        if (detectingFaction == hab.ref_faction)
        {
            return;
        }
        notificationQueueItem.primaryFactions.Add(detectingFaction);
        notificationQueueItem.relevantFactions.Add(detectingFaction);
        notificationQueueItem.musicIntensityDelta = 0.1f;
        notificationQueueItem.popupResource1 = hab.iconResource;

	    notificationQueueItem.itemHeadline = Loc.T("UI.TI_Augmenter.HabResourcesDepletedHeadline", new object[]
	    {
		    hab.faction.adjective
	    });
	    notificationQueueItem.icon = hab.iconResource;
        notificationQueueItem.illustrationResource = World.Active.GetExistingManager<CameraManager>().skyboxBackdropPath;
        notificationQueueItem.gotoGameState = hab;
        notificationQueueItem.itemSummary = Loc.T("UI.TI_Augmenter.BaseSightedSummary", new object[]
        {
            hab.faction.displayNameWithColor,
            hab.habSite.displayName,
            hab.habSite.parentBody.displayName
        });
        AddItem(notificationQueueItem);
    }
    
    private static void AddItem(NotificationQueueItem item, bool addToAlienQueue = false)
		{
			if (item.template == null)
			{
				Log.Error("Null notification template for " + item.templateName + ". No notification pushed.", Array.Empty<object>());
				return;
			}
			item.dateTime = TITimeState.Now();
			item.dateTimeString = item.dateTime.ToCustomDateString();
			item.primaryFactions = (from x in item.primaryFactions
			where x != null
			select x).Distinct<TIFactionState>().ToList<TIFactionState>();
			item.relevantFactions = (from x in item.relevantFactions
			where x != null
			select x).Distinct<TIFactionState>().ToList<TIFactionState>();
			if (string.IsNullOrEmpty(item.itemDetail))
			{
				item.itemDetail = item.itemSummary;
			}
			else if (string.IsNullOrEmpty(item.itemSummary))
			{
				item.itemSummary = item.itemDetail;
			}
			TINotificationTemplate template = item.template;
			item.itemSummary = Loc.T("UI.Notifications.DateLog", new object[]
			{
				item.dateTimeString,
				item.itemSummary
			});
			TINotificationQueueState tinotificationQueueState = GameStateManager.NotificationQueue();
			tinotificationQueueState.notificationQueue.Insert(0, item);
			if (tinotificationQueueState.notificationQueue.Count > 60)
			{
				tinotificationQueueState.notificationQueue.RemoveRange(60, tinotificationQueueState.notificationQueue.Count - 60);
			}
			if (addToAlienQueue)
			{
				tinotificationQueueState.alienEvents++;
			}
			if (!string.IsNullOrEmpty(item.alertBlockEventName))
			{
				/**if (item.promptingGameState.isNationState)
				{
					tinotificationQueueState.promptQueue.AddPrompt(item.promptingGameState.ref_nation, item.alertBlockFaction, item.alertRelatedState, item.alertBlockEventName, item.utilityValue);
				}
				else
				{
					tinotificationQueueState.promptQueue.AddPrompt(item.alertBlockFaction, item.promptingGameState, item.alertRelatedState, item.alertBlockEventName, item.utilityValue);
				}**/
			}
			NotificationSummaryItem notificationSummaryItem = new NotificationSummaryItem(item.itemSummary, item.icon, item.iconBackgroundResource, item.backgroundColor, item.gotoGameState, addToAlienQueue, item.dateTime, item.templateName, item.timerFactions, item.newsFeedFactions, item.summaryLogFactions, item.outcome);
			List<TIFactionState> list = new List<TIFactionState>();
			List<TIFactionState> list2 = new List<TIFactionState>(item.alertFactions);
			if (list2.Count > 0)
			{
				list.AddRangeUnique(list2);
			}
			if (item.putInNewsFeed)
			{
				tinotificationQueueState.notificationSummaryQueue.Insert(0, notificationSummaryItem);
				list.AddRangeUnique(item.newsFeedFactions);
			}
			if (item.putInTimerQueue)
			{
				tinotificationQueueState.timerNotificationQueue.Insert(0, notificationSummaryItem);
				list.AddRangeUnique(item.timerFactions);
				if (tinotificationQueueState.timerNotificationQueue.Count > 60)
				{
					tinotificationQueueState.timerNotificationQueue.RemoveRange(60, tinotificationQueueState.timerNotificationQueue.Count - 60);
				}
			}
			if (item.putInSummaryLog)
			{
				SummaryCategory category = item.template.summaryAudience.category;
				tinotificationQueueState.panelSummaryQueue[category].Insert(0, notificationSummaryItem);
				list.AddRangeUnique(item.summaryLogFactions);
				if (tinotificationQueueState.panelSummaryQueue[category].Count > maxSummaryQueueSize[category])
				{
					tinotificationQueueState.panelSummaryQueue[category].RemoveRange(maxSummaryQueueSize[category], tinotificationQueueState.panelSummaryQueue[category].Count - maxSummaryQueueSize[category]);
				}
			}
			if (item.template.firstAlertOverride)
			{
				foreach (TIFactionState tifactionState in list2)
				{
					if (tifactionState.checkNotificationOverrides && TINotificationQueueState.FirstNotificationOfType(tifactionState, item.templateName) && item.template.alertAudience == NotificationAudience.None && (!tifactionState.notificationOverrides.ContainsKey(item.templateName) || tifactionState.notificationOverrides[item.templateName].alert != NotificationOverrideBehavior.Add))
					{
						item.itemDetail = new StringBuilder(item.itemDetail).AppendLine().AppendLine(Loc.T("UI.Notifications.OneTimeOnly")).ToString();
					}
				}
			}
			if (list.Count > 0)
			{
				PavonisInteractive.TerraInvicta.EventManager eventManager = GameControl.eventManager;
				GameEvent evt = new NewsItemCreated(item, notificationSummaryItem);
				string eventName = null;
				object[] sourceObjects = list.ToArray();
				eventManager.TriggerEvent(evt, eventName, sourceObjects);
			}
		}
}