using System;

namespace BluePrism.Core.Utility
{
    public interface ISystemTimer : IDisposable
    {
        void Start(Action action, TimeSpan dueTime, TimeSpan period);

        void Stop();
    }
}