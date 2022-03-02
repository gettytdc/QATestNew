using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which fires every n days
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNDays : EveryNSeconds
    {
        /// <summary>
        /// Creates a new trigger which fires every n days.
        /// </summary>
        /// <param name="days">The number of days defining the period between
        /// trigger instances being fired.</param>
        public EveryNDays(int days) : base(days * 24L * 60L * 60L) { }

        /// <summary>
        /// The number of days between activations for this trigger
        /// </summary>
        protected int Days
        {
            get { return (int)(Ticks / TimeSpan.TicksPerDay); }
        }

        /// <summary>
        /// The meta data describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Day;
                meta.Period = Days;
                return meta;
            }
        }
    }
}
