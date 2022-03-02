using System;
using System.Collections.Generic;
using BluePrism.Scheduling.Calendar;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Delegate to use indicating that some store data has changed.
    /// </summary>
    public delegate void StoreDataChanged();

    /// <summary>
    /// Store with which the calendars can be manipulated
    /// </summary>
    public interface IScheduleCalendarStore : IDisposable
    {
        /// <summary>
        /// Event fired when the calendars held by this store have been
        /// updated and may have changed the timing data
        /// </summary>
        event StoreDataChanged CalendarsUpdated;

        /// <summary>
        /// Gets all the calendars described by this store
        /// </summary>
        /// <returns>A collection of schedule calendars currently mapped
        /// by this store</returns>
        ICollection<ScheduleCalendar> GetAllCalendars();

        /// <summary>
        /// Gets the calendar identified by the given ID.
        /// </summary>
        /// <param name="id">The ID which identifies the required schedule
        /// calendar.</param>
        /// <returns>The Schedule Calendar which corresponds to the given
        /// ID or null if no such calendar exists.</returns>
        ScheduleCalendar GetCalendar(int id);

        /// <summary>
        /// Gets the calendar with the given name
        /// </summary>
        /// <param name="name">The name of the calendar required.</param>
        /// <returns>The calendar corresponding to the specified name, or
        /// null if no such calendar exists within the store.</returns>
        ScheduleCalendar GetCalendar(string name);


        /// <summary>
        /// Saves the calendar to the backing store, either creating or
        /// updating it as necessary.
        /// When the method returns, the calendar object passed will have
        /// its ID set to the identifying value from the backing store.
        /// </summary>
        /// <param name="cal">The calendar to be saved.</param>
        void SaveCalendar(ScheduleCalendar cal);


        /// <summary>
        /// Deletes the given calendar from the store.
        /// </summary>
        /// <param name="cal">The calendar to delete from the backing store
        /// represented by this object.</param>
        /// <exception cref="InvalidOperationException">If a record exists
        /// within the store which requires the specified calendar, eg. if a
        /// trigger relies on this calendar for its timing data.</exception>
        void DeleteCalendar(ScheduleCalendar cal);

        /// <summary>
        /// Gets the public holiday schema from this store
        /// </summary>
        /// <returns>The public holiday schema used by the calendars within
        /// this store.</returns>
        PublicHolidaySchema GetSchema();

    }
}
