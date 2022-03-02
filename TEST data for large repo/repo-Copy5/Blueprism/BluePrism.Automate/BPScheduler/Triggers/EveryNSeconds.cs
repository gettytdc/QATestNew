using BluePrism.Scheduling.Properties;
using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Simple trigger which fires every n seconds.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNSeconds : BaseTrigger
    {
        /// <summary>
        /// The number of Ticks (note: *not* seconds) to wait in between 
        /// trigger activations.
        /// </summary>
        [DataMember]
        private long _interval;

        /// <summary>
        /// Creates a new trigger which fires every n seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds for the trigger
        /// firing period.</param>
        public EveryNSeconds(long seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentException(string.Format(Resources.InvalidIntervalProvided0, seconds), nameof(seconds));
            }
            this._interval = (seconds * TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// The number of Ticks interval between which this trigger is activated.
        /// </summary>
        protected long Ticks
        {
            get { return _interval; }
        }

        /// <summary>
        /// The number of seconds between trigger activations.
        /// </summary>
        protected int Seconds
        {
            get { return (int)(_interval / TimeSpan.TicksPerSecond); }
        }

        /// <summary>
        /// The primary metadata for this trigger
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Second;
                meta.Period = Seconds;
                return meta;
            }
        }

        /// <summary>
        /// Used by the Allows() method to indicate if activation of the trigger is allowed
        /// </summary>
        protected enum ActivationAllowed
        {
            Allowed, // if the subclass allows activation at this point in time
            NotAllowed, // if the subclass does not allow activation at this point in time
            NeverAllowed // if the subclass does not allow activation at any point in time - there are no working days in the calendar
        }

        /// <summary>
        /// Final check to see if any subclasses require a further filter on the
        /// given date/time.
        /// This allows subclasses to define ranges where activation is allowed,
        /// or other conditions, such as calendar checks.
        /// </summary>
        /// <param name="date">The point in time to check to see if an
        /// activation should be allowed.</param>
        /// <returns>ActivationAllowed to indicate if activation is allowed at this point in time
        /// </returns>
        protected virtual ActivationAllowed Allows(DateTime date)
        {
            return ActivationAllowed.Allowed;
        }

        /// <summary>
        /// Gets the next point in time that this trigger is activated after
        /// the given date/time.
        /// </summary>
        /// <param name="cal">The calendar to use when determining the 
        /// trigger's event prediction.</param>
        /// <param name="after">The datetime after which the next trigger
        /// activation time is required.</param>
        /// <returns>The next point in time that this trigger is activated, 
        /// or DateTime.MaxValue if it is never again activated.</returns>
        public override ITriggerInstance GetNextInstance(DateTime afterDate)
        {
            // next sanity check - is after > this.End?
            if (afterDate > End) // we're done here.
                return null;

            // Okay, let's do this in ticks (100-nanosecond intervals).
            // Far easier to read than arsing about with DateTimes and TimeSpans.
            long after = afterDate.Ticks;
            long start = Start.Ticks;

            // Get the end date, if it's not set, don't let it run for EVER as that
            // will pretty much infinite loop, so 10 years max to find next trigger point.
            long end =  (End == DateTime.MaxValue) ?  afterDate.AddYears(10).Ticks : End.Ticks;

            // calc how many completed intervals are between start and afterDate and add 1.
            long ticksBetween = after - start;
            long curr = start;
            if (ticksBetween >= 0)
            {
                long intervalsBetween = ticksBetween / _interval;
                curr = start + ((intervalsBetween + 1) * _interval);
            }

            // check we've not gone past the end.
            if (curr > end) return null;

            // if we're here, it means that we've found a date that is beyond 'after'
            // and before this trigger's end date. Now we have to work with any 
            // filtering within subclasses in order to find a date which is allowed
            // by those too.
            try
            {
                if (Allows(new DateTime(curr)) == ActivationAllowed.NeverAllowed)
                    return null;

                while (Allows(new DateTime(curr)) == ActivationAllowed.NotAllowed)
                {
                    curr += _interval;
                    // Again, check if 'current' date is past the end date
                    if (curr > end)
                        return null;
                }

                // Now we have a time which has meets all requirements.
                // Use the current mode set in the trigger for the generated instance
                return CreateInstance(new DateTime(curr));
            }
            catch (ArgumentOutOfRangeException) // ie. beyond DateTime.MaxValue
            {
                return null;
            }
        }

    }
}
