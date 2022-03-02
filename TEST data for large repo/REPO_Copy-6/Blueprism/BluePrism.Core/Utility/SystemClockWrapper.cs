using System;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Wrapper around system date functionality
    /// </summary>
    public class SystemClockWrapper : ISystemClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}