using System;
using System.Threading;

namespace BluePrism.Core.Utility
{
    public class SystemTimerWrapper : ISystemTimer
    {
        private Timer _timer;

        public void Start(Action action, TimeSpan dueTime, TimeSpan period)
        {
            if (_timer != null)
            {
                throw new InvalidOperationException("Already started");
            }
            _timer = new Timer(x => action(), null, (int)dueTime.TotalMilliseconds, (int)period.TotalMilliseconds);
        }

        public void Stop()
        {
            if (_timer == null)
            {
                throw new InvalidOperationException("Not started");
            }
            _timer.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}