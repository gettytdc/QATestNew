using System;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Use to limit the number of actions that are fired. If the limiter is
    /// triggered, the Execute event won't be fired unless the timer elapses 
    /// without the limiter being triggered again. If it is triggered within that
    /// duration then the timer is reset (once the timer has elapsed).
    /// 
    /// To use: 
    /// 1) Create an ActionLimiter instance
    /// 2) Where you would normally execute an action in code (e.g. on a text changed event), 
    /// call ActionLimiter.Trigger instead
    /// 3) Handle the ActionLimiter.Execute event, and call your action there instead
    /// </summary>
    public class ActionLimiter : IDisposable
    {
        /// <summary>
        /// Timer used to control the limiting
        /// </summary>
        private SystemTimer _timer;
                
        /// <summary>
        /// An action has been triggered, and is currently prevented from being invoked
        /// </summary>
        private bool _limiting = false;

        /// <summary>
        /// Action has been triggered during the limiting
        /// </summary>
        private bool _dirty = false;
                
        /// <summary>
        /// Create a new instance of the ActionLimiter class
        /// </summary>
        public ActionLimiter(TimeSpan interval) 
        {
            _timer = new SystemTimer(interval);
            _timer.AutoReset = false;
            _timer.Elapsed += HandleTimer;
        }

        /// <summary>
        /// Accesses the SystemTimer used by this ActionLimiter - exposed
        /// for unit testing
        /// </summary>
        internal SystemTimer Timer
        {
            get
            {
                return _timer;
            }
        }

        /// <summary>
        /// Call trigger to indicate that an event that should trigger the action has
        /// occurred
        /// </summary>
        public void Trigger()
        {
            if (_limiting)
            {
                _dirty = true;
            }
            else
            {
                _limiting = true;
                _timer.Start();
            }
        }
            

        /// <summary>
        /// Handles the timer elapsed event.
        /// </summary>
        private void HandleTimer(Object source, EventArgs e)
        {

            _limiting = false;

            //If changes have been made, whilst limiting then restart the timer
            //without raising the execute event
            if (_dirty) 
            {
                _dirty = false;
                _limiting = true;
                _timer.Start();
            }
            else //If no changes made, then raise the execute action
            {
                RaiseExecute();
            }
        }
        
        /// <summary>
        /// The event raised when the limiter has finished, and the action is ready to execute
        /// </summary>
        public event ExecuteEventHandler Execute;
                
        private void RaiseExecute()
        {            
            var handler = this.Execute;
            if (handler != null)
            {
                Execute(this, EventArgs.Empty);
            }
        }

        public delegate void ExecuteEventHandler(object sender, EventArgs eventArgs);
               


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
