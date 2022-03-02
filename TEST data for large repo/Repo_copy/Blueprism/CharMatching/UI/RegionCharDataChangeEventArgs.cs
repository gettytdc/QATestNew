using System;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Delegate used to represent a listener for the RegionCharDataChangeEvent
    /// </summary>
    public delegate void RegionCharDataChangeEventHandler(
        object sender, RegionCharDataChangeEventArgs e);

    /// <summary>
    /// Enumeration detailing the types of change that can occur to the char data
    /// inside a region.
    /// </summary>
    public enum RegionCharDataChange { Font }

    /// <summary>
    /// Arguments detailing a CharData change in a region
    /// </summary>
    public class RegionCharDataChangeEventArgs : EventArgs
    {
        // The type of change
        private RegionCharDataChange _type;
        // The old value
        private object _oldValue;
        // The new value
        private object _newValue;

        /// <summary>
        /// Creates a new event args object with the given attributes
        /// </summary>
        /// <param name="type">The type of data change</param>
        /// <param name="oldValue">The old value of the data which has changed
        /// </param>
        /// <param name="newValue">The new value of the data which has changed
        /// </param>
        public RegionCharDataChangeEventArgs(RegionCharDataChange type,
            object oldValue, object newValue)
        {
            _type = type;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        /// <summary>
        /// The type of change represented by these event args
        /// </summary>
        public RegionCharDataChange ChangeType { get { return _type; } }

        /// <summary>
        /// The old value for the changed data
        /// </summary>
        public object OldValue { get { return _oldValue; } }

        /// <summary>
        /// The new value after the data has changed
        /// </summary>
        public object NewValue { get { return _newValue; } }
    }
}
