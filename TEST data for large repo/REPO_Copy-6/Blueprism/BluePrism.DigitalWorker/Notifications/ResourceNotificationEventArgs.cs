using System;
using BluePrism.AutomateAppCore.Resources;

namespace BluePrism.DigitalWorker.Notifications
{
    public class ResourceNotificationEventArgs : EventArgs
    {
        public readonly ResourceNotification Notification;

        public ResourceNotificationEventArgs(ResourceNotification notification)
        {
            Notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }
    }
}