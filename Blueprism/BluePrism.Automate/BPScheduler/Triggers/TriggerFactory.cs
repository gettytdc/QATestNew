using System;
using BluePrism.Scheduling.Calendar;
using BluePrism.Scheduling.ScheduleData;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Factory class with which triggers are created from DataRows
    /// </summary>
    public class TriggerFactory
    {
        /// <summary>
        /// The single instance of this factory class
        /// </summary>
        private static readonly TriggerFactory INSTANCE = new TriggerFactory();

        /// <summary>
        /// Gets the instance of this factory class to use to create triggers.
        /// </summary>
        /// <returns>The single instance of this factory class.</returns>
        public static TriggerFactory GetInstance()
        {
            return INSTANCE;
        }

        /// <summary>
        /// Creates a trigger object which is set to activate using the values
        /// in the given data row.
        /// </summary>
        /// <param name="row">The row whose data provides the trigger which
        /// needs to be created.</param>
        /// <returns>An ITrigger instance which is configured to activate at
        /// the times specified in the data row.</returns>
        public ITrigger CreateTrigger(ScheduleTriggerDatabaseData triggerData)
        {
            return CreateTrigger(new TriggerMetaData(triggerData));
        }

        /// <summary>
        /// Creates a trigger object which is set to activate using the values
        /// in the given metadata.
        /// 
        /// </summary>
        /// <param name="data">The metadata describing the trigger which is
        /// required.</param>
        /// <returns>An ITrigger instance which is configured to activate at
        /// the times specified in the metadata.</returns>
        /// <exception cref="ArgumentNullException">If the given metadata
        /// object was null.</exception>
        /// <exception cref="InvalidDataException">If the given metadata did
        /// not accurately describe a supported trigger.</exception>
        public ITrigger CreateTrigger(TriggerMetaData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            BaseTrigger trig;

            // The interval is the main arbiter for what type of trigger is
            // created.
            switch (data.Interval)
            {
                case IntervalType.Never:
                    trig = new NeverTrigger();
                    break;

                case IntervalType.Once:
                    trig = new OnceTrigger(data.Mode, data.Start);
                    break;

                case IntervalType.Minute:
                    trig = new EveryNMinutesWithinRange(
                        data.Period, data.AllowedHours, data.CalendarId);
                    break;

                case IntervalType.Hour:
                    trig = new EveryNHoursWithinRange(
                        data.Period, data.AllowedHours, data.CalendarId);
                    break;

                case IntervalType.Day:
                    // If there's a calendar there, use it.
                    if (data.CalendarId == 0)
                    {
                        trig = new EveryNDays(data.Period);
                    }
                    else
                    {
                        trig = new EveryNDaysWithinIdentifiedCalendar(
                            data.Period, data.CalendarId);
                    }
                    break;

                case IntervalType.Week:
                    // right, awkwardness kicks in here.
                    if (data.CalendarId == 0) // no specified calendar - assume days of week
                    {
                        trig = new EveryNWeeksOnNthDayInKnownCalendar(
                            data.Period, NthOfWeek.First, new DaySetCalendar(data.Days));
                    }
                    else
                    {
                        trig = new EveryNWeeksOnNthDayInIdentifiedCalendar(
                            data.Period, data.NthOfWeek, data.CalendarId);
                    }
                    break;

                case IntervalType.Month:
                    // And the awkwardness continues with gusto here.
                    // OK, if we have a DaySet defined then we're looking at the
                    // "nth" day within the set... you know what to do....
                    if (!data.Days.IsEmpty())
                    {
                        trig = new EveryNthKnownCalendarDayInNthMonth(
                            data.Period, data.Nth, new DaySetCalendar(data.Days));
                    }
                    // likewise, if we have a calendar defined, then it's the
                    // nth day within the calendar.
                    else if (data.CalendarId != 0)
                    {
                        trig = new EveryNthIdentifiedCalendarDayInNthMonth(
                            data.Period, data.Nth, data.CalendarId);
                    }
                    // otherwise, it's on the nth of the month... 
                    else
                    {
                        trig = new EveryNthOfNthMonth(data.Period, data.MissingDatePolicy);
                    }
                    break;

                case IntervalType.Year:
                    // Every n years
                    trig = new EveryNYears(data.Period,
                        NonExistentDatePolicy.LastSupportedDayInMonth);
                    break;

                // These aren't really supported by the calling code just yet,
                // but they are supported by the architecture and the enum,
                // so we'd best deal with them regardless
                case IntervalType.Second:
                    trig = new EveryNSeconds(data.Period);
                    break;

                // Anything else is just a nonsense.
                default:
                    throw new InvalidDataException("Invalid interval type: " + data.Interval);
            }

            trig.Mode = data.Mode;
            trig.Priority = data.Priority;
            trig.Start = data.Start;
            trig.End = data.End;
            trig.IsUserTrigger = data.IsUserTrigger;
            trig.TimeZoneId = data.TimeZoneId;
            trig.UtcOffset = data.UtcOffset;

            return trig;
        }
    }
}
