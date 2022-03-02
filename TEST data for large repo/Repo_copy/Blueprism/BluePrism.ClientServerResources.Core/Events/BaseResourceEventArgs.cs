using System;

namespace BluePrism.ClientServerResources.Core.Events
{
    public abstract class BaseResourceEventArgs
    {
        public Guid SessionId { get; protected set; }
        public string ErrorMessage { get; protected set; } = string.Empty;
        public string UserMessage { get; protected set; } = string.Empty;

        public bool Success => string.IsNullOrEmpty(ErrorMessage);
        public bool PendingUserMessage => !string.IsNullOrEmpty(UserMessage);
        public virtual bool FromScheduler => false;
    }
}
