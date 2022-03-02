using System;

namespace BluePrism.ApplicationManager.AppMan.Service.Events
{
    public class DisconnectedEventArgs : EventArgs
    {
        public Guid AppManId { get; }

        public DisconnectedEventArgs(Guid appManId) => AppManId = appManId;
    }
}
