using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib.Collections;
using System.Linq;

namespace BluePrism.Scheduling.Triggers
{
    [Serializable, DataContract(Namespace = "bp", IsReference = true), KnownType(typeof(HashSet<ITrigger>))]
    public class TriggerGroup : BaseTrigger, ITriggerGroup, ICollection<ITrigger>
    {

        #region PriorityComparator inner class

        /// <summary>
        /// Comparer which compares the priority of 2 triggers. This also takes
        /// into account that 'suppress' triggers are implicitly higher priority
        /// than 'fire' triggers.
        /// </summary>
        /// <remarks>
        /// Note that this will sort a collection into <em>descending</em>
        /// priority order... not ascending as might be expected.
        /// This means that after sorting with this comparator, the first
        /// element in a collection is the highest priority trigger
        /// </remarks>
        private class PriorityComparator : IComparer<ITriggerInstance>
        {
            /// <summary>
            /// The single instance of this comparator
            /// </summary>
            private readonly static PriorityComparator INSTANCE = new PriorityComparator();

            /// <summary>
            /// Gets the singleton instance of the priority comparator.
            /// </summary>
            /// <returns></returns>
            public static PriorityComparator GetInstance()
            {
                return INSTANCE;
            }

            /// <summary>
            /// Compares the 2 triggers priorities and returns a value indicating
            /// if one is less than, equal to or greater than the other.
            /// </summary>
            /// <param name="x">The first trigger to compare</param>
            /// <param name="y">The second trigger to compare</param>
            /// <returns>
            /// <list>
            /// <item>Less than zero if x is higher priority than y</item>
            /// <item>Zero if x has the same priority as y</item>
            /// <item>Greater than zero if x is lower priority than y</item>
            /// </list>
            /// Note that this will sort a collection into <em>descending</em>
            /// priority order... not ascending as might be expected.
            /// </returns>
            public int Compare(ITriggerInstance x, ITriggerInstance y)
            {
                // Check for nulls first - count nulls as "infinitely low priority"
                if (x == null)
                    return (y == null ? 0 : -1);

                if (y == null) // we already know x is non-null... ergo
                    return 1;

                // If they have the same priority, 'Suppress' has higher
                // implicit priority than 'Fire'.
                int xpriority = x.Trigger.Priority;
                int ypriority = y.Trigger.Priority;

                if (xpriority == ypriority)
                {
                    if (x.Mode == y.Mode)
                        return 0;

                    return (x.Mode == TriggerMode.Suppress ? 1 : -1);
                }

                // Otherwise it's a straight priority compare
                return xpriority - ypriority;
            }
        }
        #endregion

        #region EarliestCatcher inner class

        /// <summary>
        /// Utility class to coalesce a number of triggers and find 
        /// the earliest date/time that any are active, and the collection
        /// of triggers which are active at that point in time.
        /// </summary>
        private class EarliestCatcher
        {
            /// <summary>
            /// The fire time currently set - as coalescing continues,
            /// the fire time retreats further back until it is at the
            /// earliest time of all triggers processed by this object.
            /// </summary>
            private DateTime fireTime;

            /// <summary>
            /// The collection of triggers which have their next
            /// activation time of the currently set 'fireTime'.
            /// </summary>
            private ICollection<ITriggerInstance> instances;

            /// <summary>
            /// Creates a new earliest catcher using the given collection.
            /// </summary>
            /// <param name="coll">The collection in which the triggers
            /// which are active at the earliest activation time of all
            /// processed triggers is to be stored.</param>
            public EarliestCatcher(ICollection<ITriggerInstance> coll)
            {
                fireTime = DateTime.MaxValue;
                instances = coll;
            }

            /// <summary>
            /// The earliest activation time found in all processed
            /// triggers, or DateTime.MaxValue if no activation times
            /// have been found / processed.
            /// </summary>
            public DateTime TriggerTime
            {
                get { return fireTime; }
            }

