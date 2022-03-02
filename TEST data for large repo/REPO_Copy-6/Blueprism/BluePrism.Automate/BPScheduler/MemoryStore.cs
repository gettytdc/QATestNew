using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using BluePrism.Scheduling.Calendar;
using BluePrism.BPCoreLib.Collections;
using System.Linq;

#pragma warning disable 67 // disable "this event is never used" warning...

namespace BluePrism.Scheduling
{
    /// <summary>
    /// A schedule store which operates only in memory.
    /// This is entirely dependent on the data which is set into it at runtime.
    /// </summary>
    public class MemoryStore : IScheduleStore
    {
        /// <summary>
        /// The scheduler which owns this store.
        /// </summary>
        private IScheduler _scheduler;

        /// <summary>
        /// The public holiday schema used within this store.
        /// </summary>
        private PublicHolidaySchema _schema;

        /// <summary>
        /// The calenders, keyed on their ID.
        /// </summary>
        private IDictionary<int, ScheduleCalendar> _calendars;

        /// <summary>
        /// The schedules keyed on their... name?
        /// </summary>
        private IDictionary<string, ISchedule> _schedules;

        /// <summary>
        /// The retired schedules, again, keyed on name
        /// </summary>
        private IDictionary<string, ISchedule> _retiredSchedules;

        /// <summary>
        /// A record of the logs created by the schedules in this store.
        /// </summary>
        private IDictionary<DateTime, IScheduleLog> _logs;

        /// <summary>
        /// Creates a new blank memory store.
        /// </summary>
        public MemoryStore()
        {
            _calendars = new Dictionary<int, ScheduleCalendar>();
            _schedules = new Dictionary<string, ISchedule>();
            _retiredSchedules = new Dictionary<string, ISchedule>();
            _logs = new Dictionary<DateTime, IScheduleLog>();
        }

        #region IScheduleStore Members

        /// <summary>
        /// Event fired when the schedules held in this store are updated.
        /// </summary>
        public event StoreDataChanged SchedulesUpdated;

        /// <summary>
        /// The scheduler which currently owns this store.
        /// </summary>
        public IScheduler Owner
        {
            get { return _scheduler; }
            set
            {
                // We're avoiding circularity here...
                // if a value is being set, check that its store is not already
                // to this and set it if not.
                if (value != null && !Object.ReferenceEquals(value.Store, this))
                    value.Store = this;

                _scheduler = value;
            }
        }

        /// <summary>
        /// Gets the active schedules registered within this store.
        /// </summary>
        /// <returns>A collection of currently active schedules in this store.
        /// </returns>
        public ICollection<ISchedule> GetActiveSchedules()
        {
            return _schedules.Values;
        }

        /// <summary>
        /// Fires the 'schedules updated' event.
        /// </summary>
        protected void FireSchedulesUpdated()
        {
            if (SchedulesUpdated != null)
                SchedulesUpdated();
        }

        /// <summary>
        /// Gets the schedule with the given name.
        /// </summary>
        /// <param name="name">The name of the schedule.</param>
        /// <returns>The schedule with the given name</returns>
        public ISchedule GetSchedule(string name)
        {
            ISchedule sched = null;
            if (_schedules.TryGetValue(name, out sched))
                return sched;
            if (_retiredSchedules.TryGetValue(name, out sched))
                return sched;
            return null;
        }

        /// <summary>
        /// Handler for a schedule being renamed.
        /// Since the schedules are held by name in this object, their
        /// keys need to be updated on a name change
        /// </summary>
        /// <param name="args">The args detailing the name change.</param>
        private void HandleScheduleRename(ScheduleRenameEventArgs args)
        {
            if (_retiredSchedules.ContainsKey(args.OldName))
            {
                throw new ArgumentException(
                    Resources.TheGivenScheduleIsRetiredItMustBeUnretiredBeforeSaving);
            }
            _schedules.Remove(args.OldName);
            _schedules[args.NewName] = args.SourceSchedule;
            FireSchedulesUpdated();
        }

        /// <summary>
        /// Saves the given schedule to this store.
        /// </summary>
        /// <param name="schedule">The schedule to save.</param>
        /// <exception cref="ArgumentException">If the given schedule is
        /// retired.</exception>
        public void SaveSchedule(ISchedule schedule)
        {
            if (_retiredSchedules.ContainsKey(schedule.Name))
            {
                throw new ArgumentException(
                    Resources.TheGivenScheduleIsRetiredItMustBeUnretiredBeforeSaving);
            }
            _schedules[schedule.Name] = schedule;
            schedule.NameChanging += HandleScheduleRename;
            FireSchedulesUpdated();
        }

        /// <summary>
        /// Deletes the given schedule from this store.
        /// </summary>
        /// <param name="schedule">The schedule to delete.</param>
        public void DeleteSchedule(ISchedule schedule)
        {
            schedule.NameChanging -= HandleScheduleRename;

            if (_schedules.Remove(schedule.Name) || _retiredSchedules.Remove(schedule.Name))
                FireSchedulesUpdated();
        }

        /// <summary>
        /// Retires the given schedule, removing it from the active map
        /// of schedules held in this store.
        /// </summary>
        /// <param name="schedule">The schedule to retire. This should be
        /// a member of the active schedules in this store - if it is not
        /// it is ignored.</param>
        public void RetireSchedule(ISchedule schedule)
        {
            string name = schedule.Name;
            if (_schedules.Remove(name))
            {
                _retiredSchedules[name] = schedule;
                FireSchedulesUpdated();
            }
        }

