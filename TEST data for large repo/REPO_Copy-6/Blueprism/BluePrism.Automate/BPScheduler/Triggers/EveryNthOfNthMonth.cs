using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which fires on the same day of the month every 'n' months.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNthOfNthMonth : PeriodicTrigger
    {
        /// <summary>
        /// The policy to use if the date does not exist for a particular
        /// month (eg. 30th February).
        /// </summary>
        [DataMember]
        private NonExistentDatePolicy _policy;

        /// <summary>
        /// Creates a new trigger which activates every 'period' months after
        /// the start date, using the given policy if such a date does not 
        /// exist for a particular month.
        /// </summary>
        /// <param name="period">The number of months between activations.
        /// </param>
        /// <param name="policy">The policy to use if the day of the month
        /// in the start date doesn't exist for a particular month.</param>
        public EveryNthOfNthMonth(int period, NonExistentDatePolicy policy)
            : base(period)
        {
            _policy = policy;
        }

        /// <summary>
        /// Gets the next instance of this trigger after the given date/time.
        /// </summary>
        /// <param name="after">The date/time after which the next trigger
        /// instance is required.</param>
        /// <returns>The first instance of this trigger which is due to 
        /// activate after the given date/time.</returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            // first we add 'Period' months to the start date until we hit (or overshoot)
            // the month that 'after' is in.
            DateTime date = Start;

            // We need this so that we can ignore skipped months without hitting the
            // fact that 
            DateTime lastResult = date;
            int month = date.Month;
            int year = date.Year;

            while (date <= after)
            {
                // get the next month/year to try:
                // note we must do it this way round to ensure that 'month' hasn't changed
                // before the year calculation
                year = (year + ((month + Period - 1) / 12));
                month = ((month + Period - 1) % 12) + 1;

                DateTime result = GetRunTimeForMonth(month, year);
                // if we get DateTime.MinValue, that means that the date didn't 
                // exist for 'month'

                if (result != DateTime.MinValue) // we're *not* skipping this month
                    date = result;

                // if we've hit our end time.... return no instances...
                // First check works if this month is not being skipped - 
                if (date > End)
                    return null;
                // second is vaguer because we're skipping a month... and
                // have no specific date to check.
                // Note that if we get it wrong this time, then we'll catch it
                // definitely after the next iteration... it just means we loop
                // one more time than is optimal - the result is the same
                if (result == DateTime.MinValue &&
                    (year > End.Year || (year == End.Year && month == End.Month)))
                {
                    return null;
                }
            }
            // Found a date which is past 'after' and before 'End'...
            return CreateInstance(date);
        }

        /// <summary>
        /// <para>
        /// Gets the date/time that this trigger would fire in the given month
        /// of the specified year, or DateTime.MinValue if it cannot run in
        /// that month/year (eg. it's set to run on 30th, month==2 and the
        /// policy indicates such dates should 'Skip').
        /// </para><para>
        /// Note that this does <em>not</em> check the period to see if this
        /// trigger should run in the specified month - only if the date on
        /// which it would run exists in the specified month
        /// </para>
        /// </summary>
        /// <param name="month">The month (1=Jan, 2=Feb, etc) for which the date
        /// of the instance of this trigger is required.</param>
        /// <param name="year">The year for which the date of the instance of
        /// this trigger is required.</param>
        /// <returns>A DateTime detailing the activation time for the given
        /// month of the given year. This will return DateTime.MinValue if there
        /// is no runtime for that month.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the given month is
        /// not between 1 and 12 (inclusive), the given year is not between 1
        /// and 9999 (inclusive) or if the run time for the specified month
        /// takes the calculation before DateTime.MinValue or after 
        /// DateTime.MaxValue</exception>
        private DateTime GetRunTimeForMonth(int month, int year)
        {
            // okay - see when the datetime would be for that month.
            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (Start.Day <= daysInMonth)
            {
                // good to go
                return new DateTime(year, month, Start.Day).Date + Start.TimeOfDay;
            }
            // otherwise we're into awkwardness...
            // check the policy to see what to do here..
            switch (_policy)
            {
                case NonExistentDatePolicy.Skip:
                default:
                    return DateTime.MinValue;

                case NonExistentDatePolicy.LastSupportedDayInMonth:
                    return new DateTime(year, month, daysInMonth).Date + Start.TimeOfDay;

                case NonExistentDatePolicy.FirstSupportedDayInNextMonth:
                    return new DateTime(year, month + 1, 1).Date + Start.TimeOfDay;
            }
        }

        /// <summary>
        /// The single metadata object describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Month;
                meta.MissingDatePolicy = _policy;
                return meta;
            }
        }
    }
}
