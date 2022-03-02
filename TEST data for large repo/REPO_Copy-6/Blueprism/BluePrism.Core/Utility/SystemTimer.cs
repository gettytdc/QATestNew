using System;
using System.Timers;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Wrapper for System.Timers.Timer class - used to enable unit testing of
    /// functionality that relies on timer functionality by allowing
    /// timer functionality to be simulated
    /// </summary>
    public class SystemTimer : IDisposable
    {
        /// <summary>
        /// The underlying System.Timers.Timer instance
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Create a new instance of the SystemTimer wrapper
        /// </summary>
        /// <param name="interval">
        /// The interval after the timer is started, that the elapsed event is fired
        /// </param>
        public static SystemTimer Create(TimeSpan interval)
        {
            return new SystemTimer(interval);
        }

        /// <summary>
        /// Create a new instance of the SystemTimer wrapper
        /// </summary>
        /// <param name="interval">
        /// The interval after the timer is started, that the elapsed event is fired
        /// </param>
        public SystemTimer(TimeSpan interval)
        {
            this._timer = new Timer(interval.TotalMilliseconds);
            this._timer.Elapsed += _timer_Elapsed;
        }


        /// <summary>
        /// The event raised when the timer has elapsed
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// The event raised when this SystemTimer is started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Start the timer and fire the Started event
        /// </summary>
        public void Start()
        {
            this._timer.Start();
            
            var handler = this.Started;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }  
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            this._timer.Stop();
        }

        /// <summary>
        /// Triggers the Elapsed event manually - exposed for unit testing
        /// </summary>
        internal void TriggerElapsed()
        {
            var handler = this.Elapsed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            } 
        }

        /// <summary>
        /// Handles the underlying timer elapsed event, and fires the elapsed event
        /// in our wrapper.
        /// </summary>
        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TriggerElapsed();
        }

        /// <summary>
        /// Indicates whether the timer will raise the elapsed event once (false) or 
        /// repeatedly (true)
        /// </summary>
        public bool AutoReset 
        {
            get 
            { 
                return _timer.AutoReset; 
            }
            set
            {
                _timer.AutoReset = value;
            }
        }

        /// <summary>
        /// The interval after the timer is started, that the elapsed event is fired 
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return TimeSpan.FromMilliseconds(_timer.Interval);
            }
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null) _timer.Dispose();
            }
        }
        #endregion 
    }
}
