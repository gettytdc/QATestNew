using System;
using System.Collections.Generic;

namespace BluePrism.Scheduling
{

    /// <summary>
    /// Delegate which is used to handle job store changes.
    /// It would be much nicer if you could define these inside interfaces
    /// since this has very little meaning outside IJobDiary.
    /// </summary>
    /// <param name="sender">The diary which has been updated.</param>
    public delegate void HandleDiaryUpdated(IDiary sender);

    /// <summary>
    /// Interface describing a job store - this is used by the scheduler to
    /// discover which jobs are due to run when.
    /// </summary>
    public interface IDiary
    {
        /// <summary>
        /// Event thrown when the diary has changed. Any implementations
        /// of IJobDiary must ensure that this event is fired whenever a
        /// change is made to the diary which affects the time which the
        /// triggers are fired, <i>eg.</i> if a trigger is added or
        /// removed from a schedule; if a trigger's start date or end 
        /// date is changed; if a new schedule is registered with the
        /// diary. Any of these should fire the JobDiaryUpdated event.
        /// </summary>
        event HandleDiaryUpdated DiaryUpdated;

        /// <summary>
        /// Gets the next activation time for any triggers held in this
        /// job store <em>which have not already been activated</em>.
        /// That last is important, since the 'after' might be historic.
        /// If a trigger has already been activated for a particular
        /// point in time, then the job which that trigger is assigned to
        /// should be discounted from the running for that point in time.
        /// </summary>
        /// <param name="after">The point in time that the next activation
        /// trigger should be after.</param>
        /// <returns>The next date time at which a trigger held by this 
        /// store is activated.</returns>
        DateTime GetNextActivationTime(DateTime after);

        /// <summary>
        /// Gets all trigger instances which are activated at a particular
        /// point in time.
        /// Note that these instances should not represent the same job -
        /// it is not implicitly a 'trigger group'. It should be all 
        /// disparate instances which are activated at the specified time.
        /// </summary>
        /// <param name="date">The point in time to search for activated
        /// triggers.</param>
        /// <returns>The non-null collection of triggers which are
        /// activated at the given point in time.</returns>
        ICollection<ITriggerInstance> GetInstancesFor(DateTime date);
    }
}
