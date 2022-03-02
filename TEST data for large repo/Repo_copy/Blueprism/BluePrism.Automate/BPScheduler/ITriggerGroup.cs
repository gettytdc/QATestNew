using System;
using System.Collections.Generic;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Interface describing a group of triggers.
    /// This is a trigger itself, but it delegates a lot of its decision
    /// making to its members.
    /// This provides the context required to determine which trigger
    /// is effective at a particular point in time.
    /// </summary>
    public interface ITriggerGroup : ITrigger
    {
        /// <summary>
        /// The member triggers of this group
        /// </summary>
        ICollection<ITrigger> Members { get; }

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
        ITrigger UserTrigger { get; set; }

        /// <summary>
        /// Clears all of the triggers from this group (not recursively).
        /// Any triggers which were previously members of this group will
        /// be orphaned after this method has returned, ie. their Group
        /// property will return null.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds the given trigger to this group, ensuring that its <see 
        /// cref="ITrigger.Group"/> property indicated this group. Also,
        /// that its <see cref="ITrigger.Job"/> property pointed to the job
        /// referenced by this group.
        /// </summary>
        /// <param name="trig">The trigger to add.</param>
        /// <returns>true if the trigger was added to this group; false if
        /// the trigger was not added - usually due to the trigger already
        /// existing within the group, but perhaps this implementation has
        /// size restrictions or such like.
        /// See the documentation for the implementation for more specific
        /// details.</returns>
        /// <exception cref="CircularReferenceException">If the given 
        /// trigger is in the ancestry of this trigger, and thus would 
        /// cause a reference circularity problem if added</exception>
        /// <exception cref="AlreadyAssignedException">if the given trigger
        /// was already a member of a group other than this one.</exception>
        /// <remarks>After this method has returned, the trigger's Group
        /// property will reference this trigger group.</remarks>
        bool Add(ITrigger trig);

        /// <summary>
        /// Adds the trigger specified by the given meta data to this group.
        /// </summary>
        /// <param name="md">The metadata defining the trigger.</param>
        /// <returns>The trigger instance added or null, if the trigger was
        /// not added to this group.</returns>
        ITrigger Add(TriggerMetaData md);

        /// <summary>
        /// Removes the given trigger from this group.
        /// </summary>
        /// <param name="trig">The trigger to remove.</param>
        /// <returns>true if the given trigger was a member of this group
        /// and has thus been removed; false if it was not actually a
        /// member, and the group has therefore not been changed.</returns>
        /// <remarks>After this method has returned, the trigger's Group
        /// property will be null.</remarks>
        bool Remove(ITrigger trig);

        /// <summary>
        /// Gets all the points in time that this trigger is activated between the 2 limits (exclusive).
        /// </summary>
        /// <param name="after">The exclusive lower limit to use as part of the date range.</param>
        /// <param name="before">The exclusive upper limit to use as part of the date range.</param>
        ICollection<ITriggerInstance> GetInstances(DateTime after, DateTime before);

        /// <summary>
        /// Indicates whether a trigger group has expired.
        /// </summary>
        bool HasExpired();
    }
}
