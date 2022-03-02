using System;
using System.Runtime.Serialization;
using System.Text;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Base class to use for a trigger instance.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class BaseTriggerInstance : ITriggerInstance
    {
        [DataMember]
        private BaseTrigger _trig;
        
        [DataMember]
        private DateTime _when;

        [DataMember]
        private TriggerMode _mode;

        /// <summary>
        /// The trigger that this is an instance of.
        /// </summary>
        public ITrigger Trigger
        {
            get { return _trig; }
        }
       
        /// <summary>
        /// The point in time that this instance covers
        /// </summary>
        public DateTime When
        {
            get { return _when; }
        }
        
        /// <summary>
        /// The mode that is effective for this instance.
        /// </summary>
        public TriggerMode Mode
        {
            get { return _mode; }
        }
        
        /// <summary>
        /// Creates a new trigger instance with the specified parameters
        /// </summary>
        /// <param name="trig">The trigger that this is an instance of</param>
        /// <param name="when">The point in time that this trigger is effective</param>
        /// <param name="mode">The mode in which this instance activates</param>
        public BaseTriggerInstance(BaseTrigger trig, DateTime when, TriggerMode mode)
        {
            _trig = trig;
            _when = when;
            _mode = mode;
        }

        /// <summary>
        /// Checks if this instance has been activated or not.
        /// </summary>
        /// <returns>true if the trigger was activated via this instance, false 
        /// otherwise.</returns>
        public virtual bool HasActivated()
        {
            ISchedule sched = Trigger.Schedule;
            return sched.Owner.Store.GetLog(sched, When) != null;
        }

        /// <summary>
        /// Flag indicating whether this instance has a valid activation time.
        /// An activation time is deemed invalid when it falls between a time zone's
        /// spring forward gap, i.e. when its clock move forward into daylight
        /// savings time.
        /// </summary>
        public virtual bool IsTimeValid()
        {
            // 'run now' instances will always have a valid time
            if (!Trigger.IsUserTrigger)
            {
                return true;
            }

            var data = Trigger.PrimaryMetaData;
            var adjustForDaylightSavings = data.TimeZoneId != null && data.UtcOffset == null;
            return !(adjustForDaylightSavings && TimeZoneInfo.FindSystemTimeZoneById(data.TimeZoneId).IsInvalidTime(When));
        }

        /// <summary>
        /// Gets a string representation of this trigger instance.
        /// This is just the scheduler name and the date and time for which
        /// this instance was triggered.
        /// </summary>
        /// <returns>A string description of this trigger instance.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append(Trigger.Schedule.Name);
            }
            catch (NullReferenceException)
            {
                // generally speaking, an instance would always be part of
                // a schedule, but just in case it isn't - ToString() should
                // not be throwing exceptions if it's not assigned to a
                // schedule at the moment.
                sb.Append("{No Schedule}");
            }
            sb.Append(" : ").Append(When.ToString("[dd/MM/yyyy HH:mm:ss]"));
            return sb.ToString();

        }
    }
}