            /// <summary>
            /// Coalesce the given activation date and trigger into this
            /// catcher. If the given date is earlier than the current 
            /// 'trigger date', the currently held date and collection is
            /// cleared, and reset with these values.
            /// </summary>
            /// <param name="instance">The trigger which is activated on the given date.</param>
            public void Coalesce(ITriggerInstance instance)
            {
                if (instance == null)
                {
                    return;
                }
                var date = instance.When;
                // if instance is a 'run now' instance
                if (!instance.Trigger.IsUserTrigger)
                {
                    var scheduleTimeZoneId = instance.Trigger.Schedule?.Triggers?.PrimaryMetaData?.TimeZoneId;
                    // if its parent trigger, i.e. main schedule, has a configured time zone
                    if (scheduleTimeZoneId != null)
                    {
                        // convert the 'run now' utc time back to schedule time
                        date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById(scheduleTimeZoneId));
                    }
                    else
                    {
                        // if its parent trigger, i.e. main schedule, has no configured time zone, then convert the 'run now' utc time back to server time
                        var serverTimeZone = instance.Trigger.Schedule.Owner.Store.GetServerTimeZone();
                        date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc), serverTimeZone);
                    }
                }
                if (date == fireTime)
                {
                    instances.Add(instance);
                }
                else if (date < fireTime)
                {
                    fireTime = date;
                    instances.Clear();
                    instances.Add(instance);
                }
                // else it's later than fireTime - ignore it.
            }
        }

        #endregion

        #region TriggerGroup-specific members

        /// <summary>
        /// The collection of triggers which is brought together by this trigger.
        /// </summary>
        [DataMember]
        private IBPSet<ITrigger> _triggers = new clsSet<ITrigger>();

        /// <summary>
        /// Checks that the given trigger is absent from the ancestry of
        /// this trigger.
        /// </summary>
        /// <param name="trig">The trigger to check to ensure that is is
        /// not in the ancestry of this trigger.</param>
        /// <returns>true if the given trigger does not figure in the ancestry of
        /// this trigger; false if it is an ancestor of this trigger.</returns>
        protected bool CheckAbsentFromAncestry(ITrigger trig)
        {
            ITriggerGroup check = this;
            while (check != null)
            {
                if (trig == check)
                    return false;

                check = check.Group;
            }
            return true;
        }

        #endregion

        #region BaseTrigger overrides

        /// <summary>
        /// Compound priority - this takes the highest priority from its members.
        /// Note that you cannot set the priority of a trigger group in this way.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the priority of this
        /// group is attempted to be modified via this property.</exception>
        public override int Priority
        {
            get
            {
                int highest = int.MinValue;
                foreach (ITrigger trig in _triggers)
                {
                    int priority = trig.Priority;
                    if (priority > highest)
                        highest = priority;
                }
                return highest;
            }
            set
            {
                throw new InvalidOperationException(
                    Resources.ThePriorityOfATriggerGroupCannotBeModifiedInThisManner);
            }
        }

        /// <summary>
        /// Gets the mode of this trigger. This is more troublesome for a trigger
        /// group since its mode is determined by its members.
        /// If they are all 'Fire' mode, or all 'Success' mode then the mode can
        /// be determined and returned, otherwise 'Indeterminate' is returned, 
        /// indicating that the mode must be calculated separately for specific
        /// points in time.
        /// Note that attempting to set the mode via this property will result
        /// in an exception being thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException">If an attempt is made to 
        /// set the mode of the group using this property.</exception>
        public override TriggerMode Mode
        {
            get
            {
                // what we can check... are all our members of one mode or another
                int fires = 0;
                int suppresses = 0;
                foreach (ITrigger trig in _triggers)
                {
                    switch (trig.Mode)
                    {
                        case TriggerMode.Fire:
                            fires++;
                            break;

                        case TriggerMode.Suppress:
                            suppresses++;
                            break;

                        default: // if any members are indeterminate, then so is this.
                            return TriggerMode.Indeterminate;
                    }
                }
                if (fires == 0 && suppresses == 0) // nothing there..
                {
                    return TriggerMode.Indeterminate;
                }
                else if (suppresses == 0) // fires > 0 && suppresses == 0 - must be a 'Fire'
                {
                    return TriggerMode.Fire;
                }
                else if (fires == 0) // suppress > 0 && fires == 0 - Congratulations, it's a 'Suppress'
                {
                    return TriggerMode.Suppress;
                }
                // fires > 0 && suppresses > 0 - it goes to arbitration for a particular date/time
                return TriggerMode.Indeterminate;
            }
            set
            {
                throw new InvalidOperationException(
                    Resources.TheModeOfATriggerGroupCannotBeModifiedInThisManner);
            }
        }

        /// <summary>
        /// The start date of this trigger group - this is equivalent to the earliest
        /// start date of any of its members.
        /// If it has no members, this will contain a value of DateTime.MaxValue.
        /// Note that the start date of the trigger group cannot be set using this
        /// property - an exception will be thrown if it is attempted.
        /// </summary>
        public override DateTime Start
        {
            get
            {
                DateTime earliest = DateTime.MaxValue;
                foreach (ITrigger trig in _triggers)
                {
                    DateTime curr = trig.Start;
                    if (curr < earliest)
                        earliest = curr;
                }
                return earliest;
            }
            set
            {
                throw new InvalidOperationException(
                    Resources.TheStartDateOfATriggerGroupCannotBeModifiedInThisManner);
            }
        }

        /// <summary>
        /// The effective end date of this trigger group. This is equivalent to
        /// the latest end date of its members.
        /// If it has no members this will return DateTime.MinValue.
        /// Note that the end date of the trigger group cannot be set using this
        /// property - an exception will be thrown if it is attempted.
        /// </summary>
        public override DateTime End
        {
            get
            {
                DateTime latest = DateTime.MinValue;
                foreach (ITrigger trig in _triggers)
                {
                    DateTime curr = trig.End;
                    if (curr > latest)
                        latest = curr;
                }
                return latest;
            }
            set
            {
                throw new InvalidOperationException(
                   Resources.TheEndDateOfATriggerGroupCannotBeModifiedInThisManner);
            }
        }

        /// <summary>
        /// The schedule that the firing of this trigger activates. A trigger can
        /// only be assigned to a single schedule at once, though a schedule can be
        /// assigned many triggers.
        /// </summary>
        public override ISchedule Schedule
        {
            get { return base.Schedule; }
            set
            {
                base.Schedule = value;
                // we also need to go through our members and set the Job on them
                // to the new job.
                foreach (ITrigger trig in this)
                    trig.Schedule = value;
            }
        }

        /// <summary>
        /// Gets all the metadata objects which describe this trigger.
        /// For a group, it is all the metadata for each of the triggers that
        /// is a member of this group.
        /// </summary>
        public override ICollection<TriggerMetaData> MetaData
        {
            get
            {
                IBPSet<TriggerMetaData> set = new clsSet<TriggerMetaData>();

                foreach (ITrigger trig in this)
                    set.Union(trig.MetaData);

                return set;
            }
        }

        /// <summary>
        /// Gets the primary metadata for this trigger, if it can be distilled
        /// into a single metadata object, or null if it cannot.
        /// In the cast of a trigger group, this uses the 'user trigger' to
        /// get its primary metadata if it has one, otherwise it returns null
        /// to indicate that its trigger data cannot be refined into a single
        /// metadata object.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                // We can only return a primary meta data if we have a single 
                // configurable trigger
                ITrigger trig = UserTrigger;
                return (trig == null ? null : trig.PrimaryMetaData);
            }
        }

        /// <summary>
        /// Gets a copy of this trigger group, disassociated from any schedule or
        /// scheduler (and by implication, any store held by the scheduler).
        /// </summary>
        /// <returns>A disassociated trigger group, which contains the same set 
        /// of triggers as this group but is not reference equal to it, and does
        /// not reference any schedule / scheduler.</returns>
        public override ITrigger Copy()
        {
            TriggerGroup tg = (TriggerGroup)base.Copy();
            tg._triggers = new clsSet<ITrigger>();
            foreach (ITrigger trig in _triggers)
            {
                ITrigger copy = trig.Copy();
                copy.Group = tg;
                tg._triggers.Add(copy);
            }
            return tg;
        }

        #endregion

        #region ITrigger / ITriggerGroup implementations

        /// <summary>
        /// 
        /// Gets/Sets the single user-configurable trigger in this group.
        /// 
        /// When getting the user trigger: if either this group contains
        /// no triggers, or it contains multiple triggers which consider
        /// themselves 'user triggers', this will return null.
        /// 
        /// When setting the user trigger: passing a non-user trigger
        /// will cause an exception to be thrown. Passing anything else
        /// will cause the group to remove all currently held user
        /// triggers and replace them with this instance. If null is
        /// passed, then the group will simply remove all user triggers.
        /// 
        /// </summary>
        /// <exception cref="InvalidDataException">If, when setting the
        /// user trigger, a trigger is used which is not a user trigger
        /// (ie. its IsUserTrigger property is false)
        /// </exception>
        public ITrigger UserTrigger
        {
            get
            {
                ITrigger userTrig = null;
                foreach (ITrigger trig in _triggers)
                {
                    if (trig.IsUserTrigger)
                    {
                        if (userTrig != null) // already found one - ergo >1 user trigger
                            return null;

                        userTrig = trig;
                    }
                }
                return userTrig;
            }
            set
            {
                if (value != null && !value.IsUserTrigger)
                    throw new ArgumentException(string.Format(Resources.Trigger0IsNotAUserTrigger, value));

                ICollection<ITrigger> trigs = new clsSet<ITrigger>(_triggers);
                foreach (ITrigger t in trigs)
                {
                    if (t.IsUserTrigger)
                        Remove(t);
                }
                if (value != null)
                    Add(value);
            }
        }

        /// <summary>
        /// Adds the trigger specified by the given meta data to this group.
        /// </summary>
        /// <param name="md">The metadata defining the trigger.</param>
        /// <returns>The trigger instance added or null, if the trigger was
        /// not added to this group.</returns>
        public ITrigger Add(TriggerMetaData md)
        {
            ITrigger trig = TriggerFactory.GetInstance().CreateTrigger(md);
            if (Add(trig))
                return trig;
            return null;
        }

        /// <summary>
        /// Adds the given trigger to this group.
        /// </summary>
        /// <param name="trig">The trigger to add.</param>
        /// <exception cref="CircularReferenceException">If the given trigger is
        /// in the ancestry of this trigger, and thus would cause a reference
        /// circularity problem if added. -or- the trigger already belongs
        /// to a group other than this one.</exception>
        /// <exception cref="AlreadyAssignedException">If the given trigger is
        /// already assigned to a different group or to a different job.
        /// A trigger can only be in one group, and be assigned to a single job,
        /// and it must be removed before re-assigning.</exception>
        public bool Add(ITrigger trig)
        {
            if (!CheckAbsentFromAncestry(trig))
            {
                throw new CircularReferenceException(
                    string.Format(Resources._0IsAnAncestorOfThisTriggerATriggerCannotBeOwnedParentedByItself, trig));
            }

            ITriggerGroup group = trig.Group;
            if (group != null && group != this)
            {
                throw new AlreadyAssignedException(
                    Resources.TheProvidedTriggerIsAlreadyAssignedToADifferentGroup);
            }
            ISchedule sched = trig.Schedule;
            if (sched != null && sched != this.Schedule)
            {
                throw new AlreadyAssignedException(
                    Resources.TheProvidedTriggerIsAlreadyAssociatedWithADifferentSchedule);
            }
            trig.Group = this;
            trig.Schedule = this.Schedule;
            trig.TriggerTimingUpdated += MemberTriggerTimingUpdated;
            return _triggers.Add(trig);
        }

        /// <summary>
        /// Delegate which handles the timing of any member triggers being updated
        /// by chaining the event out to any listeners on this trigger.
        /// </summary>
        /// <param name="trigger">The trigger on which the timing has been 
        /// updated.</param>
        private void MemberTriggerTimingUpdated(ITrigger trigger)
        {
            // We can't directly fire the event from here, we need to call a base
            // class method in order to do so (?!)
            FireTimingUpdatedEvent();
        }

        /// <summary>
        /// Removes the given trigger from this group, if it is currently a member.
        /// After removal, the trigger will be set to be orphaned, ie. to not
        /// being the member of any group.
        /// </summary>
        /// <param name="trig">The trigger to remove from this group.</param>
        /// <returns>true if the provided trigger was a member of this group and it
        /// has been removed; false if it was not a member.</returns>
        public bool Remove(ITrigger trig)
        {
            if (_triggers.Contains(trig))
            {
                _triggers.Remove(trig);
                trig.Group = null;
                trig.TriggerTimingUpdated -= MemberTriggerTimingUpdated;
                return true;
            }
            return false;
        }

        public ICollection<ITriggerInstance> GetInstances(DateTime after, DateTime before)
        {
            var list = new List<ITriggerInstance>();
            var current = after;

            while (current < before)
            {
                var instance = GetNextInstance(current);

                list.Add(instance);

                if (instance != null)
                {
                    current = instance.When;
                    // if instance is a 'run now' instance
                    if (!instance.Trigger.IsUserTrigger)
                    {
                        // if its parent trigger, i.e. main schedule, has a configured time zone
                        if (PrimaryMetaData?.TimeZoneId != null)
                        {
                            // convert the 'run now' utc time back to schedule time
                            current = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(instance.When, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById(PrimaryMetaData.TimeZoneId));
                        }
                        else
                        {
                            // if its parent trigger, i.e. main schedule, has no configured time zone, then convert the 'run now' utc time back to server time
                            var serverTimeZone = instance.Trigger.Schedule.Owner.Store.GetServerTimeZone();
                            current = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(instance.When, DateTimeKind.Utc), serverTimeZone);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        /// <summary>
        /// Gets the next time for any of the triggers held by this trigger.
        /// This will find the earliest activated triggers within its collection
        /// </summary>
        /// <param name="after">The date/time after which the next instance is
        /// required.</param>
        /// <returns>The highest priority trigger instance for the next activation
        /// of this trigger after the given date/time, or null if there are no
        /// more activations of this trigger after the given time.</returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            // Get the next time from all our triggers...
            // Find the earliest ones, and then get the highest priority one from there
            var nextInstances = new clsSortedSet<ITriggerInstance>(PriorityComparator.GetInstance());
            var catcher = new EarliestCatcher(nextInstances);

            foreach (var trigger in _triggers)
            {
                var time = after;
                // if trigger is a 'run now' trigger
                if (!trigger.IsUserTrigger)
                {
                    // convert 'after' time to utc
                    time = ConvertTimeToUtc(after);
                    // DateTime.MinValue represents an invalid time that connot be converted
                    if (time == DateTime.MinValue)
                    {
                        // we ignore invalid times and move on to the next trigger

                        continue;
                    }


                }
                catcher.Coalesce(trigger.GetNextInstance(time));
            }
            // coll now has the set of trigger instances sorted into reverse priority
            // get the first one (ie. highest priority)
            return nextInstances.FirstOrDefault();
        }
        private DateTime ConvertTimeToUtc(DateTime time)
        {
            var scheduleTimeZoneId = PrimaryMetaData?.TimeZoneId;
            // if the main schedule has a time zone
            if (scheduleTimeZoneId != null)
            {
                var scheduleTimeZone = TimeZoneInfo.FindSystemTimeZoneById(scheduleTimeZoneId);
                if (scheduleTimeZone.IsInvalidTime(time))
                {
                    // covers edge case when we attempt to convert an invalid time when handling 'run now' triggers
                    // an invalid time occurs between the times a time zone's clocks spring forward
                    return DateTime.MinValue;
                }
                // convert schedule time to utc
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(time, DateTimeKind.Unspecified), scheduleTimeZone);
            }
            else
            {
                var serverTimeZone = Schedule.Owner.Store.GetServerTimeZone();
                // if main schedule has no time zone
                if (serverTimeZone.IsInvalidTime(time))
                {
                    // covers edge case when we attempt to convert an invalid time when handling 'run now' triggers
                    // an invalid time occurs between the times a time zone's clocks spring forward
                    return DateTime.MinValue;
                }
                // convert server time to utc
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(time, DateTimeKind.Unspecified), serverTimeZone);
            }
        }

        /// <summary>
        /// Evaluates whether a time conversion is required when handling 'Run Now' triggers.
        /// </summary>
        /// <param name="trigger">Trigger instance.</param>
        /// <returns>True if the trigger is a 'Run Now' trigger whose trigger time needs converting.
        public bool IsRunNowTriggerTimeConversionRequired(ITrigger trigger) => PrimaryMetaData?.TimeZoneId != null && !trigger.IsUserTrigger && PrimaryMetaData.TimeZoneId != trigger.PrimaryMetaData.TimeZoneId;

        private DateTime ConvertScheduleToRunNowTriggerTime(DateTime date, string runNowTriggerTimeZoneId) => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), PrimaryMetaData.TimeZoneId, runNowTriggerTimeZoneId);

        private DateTime ConvertRunNowTriggerToScheduleTime(DateTime date, string runNowTriggerTimeZoneId) => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, runNowTriggerTimeZoneId, PrimaryMetaData.TimeZoneId);

        /// <summary>
        /// Removes all triggers from this group. This will set the Group
        /// property of all triggers on this group to 'null' ie. it will make
        /// each previously held trigger an orphan.
        /// Optionally, it will also unassign each trigger from its held job.
        /// </summary>
        /// <param name="unassignJobsOnTriggers">true to set the Job property
        /// on each of the held triggers to null, indicating that they are 
        /// currently not assigned to a job; false to leave the Job as it is
        /// at the point of the method being called.</param>
        private void Clear(bool unassignJobsOnTriggers)
        {
            // Unset the group in each of the contained triggers
            foreach (ITrigger trig in _triggers)
            {
                if (unassignJobsOnTriggers)
                    trig.Schedule = null;

                trig.TriggerTimingUpdated -= MemberTriggerTimingUpdated;
                trig.Group = null;
            }

            _triggers.Clear();
        }

        /// <summary>
        /// Clears the members from this group, ensuring that each trigger
        /// in the group has its 'Group' and 'Job' cleared as part of the
        /// process.
        /// </summary>
        public void Clear()
        {
            // Can't think of when you'd not want to do this really, so
            // the Clear(bool) method is left private for now.
            Clear(true);
        }

        /// <summary>
        /// The member triggers of this trigger
        /// </summary>
        public ICollection<ITrigger> Members
        {
            // ensures that any collection operations deal with the ITrigger.Group property
            get { return this; }
        }

        public bool HasExpired()
        {
            var nextInstances = new clsSortedSet<ITriggerInstance>(PriorityComparator.GetInstance());
            var catcher = new EarliestCatcher(nextInstances);
            var date = DateTime.Now;

            if (PrimaryMetaData?.TimeZone != null)
            {
                date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, TimeZoneInfo.Local.Id, PrimaryMetaData.TimeZoneId);
            }

            foreach (var trigger in _triggers)
            {
                if (trigger.IsUserTrigger)
                {
                    catcher.Coalesce(trigger.GetNextInstance(date));
                }
            }

            var hasExpired = nextInstances.FirstOrDefault() == null;
            return hasExpired;
        }

        #endregion

        #region ICollection implementations

        /// <summary>
        /// Adds the given trigger to this collection.
        /// </summary>
        /// <param name="item">The trigger to be added to this collection.
        /// </param>
        void ICollection<ITrigger>.Add(ITrigger item)
        {
            Add(item);
        }

        /// <summary>
        /// Checks if this collection contains the given trigger.
        /// </summary>
        /// <param name="item">The trigger to be checked.</param>
        /// <returns>true if this group has the given trigger as a member;
        /// false otherwise.</returns>
        public bool Contains(ITrigger item)
        {
            return _triggers.Contains(item);
        }

        /// <summary>
        /// Copies the triggers in this group to the given array at the
        /// index specified.
        /// </summary>
        /// <param name="array">The array into which this collection's
        /// triggers should be copied.</param>
        /// <param name="arrayIndex">The index at which the triggers
        /// should be copied into the array.</param>
        /// <exception cref="ArgumentNullException">If the given array is
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If :- <list>
        /// <item>arrayIndex is negative -or-</item>
        /// <item>arrayIndex is equal to or greater than the length of 
        ///     array -or-</item>
        /// <item>The number of triggers in this group is greater than the
        ///     available space from arrayIndex to the end of the destination
        ///     array</item>
        /// </list></exception>
        public void CopyTo(ITrigger[] array, int arrayIndex)
        {
            // Fail if no array to copy to
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            // Fail if not enough space in the array for the triggers.
            if (arrayIndex < 0 || (arrayIndex + _triggers.Count > array.Length))
            {
                throw new ArgumentOutOfRangeException(String.Format(
                    Resources.ArrayIndex0ArrayLength1MembersCount2,
                    arrayIndex, array.Length, _triggers.Count));
            }

            // Otherwise, load 'em up
            foreach (ITrigger trig in _triggers)
                array[arrayIndex++] = trig;

        }

        /// <summary>
        /// Gets a count of the triggers in this group.
        /// </summary>
        public int Count
        {
            get { return _triggers.Count; }
        }

        /// <summary>
        /// Checks if this collection is read-only. It is not.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable implementations

        public IEnumerator<ITrigger> GetEnumerator()
        {
            return _triggers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
