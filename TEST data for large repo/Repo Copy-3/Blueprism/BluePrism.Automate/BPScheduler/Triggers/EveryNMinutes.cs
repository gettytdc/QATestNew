using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Simple class which fires every n minutes.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNMinutes : EveryNSeconds
    {
        /// <summary>
        /// Creates a new trigger which fires every n minutes.
        /// </summary>
        /// <param name="minutes"></param>
        public EveryNMinutes(long minutes) : base(minutes * 60) { }

        /// <summary>
        /// The number of minutes between trigger activations for this trigger.
        /// </summary>
        protected int Minutes
        {
            get { return (int)(base.Ticks / TimeSpan.TicksPerMinute); }
        }

        /// <summary>
        /// The single meta data object which describes this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Minute;
                meta.Period = Minutes;
                return meta;
            }
        }
    }
}
