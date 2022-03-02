using System;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Interface describing a calendar which determines whether a job
    /// can run at a specified point in time or not.
    /// </summary>
    public interface ICalendar
    {
        /// <summary>
        /// Checks whether this calendar permits execution at the given
        /// point in time or not.
        /// </summary>
        /// <param name="dt">The point in time at which a job may run
        /// </param>
        /// <returns>true if this calendar supports the execution of a
        /// job at the specified point in time; false otherwise.
        /// </returns>
        bool CanRun(DateTime dt);

        /// <summary>
        /// Returns true if there is at least 1 working day in the calendar
        /// </summary>
        /// <returns></returns>
        bool HasAnyWorkingDays();

        /// <summary>
        /// Updates the given metadata with the calendar data held on 
        /// this object.
        /// </summary>
        /// <param name="metadata">The metadata on which the calendar
        /// data is required.</param>
        void UpdateMetaData(TriggerMetaData metadata);
    }
}
