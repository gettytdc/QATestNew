
using System;
using System.Timers;

namespace BluePrism.Core.Utility
{
    public class TimerWrapper : ITimer
    {
        private readonly Timer _timer;
        private bool _disposed;

        public TimerWrapper(Timer timer)
        {
            _timer = timer;
            _timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(this, e);
        }

        public bool AutoReset { get => _timer.AutoReset; set => _timer.AutoReset = value; }
        public double Interval { get => _timer.Interval; set => _timer.Interval = value; }

        public event ElapsedEventHandler Elapsed;

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_disposed)
            {
                return;
            }

            if(disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }
}
