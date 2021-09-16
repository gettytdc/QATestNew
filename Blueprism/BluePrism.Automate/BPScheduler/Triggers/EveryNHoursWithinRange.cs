using System;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which activates every n hours within a certain time range.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNHoursWithinRange : EveryNMinutesWithinRange
    {
        #region Constructors

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours with no time limitations.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        public EveryNHoursWithinRange(int hours)
            : base(hours * 60) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours with no time limitations.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        public EveryNHoursWithinRange(int hours, int calendarId)
            : base(hours * 60, calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours, if that time falls between the given
        /// <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        /// <param name="startTime">The start time at which an activation
        /// should be valid - this is inclusive, ie. the start time itself
        /// would be a valid activation time.</param>
        /// <param name="endTime">The end time at which an activation
        /// should be valid - this is inclusive, ie. the end time itself
        /// would be a valid activation time.</param>
        public EveryNHoursWithinRange(int hours, TimeSpan startTime, TimeSpan endTime)
            : base(hours * 60, startTime, endTime) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours, if that time falls between the given
        /// <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        /// <param name="startTime">The start time at which an activation
        /// should be valid - this is inclusive, ie. the start time itself
        /// would be a valid activation time.</param>
        /// <param name="endTime">The end time at which an activation
        /// should be valid - this is inclusive, ie. the end time itself
        /// would be a valid activation time.</param>
        public EveryNHoursWithinRange(int hours, TimeSpan start, TimeSpan end, int calendarId)
            : base(hours * 60, start, end, calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours, if that time falls within the given
        /// <paramref name="range"/>.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        /// <param name="range">The time range within which an activation
        /// should be valid.</param>
        public EveryNHoursWithinRange(int hours, clsTimeRange range)
            : base(hours * 60, range) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="hours"/> hours, if that time falls within the given
        /// <paramref name="range"/>.
        /// </summary>
        /// <param name="hours">The number of hours between activations
        /// </param>
        /// <param name="range">The time range within which an activation
        /// should be valid.</param>
        /// <param name="calendarId">The ID of the calendar which dictates
        /// when this trigger should be enabled.</param>
        public EveryNHoursWithinRange(int hours, clsTimeRange range, int calendarId)
            : base(hours * 60, range, calendarId) { }

        #endregion

        /// <summary>
        /// The number of hours between activations for this trigger.
        /// </summary>
        protected int Hours
        {
            get { return (int)(base.Ticks / TimeSpan.TicksPerHour); }
        }

        /// <summary>
        /// The meta data describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                // EveryNMinutes gets it mostly right, but the interval and
                // period need to be changed...
                meta.Interval = IntervalType.Hour;
                meta.Period = Hours;
                return meta;
            }
        }
    }
}