        /// <summary>
        /// Unretires the given schedule, re-adding it into the active
        /// map of schedules held in this store.
        /// </summary>
        /// <param name="schedule">The schedule to retire. This should
        /// be a member of the retired schedules in this store - if it
        /// is not, it is ignored.</param>
        public void UnretireSchedule(ISchedule schedule)
        {
            string name = schedule.Name;
            if (_retiredSchedules.Remove(name))
            {
                _schedules[name] = schedule;
                FireSchedulesUpdated();
            }
        }

        /// <summary>
        /// Gets the schedule log for the given schedule which was fired at
        /// the given instant in time.
        /// </summary>
        /// <param name="schedule">The schedule for which the log is required.
        /// </param>
        /// <param name="instant">The instant in time at which the schedule log
        /// is required</param>
        /// <returns>The schedule log which was created for that specific time
        /// for the given schedule.</returns>
        public IScheduleLog GetLog(ISchedule schedule, DateTime instant)
        {
            // FIXME: This doesn't actually filter on the schedule at all.
            if (_logs.ContainsKey(instant))
                return _logs[instant];
            return null;
        }

        /// <summary>
        /// Gets the logs pertaining to the given schedule which fall in between
        /// the given date/times.
        /// </summary>
        /// <param name="schedule">The schedule for which the logs are required.
        /// </param>
        /// <param name="after">The point in time after which logs are required.
        /// </param>
        /// <param name="before">The point in time before which logs are required.
        /// </param>
        /// <returns></returns>
        public IBPSet<IScheduleLog> GetLogs(ISchedule schedule, DateTime after, DateTime before)
        {
            clsSet<IScheduleLog> set = new clsSet<IScheduleLog>();
            foreach (DateTime instancetime in _logs.Keys)
            {
                if (instancetime > after && instancetime < before)
                {
                    set.Add(_logs[instancetime]);
                }
            }
            return set;
        }

        /// <summary>
        /// Creates a log for the given trigger instance.
        /// </summary>
        /// <param name="instance">The instance for which a log is required.</param>
        /// <returns>An IScheduleLog instance which can be used to log events occurring
        /// throughout a schedule's execution.</returns>
        public IScheduleLog CreateLog(ITriggerInstance instance)
        {
            _logs[instance.When] = null;
            return null;
        }

        public void StopListeningForChanges()
        {
        }

        #endregion

        #region IScheduleCalendarStore Members

        /// <summary>
        /// Event fired when the calendars have been updated in this store.
        /// </summary>
        public event StoreDataChanged CalendarsUpdated;

        /// <summary>
        /// Gets all calendars held within this store.
        /// </summary>
        /// <returns></returns>
        public ICollection<ScheduleCalendar> GetAllCalendars()
        {
            return _calendars.Values;
        }

        /// <summary>
        /// Gets the calendar with the given ID.
        /// </summary>
        /// <param name="id">The ID of the required calendar.</param>
        /// <returns>The calendar corresponding to the given ID, or null if no
        /// such calendar exists.</returns>
        public ScheduleCalendar GetCalendar(int id)
        {
            if (_calendars.ContainsKey(id))
                return _calendars[id];
            return null;
        }

        /// <summary>
        /// Gets the calendar with the given name.
        /// </summary>
        /// <param name="name">The name of the calendar required.</param>
        /// <returns>The calendar with the specified name, or null if no
        /// such calendar exists within this store.</returns>
        public ScheduleCalendar GetCalendar(string name)
        {
            foreach (ScheduleCalendar cal in GetAllCalendars())
            {
                if (cal != null && string.Equals(name, cal.Name))
                    return cal;
            }
            return null;
        }

        /// <summary>
        /// A lock object to use for auto-generating a calendar ID.
        /// </summary>
        private readonly Object _autogenLock = new Object();

        /// <summary>
        /// Creates a new ID for the calendar.
        /// </summary>
        /// <returns>A new unused ID for the calendar, or -1 if no ID exists...
        /// which is unlikely, really.</returns>
        private int autogenId()
        {
            lock (_autogenLock)
            {
                for (int i = 1; i < int.MaxValue; i++)
                {
                    if (!_calendars.ContainsKey(i))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// Saves the given calendar to this store.
        /// </summary>
        /// <param name="cal">The calendar to save in this store.</param>
        public void SaveCalendar(ScheduleCalendar cal)
        {
            if (cal.Id == 0)
                cal.Id = autogenId();
            _calendars[cal.Id] = cal;
        }

        /// <summary>
        /// Deletes the given calendar from this store.
        /// </summary>
        /// <param name="cal">The calendar to delete.</param>
        public void DeleteCalendar(ScheduleCalendar cal)
        {
            _calendars.Remove(cal.Id);
        }

        /// <summary>
        /// Gets the public holiday schema registered with this store.
        /// </summary>
        /// <returns>The public holiday schema for this store, or null if
        /// no schema has been placed in this store.</returns>
        public PublicHolidaySchema GetSchema()
        {
            return _schema;
        }

        /// <summary>
        /// Sets the public holiday schema on this store.
        /// </summary>
        /// <param name="schema">The schema to use within this store.</param>
        public void SetSchema(PublicHolidaySchema schema)
        {
            _schema = schema;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of this memory store. Not really that much to do - this just
        /// clears down the dictionaries used to store the scheduling elements.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _schedules.Clear();
                    _retiredSchedules.Clear();
                    _calendars.Clear();
                    _logs.Clear();
                }
            }
            _disposed = true;
        }

        public TimeZoneInfo GetServerTimeZone() => throw new NotImplementedException();

        public DateTime TriggerSchedule(ISchedule schedule) =>
            throw new NotImplementedException();

        public DateTime TriggerSchedule(ISchedule schedule, DateTime when) =>
            throw new NotImplementedException();

        ~MemoryStore()
        {
            Dispose(false);
        }


        #endregion
    }
}
