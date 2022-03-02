using System;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Provides access to date and time on machine on which code is running
    /// </summary>
    public interface ISystemClock
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }
    }
}