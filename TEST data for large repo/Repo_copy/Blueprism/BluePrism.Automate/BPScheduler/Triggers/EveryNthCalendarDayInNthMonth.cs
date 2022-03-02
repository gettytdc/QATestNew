using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which runs monthly on the nth valid day within a calendar.
    /// This can be used to run a trigger on the last working day (where the
    /// calendar keeps track of the working days).
    /// It will currently throw an exception if for a month that it is due
    /// to be activated, the calendar allows no valid activation days.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public abstract class EveryNthCalendarDayInNthMonth : PeriodicTrigger
    {
        /// <summary>
        /// The calendar to use to ascertain which date this trigger should
        /// activate.
        /// </summary>
        protected abstract ICalendar Calendar { get; }

        /// <summary>
        /// Which iteration of a valid day to activate a trigger.
        /// </summary>
        [DataMember]
        private NthOfMonth _nth;

        /// <summary>
        /// Creates a new trigger set to fire every month on the given iteration
        /// of a valid day within the specified calendar.
        /// </summary>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate. A value of <see cref="NthOfMonth.None"/> will create
        /// a trigger which will never activate.</param>
        public EveryNthCalendarDayInNthMonth(NthOfMonth nth) : this(1, nth) { }

        /// <summary>
        /// Creates a new trigger set to fire every n months on the given iteration
        /// of a valid day within the specified calendar.
        /// </summary>
        /// <param name="period">The period of months between trigger activations
        /// for this trigger.</param>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate. A value of <see cref="NthOfMonth.None"/> will create
        /// a trigger which will never activate.</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public EveryNthCalendarDayInNthMonth(int period, NthOfMonth nth)
            : base(period)
        {
            _nth = nth;
        }

        /// <summary>
        /// The single meta data object describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Month;
                meta.Nth = _nth;
                // tell the calendar to write its metadata properties to the
                // metadata and return the resultant data.
                Calendar.UpdateMetaData(meta);
                return meta;
            }
        }

        /// <summary>
        /// Gets the next instance of this trigger after the given date.
        /// </summary>
        /// <param name="after">The date after which the next activation of this
        /// trigger is required.</param>
        /// <returns>The instance detailing the next activation time for this
        /// trigger, or null if there are no more trigger instances after the
        /// given date/time.</returns>
        /// <exception cref="NoValidActivationTimeException">If, for a month in
        /// which activation is expected, the calendar did not allow any days
        /// in which to activate it.</exception>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            // quick sanity check
            if (_nth == NthOfMonth.None)
                return null;

            // another sanity check (DateTime.AddMonths() can't cope with 120000+)
            if (Period > 119999)
                return null;

            // Okay, we need to build up from the start date by Period
            // months each time until we reach/pass the month that after is in.

            // Disregard the day of the month to determine the month to begin the
            // trigger activations. To do this, we go from the 1st of the month and
            // compare against the first of the month for the boundary date

            // We also disregard the time component at this point
            DateTime date = Start.AddDays(-(Start.Day - 1)).Date;
            DateTime targetDate = after.AddDays(-(after.Day - 1)).Date;

            try
            {
                // We want to disregard the day of the month while we're looking
                // for the first month to run..
                while (date < targetDate)
                {
                    // add months is safe from the 1st of the month...
                    date = date.AddMonths(Period);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Thrown if date.AddMonths() sends the date past DateTime.MaxValue.
                // Effectively, there is no next instance
                return null;
            }

            DateTime boundary = (this.End < TriggerBounds.LatestSupportedDate
                ? this.End
                : TriggerBounds.LatestSupportedDate);

            // so now we're either on the month that after's on or past it.
            // Find the run time for this month...
            DateTime? result = GetActivationTimeFor(date.Month, date.Year);

            // Check that this is past 'after' (or, indeed, past the start date since
            // we reset to the first of the month before we started)
            // If not, just get the next valid month (ie. date.Month + Period)
            while (!result.HasValue || result.Value <= after || result.Value < Start)
            {
                // If we've overshot the end of the trigger, might as well return
                // nothing now, since we're going to anyway.
                if ((result.HasValue && result.Value > boundary) || date > boundary)
                    return null;

                try
                {
                    date = date.AddMonths(Period);
                    result = GetActivationTimeFor(date.Month, date.Year);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // we've gone over DateTime.MaxValue ergo no more instances
                    return null;
                }
            }

            // If the actual result is past the end date, return nothing
            if (result.Value > boundary)
                return null;

            // otherwise, create an instance from the resultant date.
            return CreateInstance(result.Value);
        }

        /// <summary>
        /// Gets the activation time for this trigger within the given month and year.
        /// </summary>
        /// <param name="month">The month for which the activation time is required.
        /// </param>
        /// <param name="year">The year for which the activation time is required.
        /// </param>
        /// <returns>The point in time within the given month and year at which this
        /// trigger will be activated, or null if the calendar set within this class
        /// didn't allow any dates within the specified month/year to activate this
        /// trigger.</returns>
        private DateTime? GetActivationTimeFor(int month, int year)
        {
            // date: The working date
            DateTime date;

            // dayModifier: how many days to add on each iteration to find cal day : 1 or -1
            int dayModifier;

            // target: How many valid days to count before we've found our target
            int target;

            if (_nth == NthOfMonth.Last)
            {
                // start from last day of the month
                date = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                // we're working backwards
                dayModifier = -1;
                // target is always the first one we find.
                target = 1;
            }
            else
            {
                // start from 1st of month
                date = new DateTime(year, month, 1);
                // And we're working forwards.
                dayModifier = 1;
                // if not last (or none) First = 1, Second = 2 etc.
                target = (int)_nth;
            }

            // make sure the time of day is set on the date.
            date = date.Date + Start.TimeOfDay;

            // Go through the dates, increment our count when we find a valid day
            // in the calendar and return the resultant date if we reach the target.
            // Come out of the loop if we find ourselves in a different month..
            int count = 0;
            while (date.Month == month)
            {
                if (Calendar.CanRun(date) && ++count >= target)
                    return date;
                date = date.AddDays(dayModifier);
            }

            // if we're here, then we ran out of month...
            // not a lot we can do, so fail to do even that
            return null;
        }
    }
}
