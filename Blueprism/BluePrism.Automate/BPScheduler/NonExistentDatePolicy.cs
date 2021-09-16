namespace BluePrism.Scheduling
{
    /// <summary>
    /// An enumeration of the policies to enact if the date to 
    /// activate a trigger falls on a non-existent date.
    /// 
    /// This always occurs when a date falls past the end of the
    /// month, and the policies effectively describe whether the
    /// alternative trigger should activate before or after the
    /// date on which it would have activated if such a date exists.
    /// </summary>
    public enum NonExistentDatePolicy : int
    {
        /// <summary>
        /// Skip the instance if the date doesn't exist for a
        /// particular invocation. ie. Do not schedule an
        /// alternative trigger activation... just treat it
        /// as if it has fired on the missing date.
        /// </summary>
        Skip = 0,

        /// <summary>
        /// Trigger an activation on the last supported day of the
        /// month in which the activity date does not exist.
        /// If a calendar is being used, this will be the last day
        /// before the nonexistent date which is allowed by the
        /// calendar.
        /// Without a calendar, this will just activate on the 
        /// last day of the month.
        /// </summary>
        LastSupportedDayInMonth = 1,

        /// <summary>
        /// Trigger an activation on the first supported day after
        /// the nonexistent date.
        /// If a calendar is being used, this will be the first day
        /// reached after the nonexistent date which is allowed by
        /// the calendar.
        /// Without a calendar, this will just activate on the first
        /// day of the subsequent month.
        /// </summary>
        FirstSupportedDayInNextMonth = 2
    }

}
