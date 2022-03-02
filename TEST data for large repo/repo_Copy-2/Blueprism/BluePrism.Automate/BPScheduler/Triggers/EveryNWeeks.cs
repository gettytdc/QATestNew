using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger initialised to fire every n weeks
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNWeeks : EveryNSeconds
    {
        /// <summary>
        /// Creates a new trigger which fires at a period of the given
        /// number of weeks.
        /// </summary>
        /// <param name="weeks">The number of weeks that this trigger should
        /// use as its firing period.</param>
        public EveryNWeeks(int weeks) : base(weeks * 7L * 24L * 60L * 60L) { }

        /// <summary>
        /// The number of weeks between activations for this trigger.
        /// </summary>
        protected int Weeks
        {
            get { return (int)(base.Ticks / (7L * TimeSpan.TicksPerDay)); }
        }

        /// <summary>
        /// Gets the single metadata object that describes this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Week;
                meta.Period = Weeks;
                return meta;
            }
        }
    }
}
