using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Simple trigger which is never fired.
    /// This can be used as a placeholder for a job which has no assigned
    /// triggers.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference=true)]
    public sealed class NeverTrigger : BaseTrigger
    {
        /// <summary>
        /// Gets the next lack of instance for this trigger.
        /// </summary>
        /// <param name="after">The date after which no instances should be
        /// fired.</param>
        /// <returns>null. This trigger is never fired.</returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            return null;
        }

        /// <summary>
        /// Gets the single metadata object which defines this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Never;
                return meta;
            }
        }
    }
}
