using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class StopTrigger : BaseTrigger
    {
        public StopTrigger(DateTime when)
            : this(TriggerMode.Stop, when) { }

        public StopTrigger(TriggerMode mode, DateTime when)
            : base(mode, DEFAULT_PRIORITY, when, when) { }


        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            if (after >= Start)
                return null;

            return CreateInstance(Start);
        }

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
