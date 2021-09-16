using BluePrism.AutomateAppCore.Resources;
using System;

namespace BluePrism.DigitalWorker.Notifications
{
    public interface INotificationHandler
    {
        void HandleNotification(ResourceNotification notification);
        event EventHandler<ResourceNotificationEventArgs> Notify;
    }
}
