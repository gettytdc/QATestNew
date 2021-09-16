using System;

namespace BluePrism.ClientServerResources.Core.Enums
{
    [Flags]
    public enum ResourceStatusChange
    {    
        None = 0, 
        OfflineChange = 1,
        OnlineChange = 2,
        UserMessageWaiting = 4,
        EnvironmentChange = 8,
        OnlineOrOfflineChange = OnlineChange | OfflineChange
    }
}
