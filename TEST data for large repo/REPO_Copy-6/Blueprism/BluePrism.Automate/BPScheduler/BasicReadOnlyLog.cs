using BluePrism.Scheduling.Properties;
using System;
using BluePrism.BPCoreLib.Data;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Schedule log which provides the barest details about the running schedule of
    /// the schedule instance - that of the instance time, start and end dates and
    /// last pulse time.
    /// </summary>
    [Serializable]
    public class BasicReadOnlyLog : IScheduleLog
    {
        /// <summary>
        /// A UTC representation of DateTime.MinValue - this represents the same
        /// "point in time" but has a UTC 'kind'.
        /// </summary>
        private static readonly DateTime MinValueUTC =
            new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);

        /// <summary>
        /// A UTC representation of DateTime.MaxValue - this represents the same
        /// "point in time" but has a UTC 'kind'.
        /// </summary>
        private static readonly DateTime MaxValueUTC =
            new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc);

        // The schedule that this log represents an instance of
        [NonSerialized]
        private ISchedule _schedule;

        // The name of the scheduler
        private string _schedulerName;

        // The date/time of the instance of the schedule that this log represents (UTC)
        private DateTime _instance;

        // The time at which the schedule was logged as starting (UTC)
        private DateTime _start;

        // The time at which the schedule was logged as having finished (UTC)
        private DateTime _end;

        // The last pulse that the log represented by this object received (UTC)
        private DateTime _lastPulse;

        /// <summary>
        /// Creates a new basic readonly log for the given instance time.
        /// </summary>
        /// <param name="instanceTime">The instance time for the schedule instance
        /// that this log represents</param>
        /// <remarks>Although this is conceptually a read-only log, its properties
        /// can be changed by subclasses while setting up the object. To outside
        /// classes, it remains read-only.</remarks>
        public BasicReadOnlyLog(DateTime instanceTime)
            : this("", instanceTime, MinValueUTC, MaxValueUTC, MinValueUTC) { }

        /// <summary>
        /// Creates a new basic readonly log with data drawn from the given provider.
        /// </summary>
        /// <param name="prov">The data provider responsible for providing the data
        /// for this log. The expected properties from the provider are :<list>
        /// <item>schedulername: String: The name of the scheduler which created
        /// this log instance</item>
        /// <item>instancetime: DateTime: The date/time of the log instance</item>
        /// <item>starttime: DateTime: The start time of the log</item>
        /// <item>endtime: DateTime: The end time of the log, or null if it has
        /// not yet ended</item>
        /// <item>lastpulse: DateTime: The last time that the log was pulsed or null
        /// if it has never been pulsed.</item></list></param>
        public BasicReadOnlyLog(IDataProvider prov)
            : this(
                prov.GetString("schedulername"),
                prov.GetValue("instancetime", default(DateTime)),
                prov.GetValue("starttime", MinValueUTC),
                prov.GetValue("endtime", MaxValueUTC),
                prov.GetValue("lastpulse", MinValueUTC))
        { }

        /// <summary>
        /// Creates a new basic readonly log with the given arguments.
        /// </summary>
        /// <param name="schedulerName">The name of the scheduler which executed
        /// this instance log.</param>
        /// <param name="instanceTime">The time at which the instance that 
        /// generated this log was scheduled to execute - this is expected to be
        /// in local time, since that is how the instance time is held in the
        /// scheduler.</param>
        /// <param name="start">The time at which this log was marked as having
        /// started execution. This is expected to be in UTC - all 'timestamp'
        /// dates/times are recorded in UTC</param>
        /// <param name="end">The time at which this log was marked as having
        /// finished execution. This is expected to be in UTC - all 'timestamp'
        /// dates/times are recorded in UTC</param>
        /// <param name="lastPulse">The time at which this log had its last pulse
        /// sent to it from its running scheduler. This is expected to be in UTC -
        /// all 'timestamp' dates/times are recorded in UTC</param>
        public BasicReadOnlyLog(
            string schedulerName,
            DateTime instanceTime, DateTime start, DateTime end, DateTime lastPulse)
        {
            if (instanceTime == default(DateTime))
            {
                throw new ArgumentException(
                    Resources.InstanceTimeMustBeProvidedForALog, nameof(instanceTime));
            }

            // Since the rest are in UTC, let's convert instance time to match
            _instance = instanceTime.ToUniversalTime();

            // The rest should already be in UTC
            _start = start;
            _end = end;
            _lastPulse = lastPulse;
        }

        #region IScheduleLog Members

        /// <summary>
        /// The schedule that this log represents an instance of
        /// </summary>
        public ISchedule Schedule
        {
            get { return _schedule; }
            set { _schedule = value; }
        }

        /// <summary>
        /// The name of the scheduler which created this log or an empty string if
        /// that information is not available.
        /// </summary>
        public string SchedulerName
        {
            get { return (_schedulerName ?? ""); }
            set { _schedulerName = value; }
        }

        /// <summary>
        /// The time at which the instance of the schedule that this log represents
        /// was scheduled to begin.
        /// </summary>
        public DateTime InstanceTime
        {
            get { return _instance; }
            protected set { _instance = value; }
        }

        /// <summary>
        /// The time at which this log was started
        /// </summary>
        public DateTime StartTime
        {
            get { return _start; }
            protected set { _start = value; }
        }

        /// <summary>
        /// The time at which this log was ended
        /// </summary>
        public DateTime EndTime
        {
            get { return _end; }
            protected set { _end = value; }
        }

        /// <summary>
        /// The time that this log was updated - this will be the end date/time
        /// if it is set; otherwise the last pulse time or the start time if it
        /// has never been pulsed
        /// </summary>
        /// <remarks>The datetime returned from this property will always be UTC
        /// </remarks>
        public DateTime LastUpdated
        {
            get
            {
                if (_end != MaxValueUTC)
                    return _end;
                if (_lastPulse != MinValueUTC)
                    return _lastPulse;
                return _start;
            }
        }

        /// <summary>
        /// Checks if this log has been marked as finished.
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return (_end != MaxValueUTC);
        }

        /// <summary>
        /// Fires a readonly exception - called when any of the update methods
        /// are called on this object.
        /// </summary>
        private void FireReadOnly()
        {
            throw new InvalidOperationException(Resources.CannotUpdateThisLogItIsReadOnly);
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called - this log is
        /// readonly and thus its state cannot be changed</exception>
        public void Start()
        {
            FireReadOnly();
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called - this log is
        /// readonly and thus its state cannot be changed</exception>
        public void Complete()
        {
            FireReadOnly();
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called - this log is
        /// readonly and thus its state cannot be changed</exception>
        public void Terminate(string reason)
        {
            FireReadOnly();
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called - this log is
        /// readonly and thus its state cannot be changed</exception>
        public void Terminate(string reason, Exception ex)
        {
            FireReadOnly();
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called - this log is
        /// readonly and thus its state cannot be changed</exception>
        public void Pulse()
        {
            FireReadOnly();
        }

        #endregion
    }
}
