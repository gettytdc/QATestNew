using System;
using BluePrism.AutomateAppCore.Resources;

namespace BluePrism.DigitalWorker.Notifications
{
    public class NotificationHandler : INotificationHandler
    {
        public event EventHandler<ResourceNotificationEventArgs> Notify;

        public void HandleNotification(ResourceNotification notification)
            => Notify?.Invoke(this, new ResourceNotificationEventArgs(notification));
    }
}
