using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is set to fire once.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class OnceTrigger : BaseTrigger
    {
        /// <summary>
        /// Creates a new trigger set to fire at the specified time, if the calendar
        /// allows it.
        /// </summary>
        /// <param name="when">The point in time that this trigger is to be fired.
        /// </param>
        public OnceTrigger(DateTime when)
            : this(TriggerMode.Fire, when) { }

        /// <summary>
        /// Creates a new trigger set to fire or suppress at the specified time,
        /// if the job's calendar allows it.
        /// </summary>
        /// <param name="mode">The mode with which any instances of this trigger
        /// should be created.</param>
        /// <param name="when">The point in time that this trigger is to be 
        /// activated.</param>
        public OnceTrigger(TriggerMode mode, DateTime when)
            : base(mode, DEFAULT_PRIORITY, when, when) { }

        /// <summary>
        /// Gets the next instance that this trigger is fired. This will be
        /// at the time specified in the contructor or will return null if
        /// the time specified is before the given <paramref name="after"/>
        /// parameter.
        /// </summary>
        /// <param name="cal">The calendar to check to see if this instance
        /// should run.</param>
        /// <param name="after"></param>
        /// <returns></returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            if (after >= Start) // the 'once' has already passed by this point...
                return null;

            return CreateInstance(Start);
        }

        /// <summary>
        /// Gets the single metadata object which defines this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Once;
                return meta;
            }
        }
    }
}
