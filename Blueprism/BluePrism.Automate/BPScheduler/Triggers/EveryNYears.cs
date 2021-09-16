using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which activates on the same date every n years.
    /// This requires a <see cref="NonExistentDatePolicy"/> set which can
    /// decide how to handle a year for which that date doesn't exist. If
    /// none is specified, it will just skip the instance that year.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNYears : PeriodicTrigger
    {
        [DataMember]
        private NonExistentDatePolicy _policy;

        /// <summary>
        /// Creates a new trigger which is activated every n years, unless 
        /// the date on which it would activate does not exist for a particular
        /// year.
        /// </summary>
        /// <param name="period">The number of years between activations.</param>
        public EveryNYears(int period)
            : this(period, NonExistentDatePolicy.Skip) { }

        /// <summary>
        /// Creates a new trigger which is activated every n years, and will
        /// follow the specified policy on how to handle dates which don't
        /// exist for a particular year.
        /// </summary>
        /// <param name="period">The number of years between activations.
        /// </param>
        /// <param name="policy">The policy to enact when an activation date
        /// does not exist for a particular year.</param>
        public EveryNYears(int period, NonExistentDatePolicy policy)
            : base(period)
        {
            _policy = policy;
        }

        /// <summary>
        /// Gets the next point in time that this trigger is activated after 
        /// the given date/time.
        /// </summary>
        /// <param name="after">The datetime after which the next trigger 
        /// activation time is required.</param>
        /// <returns>The next instance of this trigger which is due to be 
        /// activated after the given point in time, or null if it is never 
        /// again activated.</returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            try
            {
                DateTime curr = DateTime.MinValue;
                // Keep incrementing the number of years forward we're
                // looking at until we go past 'after' or we pass the
                // 'End' date on this trigger.
                for (int i = 0; curr <= after; i += Period)
                {
                    // note: this may return MinValue if no instance this year and skipping
                    curr = AddYears(Start, i);               
                    if (curr > End) // Have we gone over the trigger's bounds?
                        return null;
                }
                // We got past 'after' and landed before (or on) 'End'
                // Looks like we've got ourselves an instance.
                return CreateInstance(curr);
            }
            catch (ArgumentOutOfRangeException)
            {
                // We've gone over the bounds of DateTime.MaxValue - no more instances
                return null;
            }
        }

        /// <summary>
        /// Adds the given number of years to the given date, taking into account
        /// the NonExistentDatePolicy set in this object and returns the resultant
        /// date/time. This will return DateTime.MinValue if the date within that
        /// year doesn't exist and the policy is set to 'Skip'.
        /// </summary>
        /// <param name="date">The date to which a number of years should be added.
        /// </param>
        /// <param name="years">The number of years to add.</param>
        /// <returns>The date time representing the given date/time with the
        /// specified number of years added, taking into account the policy to use
        /// when such a date doesn't exist.</returns>
        private DateTime AddYears(DateTime date, int years)
        {
            int targetYear = date.Year + years;
            DateTime resultDate;

            // Check that the date exists first
            // If it's not Feb, or if the day falls in Feb for that year,
            // then it's okay and we don't need to consult the policy
            if (date.Month != 2 || date.Day <= DateTime.DaysInMonth(targetYear, 2))
            {
                resultDate = new DateTime(targetYear, date.Month, date.Day);
            }
            else // Okay - the date is > 28th Feb and it's not a leap year. Check the policy
            {
                switch (_policy)
                {
                    case NonExistentDatePolicy.Skip:
                    default:
                        return DateTime.MinValue;

                    case NonExistentDatePolicy.FirstSupportedDayInNextMonth:
                        resultDate = new DateTime(targetYear, date.Month + 1, 1);
                        break;

                    case NonExistentDatePolicy.LastSupportedDayInMonth:
                        resultDate = new DateTime(targetYear, date.Month,
                            DateTime.DaysInMonth(targetYear, date.Month));
                        break;

                }
            }

            // Date's good - now we need to match the time
            return resultDate.Date + date.TimeOfDay;
        }

        /// <summary>
        /// Gets the single metadata object which represents this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Year;
                meta.MissingDatePolicy = _policy;
                return meta;
            }
        }
    }
}
