using System;
using System.Timers;

namespace BluePrism.Core.Utility
{
    public interface ITimer : IDisposable
    {
        event ElapsedEventHandler Elapsed;
        bool AutoReset { get; set; }

        double Interval { get; set; }

        void Start();

        void Stop();
    }
}
