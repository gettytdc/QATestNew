using System;

namespace BluePrism.BPServer.Enums
{
    internal class ExpiryAttribute : Attribute
    {
        internal double Interval { get; }
        internal LoggingLevel LogLevel { get; }

        /// <param name="interval">Interval (in days) that the timer will trigger at</param>
        /// <param name="logLevel">Ordinal representation of the LogLevel that this should create <see cref="NLog.LogLevel.FromOrdinal(int)"/></param>
        internal ExpiryAttribute(double interval, LoggingLevel logLevel)
        {
            Interval = TimeSpan.FromDays(interval).TotalMilliseconds;
            LogLevel = logLevel;

            if (Interval > int.MaxValue)
                throw new ArgumentException($"{nameof(Interval)} cannot be greater than int.max. (24 days max)");
        }
    }
}
