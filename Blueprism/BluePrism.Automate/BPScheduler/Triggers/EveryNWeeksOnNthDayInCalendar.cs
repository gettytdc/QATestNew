using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n weeks on days defined in a particular
    /// calendar.
    /// Note that we can't simply use a constant interval on this because the
    /// calendar could upset any interval that we may define.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public abstract class EveryNWeeksOnNthDayInCalendar : PeriodicTrigger
    {
        /// <summary>
        /// The 'n' in 'nth day in calendar'.
        /// </summary>
        [DataMember]
        private NthOfWeek _nthOfWeek;

        /// <summary>
        /// The calendar which is used to determine activation times
        /// for this trigger.
        /// </summary>
        protected abstract ICalendar Calendar { get; }

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calender every week.
        /// </summary>
        protected EveryNWeeksOnNthDayInCalendar(NthOfWeek nth) : this(1, nth) { }

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calender every n weeks.
        /// </summary>
        /// <param name="period">The number of weeks between trigger
        /// activations for this trigger.</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        protected EveryNWeeksOnNthDayInCalendar(int period, NthOfWeek nth)
            : base(period)
        {
            _nthOfWeek = nth;
        }

        private DateTime GetWeekStart(DateTime weekStart)
        {
            // Go back from the start until we reach the first day of the week
            while (weekStart.DayOfWeek != DaySet.FirstDayOfWeek)
            {
                try
                {
                    weekStart = weekStart.AddDays(-1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // We crossed the DateTime.MinValue boundary... erk.
                    // well for now.. just rethrow.. I might clean this up so
                    // that it works for dates near to the epoch, but for now...
                    throw;
                }
            }
            return weekStart;
        }

        private List<DateTime> GetFollowingSevenValidDays(DateTime start)
        {
            var days = new List<DateTime>(7);
            // Check on each day of this week
            for (int i = 0; i < 7; i++)
            {
                DateTime day = start.AddDays(i);

                if (Calendar.CanRun(day))
                    days.Add(day);
            }
            return days;
        }


        /// <returns>Null if a valid instance is not found which can be checked for</returns>
        private ITriggerInstance TryGetTrigger(List<DateTime> validDatesInWeek, DateTime earliestValidTime)
        {
            if (validDatesInWeek.Count > 0) // else... none there, continue...
            {
                // if the last day in the collection is before the earliestValidTime
                // just move on... 
                DateTime lastValidDate = validDatesInWeek[validDatesInWeek.Count - 1];
                if (lastValidDate >= earliestValidTime)
                {
                    // Otherwise, this might be our boy...
                    if (_nthOfWeek == NthOfWeek.Last)
                    {
                        return CreateInstance(lastValidDate);
                    }
                    else if (_nthOfWeek == NthOfWeek.None)
                    {
                        for (int i = 0; i < validDatesInWeek.Count; i++)
                        {
                            DateTime dt = validDatesInWeek[i];
                            if (dt >= earliestValidTime)
                                return CreateInstance(dt);
                        }
                        // okay, so lastValidDate >= earliestValidTime, but
                        // validDatesInWeek[count-1] < earliestValidTime... nooooo....
                        throw new InvalidOperationException(
                            "Invalid State entered in schedule trigger : " + this);
                    }
                    // NthOfWeek maps quite cleanly onto ints,
                    // other than Last or None (dealt with above), you can
                    // just subtract 1 from the int value to get the index 
                    // in the list.
                    int index = ((int)_nthOfWeek) - 1;
                    if (index < validDatesInWeek.Count)
                    {
                        DateTime dt = validDatesInWeek[index];
                        if (dt >= earliestValidTime)
                            return CreateInstance(dt);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the next instance of this trigger which is due to activate
        /// after the specified point in time.
        /// </summary>
        /// <param name="after">The point in time after which the next instance
        /// of this trigger is activated.</param>
        /// <returns>The next activation instance of this trigger after the given
        /// date or null if it is not to be activated again.</returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            // Okay, we need to find the beginning of the week..
            DateTime weekStart = Start;

            // Set our 'bottom date' - that is the earliest date that we can have as a 
            // valid value. Thus, the latest of the trigger's start date and the 'after'
            // parameter that we're searching for (plus 1s because after is exclusive).
            DateTime earliestValidTime = after.AddSeconds(1);
            if (Start > earliestValidTime) earliestValidTime = Start;

            weekStart = GetWeekStart(weekStart);

            // OK - weekStart points to the first day of the first week within the
            // start date's week
            // So, for a given week, we want to check each day and see if the
            // calendar allows it to be run on that day or not.
            try
            {
                while (weekStart <= End)
                {
                    var validDatesInWeek = GetFollowingSevenValidDays(weekStart);

                    var potentialTrigger = TryGetTrigger(validDatesInWeek, earliestValidTime);

                    if (potentialTrigger != null)
                        return potentialTrigger;
                    
                    // scoot on the specified number of weeks to the
                    // next start of week we need to check.
                    weekStart = weekStart.AddDays(Period * 7);
                }
                // we reached the end date without finding a valid date
                return null;
            }
            catch (ArgumentOutOfRangeException)
            {
                // we overran DateTime.MaxValue - ie. no more instances.
                return null;
            }
        }

        /// <summary>
        /// Gets the single meta data which can be used to describe this
        /// trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Week;
                meta.NthOfWeek = _nthOfWeek;
                Calendar.UpdateMetaData(meta);
                return meta;
            }
        }
    }
}
