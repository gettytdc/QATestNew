using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which repeats after a certain period - the type that the period
    /// represents is defined in the context of the concrete subclass, but the
    /// period can be represented by an int (ie. not ticks or milliseconds)
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public abstract class PeriodicTrigger : BaseTrigger
    {
        /// <summary>
        /// The period after which this trigger should fire.
        /// </summary>
        [DataMember]
        private int _period;

        /// <summary>
        /// Creates a new periodic trigger which activates after the given 
        /// period.
        /// </summary>
        /// <param name="period">The number of time units that this trigger should
        /// wait between activations. The unit of time used is not defined within
        /// this class.</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public PeriodicTrigger(int period)
        {
            this.Period = period;
        }

        /// <summary>
        /// The number of time units that this trigger should
        /// wait between activations.
        /// </summary>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public int Period
        {
            get { return _period; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Period must be a positive integer");

                _period = value;
            }
        }

        /// <summary>
        /// Gets the single metadata object representing this trigger.
        /// At this point, it only adds the period data to the metadata defined
        /// in the base trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Period = Period;
                return meta;
            }
        }
    }
}
