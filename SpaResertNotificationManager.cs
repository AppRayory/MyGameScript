using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeachSpaResert;
using UnityEngine;
using NotificationSamples;
using TMPro;

public class SpaResertNotificationManager : MonoBehaviour
{
    // On Android, this represents the notification's channel, and is required (at least one).
    // Channels defined as global constants so can be referred to from GameController.cs script when setting/sending notification
    public const string ChannelId = "game_channel0";
    public const string ReminderChannelId = "reminder_channel1";
    [SerializeField] private ItemsSaveLoadDataKeeper _saveLoadDataKeeper;

    [Header("For broken env obj")] 
    [SerializeField] private string titleField;
    [SerializeField] private string bodyField;
    [SerializeField] private string channelField;
    [SerializeField] private int ReminderTime;
    [SerializeField] private string TitleForPrizeNotification;
    [SerializeField] private string BodyForPrizeNotification;
    private PendingNotification _brokenEnvObjNotification;
    private EnvironmentObject _cachedLowEnvironmentObject;
    [SerializeField] protected GameNotificationsManager manager;
    private bool isBackToGame;
    


    private void Start()
    {
        var c1 = new GameNotificationChannel(ChannelId, "Default Game Channel", "Generic notifications");
        var c3 = new GameNotificationChannel(ReminderChannelId, "Reminder Channel", "Reminder notifications");
        manager.Initialize(c1, c3);
        OnPlayReminder();
        isBackToGame = true;
    }

    private void LateUpdate(){
        //cancel notification if it is too early to push

        if(!ReferenceEquals(_cachedLowEnvironmentObject, null) && _cachedLowEnvironmentObject.isWorkableEnvObject ) return;
        
        if (_saveLoadDataKeeper.EnvironmentObjects.Count > 0)
        {
            List<EnvironmentObject> WorkableObjects = _saveLoadDataKeeper.EnvironmentObjects.
                Where(el => el.isWorkableEnvObject).ToList();
            
            if (WorkableObjects.Any()){
                _cachedLowEnvironmentObject = WorkableObjects[0];
                if (WorkableObjects.Count > 1){
                    for(int i = 1; i < WorkableObjects.Count; i++)
                    {
                        if (WorkableObjects[i].TimeOfWorkEnd < _cachedLowEnvironmentObject.TimeOfWorkEnd) 
                            _cachedLowEnvironmentObject = WorkableObjects[i];
                    }
                }
                
                _brokenEnvObjNotification = CreateNotification(titleField,bodyField, _cachedLowEnvironmentObject.TimeOfWorkEnd,
                    true,channelField);
            }
        }
    }

    /// <summary>
    /// Queue a notification with the given parameters.
    /// </summary>
    /// <param name="title">The title for the notification.</param>
    /// <param name="body">The body text for the notification.</param>
    /// <param name="deliveryTime">The time to deliver the notification.</param>
    /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
    /// <param name="reschedule">
    /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
    /// </param>
    /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
    /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
    /// <param name="smallIcon">Notification small icon.</param>
    /// <param name="largeIcon">Notification large icon.</param>
    private PendingNotification CreateNotification(string title, string body, DateTime deliveryTime,
        bool reschedule = false, string channelId = null)
    {
        IGameNotification notification = manager.CreateNotification();
        if (notification != null)
        {
            notification.Title = title;
            notification.Body = body;
            notification.Group = !string.IsNullOrEmpty(channelId) ? channelId : ChannelId;
            notification.DeliveryTime = deliveryTime;

            PendingNotification notificationToDisplay = manager.ScheduleNotification(notification);
            notificationToDisplay.Reschedule = reschedule;
            Debug.LogWarning($"Queued event with ID \"{notification.Id}\" at time {deliveryTime:HH:mm}");
            return  manager.ScheduleNotification(notification);
        }
        return null;
    }
    public void OnPlayReminder()
    {
        DateTime deliveryTime = DateTime.Now.ToLocalTime().AddDays(1);
        DateTime secondTimeDeliveryTime = DateTime.Now.ToLocalTime().AddDays(2);
        DateTime nextTimedeliveryTime = DateTime.Now.ToLocalTime().AddDays(3);
        deliveryTime = new DateTime(deliveryTime.Year, deliveryTime.Month, deliveryTime.Day, ReminderTime, 0, 0,
            DateTimeKind.Local);
        CreateNotification("Time to play!", "Spa Resert Manager is waiting you", deliveryTime,
            channelId:ReminderChannelId);
        secondTimeDeliveryTime = new DateTime(secondTimeDeliveryTime.Year, secondTimeDeliveryTime.Month, secondTimeDeliveryTime.Day, ReminderTime, 0, 0,
            DateTimeKind.Local);
        CreateNotification("Time to play!", "Spa Resert Manager is waiting you", secondTimeDeliveryTime,
            channelId:ReminderChannelId);
        nextTimedeliveryTime = new DateTime(nextTimedeliveryTime.Year, nextTimedeliveryTime.Month, nextTimedeliveryTime.Day, ReminderTime, 0, 0,
            DateTimeKind.Local);
        CreateNotification("Time to play!", "Spa Resert Manager is waiting you", nextTimedeliveryTime,
            channelId:ReminderChannelId);
    }

    /// <summary>
    /// Cancel a given pending notification
    /// </summary>
    public void CancelPendingNotificationItem(PendingNotification itemToCancel)
    {
        manager.CancelNotification(itemToCancel.Notification.Id.Value);
    }
}