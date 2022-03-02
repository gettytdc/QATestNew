namespace BluePrism.Scheduling
{
    /// <summary>
    /// Enumeration containing the type of interval at which a trigger is repeated.
    /// </summary>
    public enum IntervalType : int
    {
        /// <summary>
        /// The trigger is never activated
        /// </summary>
        Never = -1,

        /// <summary>
        /// The trigger is activated once and never repeated
        /// </summary>
        Once = 0,

        /// <summary>
        /// The trigger is activated at hourly intervals
        /// </summary>
        Hour = 1,

        /// <summary>
        /// The trigger is activated at daily intervals
        /// </summary>
        Day = 2,

        /// <summary>
        /// The trigger is activated at weekly intervals
        /// </summary>
        Week = 3,

        /// <summary>
        /// The trigger is activated at monthly intervals
        /// </summary>
        Month = 4,

        /// <summary>
        /// The trigger is activated at annual intervals
        /// </summary>
        Year = 5,

        /// <summary>
        /// The trigger is activated in minute intervals
        /// </summary>
        Minute = 6,

        /// <summary>
        /// The trigger is activated at second intervals
        /// </summary>
        Second = 7

    }
}
