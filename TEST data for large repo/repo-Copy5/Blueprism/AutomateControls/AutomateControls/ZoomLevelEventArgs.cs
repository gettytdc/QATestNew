using System;

namespace AutomateControls
{
    /// <summary>
    /// Delegate describing an event handler for ZoomLevelChanged events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ZoomLevelChangeHandler(object sender, ZoomLevelEventArgs e);

    /// <summary>
    /// Event arguments detailing a zoom level event
    /// </summary>
    public class ZoomLevelEventArgs : EventArgs
    {
        // The zoom factor currently applied
        private float _factor;

        /// <summary>
        /// Creates a new zoom level event args with the given factor.
        /// </summary>
        /// <param name="factor">The zoom factor which is currently applied.</param>
        public ZoomLevelEventArgs(float factor)
        {
            _factor = factor;
        }

        /// <summary>
        /// The zoom factor as a percentage.
        /// </summary>
        public int ZoomPercent { get { return (int)(100.0 * _factor); } }

        /// <summary>
        /// The raw zoom factor that is applied in the event.
        /// </summary>
        public float ZoomFactor { get { return _factor; } }
    }
}
