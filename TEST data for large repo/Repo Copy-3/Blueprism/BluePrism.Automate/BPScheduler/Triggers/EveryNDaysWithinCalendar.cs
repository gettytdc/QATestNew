using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n days if the day falls on a day
    /// within a predefined day set.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public abstract class EveryNDaysWithinCalendar : EveryNDays
    {
        /// <summary>
        /// The calendar containing the valid days on which trigger activation
        /// is allowed.
        /// </summary>
        protected abstract ICalendar Calendar { get; }

        /// <summary>
        /// Creates a new trigger which activates every n days, if that falls
        /// on a day within the given day set.
        /// This will add <paramref name="period"/> days repeatedly until the
        /// date falls on a day within the given day set.
        /// </summary>
        /// <param name="period">The number of days between instances of this
        /// trigger.</param>
        public EveryNDaysWithinCalendar(int period) : base(period) { }

        /// <summary>
        /// Checks if a trigger activation is allowed at the given point in time.
        /// In this instance, it is allowed if the day of the week falls on a day
        /// specified in the contained day set.
        /// </summary>
        /// <param name="date">The date to check to see if it is allowed.</param>
        /// <returns>true if the given date falls on a day allowed by the day set
        /// held within this object; false otherwise.</returns>
        protected override ActivationAllowed Allows(DateTime date)
        {
            if(Calendar != null && !Calendar.HasAnyWorkingDays())
            {
                return ActivationAllowed.NeverAllowed;
            }
            return (Calendar == null || Calendar.CanRun(date)) ? ActivationAllowed.Allowed : ActivationAllowed.NotAllowed;
        }

        /// <summary>
        /// The meta data describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                Calendar.UpdateMetaData(meta);
                return meta;
            }
        }

    }
}
