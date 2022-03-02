using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// trigger which fires a specified number of hours apart.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNHours : EveryNSeconds
    {
        /// <summary>
        /// Creates a new trigger which fires every n hours
        /// </summary>
        /// <param name="hours">The number of hours to wait in between
        /// this trigger firing.</param>
        public EveryNHours(int hours) : base(hours * 60L * 60L) { }

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
                meta.Interval = IntervalType.Hour;
                meta.Period = Hours;
                return meta;
            }
        }

    }
}
