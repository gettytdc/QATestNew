namespace BluePrism.Scheduling
{
    /// <summary>
    /// The action to perform in the case of a trigger misfire.
    /// </summary>
    public enum TriggerMisfireAction
    {
        /// <summary>
        /// 'Zero' value indicating that the action is not set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tells the scheduler that the misfired trigger instance should be
        /// fired as if its more was TriggerMode.Fire.
        /// This may typically be returned after a misfire of types
        /// <see cref="TriggerMisfireReason.ModeWasIndeterminate"/> or
        /// <see cref="TriggerMisfireReason.EarlierScheduleInstanceStillRunning"/>
        /// </summary>
        FireInstance,

        /// <summary>
        /// Tells the scheduler that the misfired trigger instance should be suppressed
        /// as if its mode was TriggerMode.Suppress.
        /// Note that this will cause a second misfire to occur with the reason being
        /// that the 'ModeWasSuppress'.
        /// </summary>
        SuppressInstance,

        /// <summary>
        /// Aborts the trigger instance, performing no work on it. This is valid
        /// for all misfire types. No second misfire will be fired if this action
        /// is returned in response to a misfire (unlike <see cref="SuppressInstance"/>)
        /// </summary>
        AbortInstance
    }

}
