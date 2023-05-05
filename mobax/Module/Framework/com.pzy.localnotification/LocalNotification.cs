using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID || UNITY_EDITOR
using Unity.Notifications.Android;
#endif
#if UNITY_IOS || UNITY_EDITOR
using Unity.Notifications.iOS;
#endif
using System;

public static class LocalNotification 
{

    public static string gameName = Application.productName;

    static LocalNotification()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            var gameName = Application.productName;
            var channel = new AndroidNotificationChannel()
            {
                Id = gameName,
                Name = gameName,
                Importance = Importance.Default,
                Description = gameName,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }
    }

    public static void Cancel(int id)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            AndroidNotificationCenter.CancelNotification(id);
#endif
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS || UNITY_EDITOR
            iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
#endif
        }
    }

    public static void Set(int id, string title, string text, DateTime dateTime)
    {
        Debug.Log($"[LocalNotification] id {id} set" );
        if(Application.isEditor)
        {
            return;
        }

        if(Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            AndroidNotificationCenter.CancelNotification(id);
            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = text;
            notification.FireTime = dateTime;

            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, gameName, id);
#endif
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS || UNITY_EDITOR
            iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
            var timeTrigger = new iOSNotificationCalendarTrigger()
            {
                Year = dateTime.Year,
                Month = dateTime.Month,
                Day = dateTime.Day,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second,
                Repeats = false,
            };

            var notification2 = new iOSNotification()
            {
                // You can specify a custom identifier which can be used to manage the notification later.
                // If you don't provide one, a unique string will be generated automatically.
                Identifier = id.ToString(),
                Title = title,
                Body = text,
                Subtitle = "",
                ShowInForeground = false,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = gameName,
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };
#endif
        }
    }
}
