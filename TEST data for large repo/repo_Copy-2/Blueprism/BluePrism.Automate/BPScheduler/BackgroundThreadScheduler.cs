using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Scheduling.Events;

// If we import all of System.Threading, the Timer class reference would get confused.
using Thread = System.Threading.Thread;
using System.Linq;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// The basic scheduler which maintains a scheduler thread in the background.
    /// </summary>
    public abstract class BackgroundThreadScheduler : IScheduler, IDiary
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// Class representing a mapping of schedule logs onto the trigger instance
        /// which begat them - adds nothing, it just makes references to the construct
        /// easier to read
        /// </summary>
        private class InstanceLogMap : Dictionary<ITriggerInstance, IScheduleLog> { }

        /// <summary>
        /// The interval between heartbeat pulses - sent to each of the running
        /// logs to indicate that the scheduler still considers them as running
        /// schedules.
        /// </summary>
        private const int HeartbeatInterval = 30 * 1000;

        #endregion

        #region - Members -

        /// <summary>
        /// Event fired when the status of the scheduler has been updated and
        /// it wishes to inform any interested parties.
        /// </summary>
        public event LogStatus StatusUpdated;

        public event LogStatus AddInfoLog;

        /// <summary>
        /// Lock object to ensure that 2 threads running within this scheduler
        /// and logging at the same time don't clash and cause log munging.
        /// </summary>
        private readonly object LOG_LOCK = new object();

        /// <summary>
        /// The name of this scheduler
        /// </summary>
        private string _name;

        /// <summary>
        /// The schedule store where this schedule saves and loads its
        /// information.
        /// </summary>
        private IScheduleStore _store;

        /// <summary>
        /// The thread which is sitting in the background awaiting the
        /// time when schedules need to be activated.
        /// </summary>
        private SchedulerThread _thread;

        /// <summary>
        /// The heartbeat timer which indicates to the schedule logs that
        /// the scheduler is still running them
        /// </summary>
        private Timer _heartbeatTimer;

        /// <summary>
        /// The set of currently running schedules.
        /// </summary>
        private IDictionary<ISchedule, InstanceLogMap> _running;

        /// <summary>
        /// The schedules that have been executed while this scheduler is
        /// starting up or resuming. This ensures that a schedule is only
        /// executed a maximum of once.
        /// </summary>
        private IBPSet<ISchedule> _executed;

        /// <summary>
        /// The server time zone.
        /// Needed to calculate the next schedule instance activation according to server time.
        /// </summary>
        private readonly TimeZoneInfo _serverTimeZone;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new background thread scheduler using the current machine
        /// name as its name and retrieving schedules / logs from the provided
        /// store.
        /// </summary>
        /// <param name="store">The store from which the schedules / calendars
        /// and logs are drawn</param>
        public BackgroundThreadScheduler(IScheduleStore store)
            : this(Environment.MachineName, store) { }

        /// <summary>
        /// Creates a new empty scheduler which uses the given store to retrieve
        /// its schedules and logs.
        /// </summary>
        /// <param name="store">The store from which the schedules / calendars
        /// and logs are drawn</param>
        public BackgroundThreadScheduler(string name, IScheduleStore store)
        {
            this.Store = store;
            _name = name;
            _running = new Dictionary<ISchedule, InstanceLogMap>();
            _serverTimeZone = Store.GetServerTimeZone();

            // Set up the heartbeat timer - {AutoReset=false} in order to ensure that
            // it is non re-entrant
            _heartbeatTimer = new Timer(HeartbeatInterval);
            _heartbeatTimer.AutoReset = false;
            _heartbeatTimer.Elapsed += HandleHeartbeat;
            _heartbeatTimer.Start();
        }

        #endregion

        #region - Executing and schedule monitoring -

        /// <summary>
        /// Handles the schedules on the store being updated by passing the event
        /// onto anything listening for diary updates.
        /// </summary>
        private void HandleUpdatedSchedules()
        {
            if (DiaryUpdated != null)
                DiaryUpdated(this);
        }

        /// <summary>
        /// Checks if the given schedule has executed already at some point
        /// during this resume action, and if it hasn't, marks it as having
        /// done so now.
        /// </summary>
        /// <param name="sched">The schedule to check to see if it has
        /// already run while the scheduler is in Startup / Resume mode.
        /// </param>
        /// <returns>true if the schedule has already run in this startup
        /// or resume mode; false otherwise. In either case, the schedule
        /// will have been marked as having executed during this resume.
        /// </returns>
        private bool MarkExecutedDuringResume(ISchedule sched)
        {
            if (_executed == null)
                _executed = new clsSet<ISchedule>();
            // if it *didn't* add, then it's executed before...
            return !_executed.Add(sched);
        }

        /// <summary>
        /// Clears the 'executed schedules' collection, setting the state
        /// such that no schedules have run during startup / resume mode
        /// from this point on.
        /// </summary>
        private void ClearExecuted()
        {
            _executed = null;
        }

        /// <summary>
        /// <para>
        /// Handler method for 'Triggered' events - fired by the background thread
        /// monitored by this scheduler.
        /// </para><para>
        /// This will determine whether the triggered event constitutes a 'misfire'
        /// or not (ie. an instance which results in the schedule *not* executing)
        /// and informs the schedule in question if it does.
        /// </para><para>
        /// It then creates a separate thread on which the schedule is executed,
        /// returning immediately to allow the event firing code to continue.
        /// </para>
        /// </summary>
        /// <param name="args">The event arguments that detail the event
        /// which has been fired.</param>
        protected virtual void OnTriggered(TriggeredEventArgs args)
        {
            // A selection of shorthand local vars and state
            ITriggerInstance inst = args.Instance;
            ISchedule sched = inst.Trigger.Schedule;
            TriggerMisfireAction action = TriggerMisfireAction.None;
            TriggerMode trigMode = inst.Mode;
            ExecutionMode execMode = args.Mode;

            // Due to the way that this can be reached, this can get a little spaghetti-like.
            // Basically we want to execute the schedule if :-
            // 1) Execution mode is 'Normal' and Instance mode is 'Fire'
            // 2) Execution mode is 'Normal', Instance mode is 'Indeterminate' and the
            //    Misfire action is 'FireIndeterminateInstance'
            // 3) Execution mode is 'Startup' or 'Resume' and the schedule referred to
            //    by the instance has not yet been executed within this 'startup'/'resume' cycle.

            // Deal with the basic state first
            bool executeSchedule =
                (args.Mode == ExecutionMode.Normal && inst.Mode == TriggerMode.Fire);

            // If it wasn't that easy, we need to 
            // a) figure out if we're executing the schedule or not, and
            // b) ensure that the schedule is informed of the misfire if we're not.
            if (!executeSchedule)
            {
                // If the trigger mode is indeterminate, we need to figure out how it
                // should be treated first.
                if (args.Mode == ExecutionMode.Normal && trigMode == TriggerMode.Indeterminate)
                {
                    action = sched.Misfire(inst, TriggerMisfireReason.ModeWasIndeterminate);
                    switch (action)
                    {
                        // in case of an abort, just return - no further work required
                        case TriggerMisfireAction.AbortInstance:
                            LogStatus("Trigger aborted : {0}", args.Instance);
                            return;

                        // If we're firing, set the mode and execute flag accordingly
                        case TriggerMisfireAction.FireInstance:
                            trigMode = TriggerMode.Fire;
                            executeSchedule = true;
                            break;

                        // Suppressing? execute flag stays false, and the mode is re-set.
                        case TriggerMisfireAction.SuppressInstance:
                            trigMode = TriggerMode.Suppress;
                            break;

                        // Bad karma, man.
                        default:
                            throw new InvalidOperationException(
                                string.Format(Resources.BadMisfireActionForIndeterminateModes0, action));
                    }
                }
            }

            // The second point where we could be executing (the base case or
            // if the indeterminate mode action was FireIndeterminateInstance)
            if (!executeSchedule)
            {

                if (trigMode == TriggerMode.Stop)
                {
                    var runningSchedule = sched;
                    if (_running.ContainsKey(sched))
                    {
                        // Get the schedule from memory as it contains if the schedule is running (used in Abort)
                        // sched is fetched from the Database each time.
                        runningSchedule = _running.Keys.FirstOrDefault(x => x.Name == sched.Name);
                    }
                    runningSchedule.Abort(Resources.UserAbortedSchedule);
                    return;
                }

                // Now 'trigMode' determines what we do with the instance
                // and 'execMode' tells us what mode the thread is currently in.                
                if (trigMode == TriggerMode.Suppress)
                {
                    // First off deal with suppression triggers cos they're easy
                    sched.Misfire(inst, TriggerMisfireReason.ModeWasSuppress); // ignore output
                    LogStatus("Trigger Suppressed: {0}", inst);
                    // no need to do any more work...
                    return;
                }

                // Now check the state of play for 'Startup' or 'Resuming' execution
                if (execMode == ExecutionMode.Startup || execMode == ExecutionMode.Resume)
                {
                    // First off, check if it's already run in this execution mode period.
                    if (MarkExecutedDuringResume(sched))
                    {
                        // It's already run... inform the schedule.
                        sched.Misfire(inst, execMode == ExecutionMode.Startup
                            ? TriggerMisfireReason.ScheduleAlreadyExecutedDuringStartup
                            : TriggerMisfireReason.ScheduleAlreadyExecutedDuringResume
                        );
                        // Okay, we're not running it, so let's geddahelloutahere
                        LogStatus("Trigger suppressed - already executed during {0}: {1}",
                         execMode, inst);
                        return;
                    }
                    else // it's not been executed yet, do so now.
                    {
                        executeSchedule = true;
                    }
                }
            }

            // Any 'end of work' points should have returned from the method by now...
            // if we're still not executing the schedule, it's a programming error
            if (!executeSchedule)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.NoReasonFoundForNotExecutingSchedule0ButExecuteFlagIsUnset,
                    sched));
            }

            // So we're now sure we want to execute the schedule, given the trigger's
            // properties and schedule responses - we now need to make sure that
            // a) an earlier instance isn't still running, and
            // b) that no other scheduler is currently running this schedule instance

            IScheduleLog runningLog = null; // var in which we will store the already running log

            // First check - is it running in this scheduler?
            InstanceLogMap map;
            if (_running.TryGetValue(sched, out map))
                runningLog = CollectionUtil.First(map.Values);

            // If not, is it running in another scheduler?
            if (runningLog == null)
            {
                // If the log has been updated within the last 2 heartbeat intervals,
                // assume that it is still alive. (Ensure UTC - IScheduleLog.LastUpdated
                // returns a UTC time)
                DateTime checkTime = DateTime.UtcNow.AddMilliseconds(-2 * HeartbeatInterval);

                foreach (IScheduleLog log in sched.GetRunningInstances())
                {
                    LogStatus("Checking log for {0}; Last Updated: {1}; Check Time: {2}",
                        log.InstanceTime, log.LastUpdated, checkTime);

                    if (log.LastUpdated > checkTime)
                    {
                        runningLog = log;
                        break;
                    }
                }
            }

            // If found running in either place, ask the schedule what to do
            if (runningLog != null)
            {
                LogStatus("'{0}' is still being executed from the instance at {1}",
                    sched.Name, runningLog.InstanceTime);

                switch (sched.Misfire(
                    inst, TriggerMisfireReason.EarlierScheduleInstanceStillRunning))
                {
                    // in case of an abort, just return - no further work required
                    case TriggerMisfireAction.AbortInstance:
                        LogStatus("Trigger aborted : {0}", args.Instance);
                        return;

                    case TriggerMisfireAction.SuppressInstance:
                        // second misfire to record the not running of the instance
                        sched.Misfire(inst, TriggerMisfireReason.ModeWasSuppress);
                        LogStatus("Trigger Suppressed: {0}", inst);
                        // no need to do any more work...
                        return;

                    // If we're firing, just carry on - everything's set correctly already
                    case TriggerMisfireAction.FireInstance:
                        UpdateLog(StatusUpdated, "Trigger firing");
                        break;

                    // More bad karma
                    default:
                        throw new InvalidOperationException(
                            string.Format(Resources.BadMisfireActionForAlreadyRunningSchedule0, action));
                }
            }

            // The only way to see if another scheduler is running this instance
            // is to attempt to create a log
            IScheduleLog currLog = null;
            try
            {
                currLog = Store.CreateLog(inst);
            }
            catch (AlreadyActivatedException)
            {
                // Final chance of a misfire
                sched.Misfire(inst, TriggerMisfireReason.ScheduleInstanceAlreadyExecuted);

                const string alreadyActivatedLogMessage = "already activated - skipping";
                LogStatus("'{0}' {1}", inst, alreadyActivatedLogMessage);
                UpdateLog(AddInfoLog, $"{inst} {alreadyActivatedLogMessage}");

                return;
            }
            catch (Exception e)
            {
                LogStatus("Exception occurred creating schedule log : {0}", e);
                return;
            }

            // Run the schedule in a separate thread to allow the scheduler
            // thread to continue with any other schedules which should
            // execute at the same time.
            Thread t = new Thread(delegate ()
            {
                try
                {
                    RegisterRunningSchedule(inst, currLog);
                    inst.Trigger.Schedule.Execute(currLog);
                }
                catch (Exception e)
                {
                    UpdateLog(StatusUpdated, "Exception occurred executing schedule : ");
                    UpdateLog(StatusUpdated, $"{e}");
                    try
                    {
                        currLog.Terminate(
                            "Unhandled Exception occurred while executing schedule", e);
                    }
                    catch (ScheduleFinishedException) { } // log has already been terminated...
                    catch (Exception ex)
                    {
                        LogStatus("Exception occurred terminating schedule : {0}", ex);
                    }
                }
                finally
                {
                    UnregisterRunningSchedule(inst);
                }
            });
            t.Start();
            // Thread.Yield() for the .NET 2.0 generation
            Thread.Sleep(1);
        }

        /// <summary>
        /// Registers the given schedule as a running schedule within this
        /// scheduler. If the schedule is already registered as running by
        /// another thread, this throws an exception
        /// </summary>
        /// <param name="inst">The instance to register as a running instance
        /// within this scheduler.</param>
        /// <param name="log">The schedule log on which the running schedule is
        /// reporting progress.</param>
        private void RegisterRunningSchedule(ITriggerInstance inst, IScheduleLog log)
        {
            ISchedule sched = inst.Trigger.Schedule;
            lock (_running)
            {
                InstanceLogMap map;
                if (!_running.TryGetValue(sched, out map))
                {
                    map = new InstanceLogMap();
                    _running[sched] = map;
                }
                map[inst] = log;
            }
        }

        /// <summary>
        /// Unregisters the given schedule from the running schedules
        /// collection maintained in this scheduler.
        /// </summary>
        /// <param name="inst">The instance to de-register.</param>
        private void UnregisterRunningSchedule(ITriggerInstance inst)
        {
            lock (_running)
            {
                ISchedule sched = inst.Trigger.Schedule;

                // Remove the given instance from the schedules instance/log map
                InstanceLogMap map = _running[sched];
                map.Remove(inst);

                // If that makes the map empty, remove the schedule from the running collection
                if (map.Count == 0)
                    _running.Remove(sched);
            }
        }

        /// <summary>
        /// Handles the heartbeat timer elapsing - this ensures that the logs
        /// are told that the scheduler still considers them to be part of a
        /// running schedule, allowing them to broadcast this information to
        /// any interested parties as they see fit.
        /// </summary>
        private void HandleHeartbeat(object sender, ElapsedEventArgs e)
        {
            lock (_running)
            {
                foreach (InstanceLogMap map in _running.Values)
                {
                    foreach (IScheduleLog log in map.Values)
                    {
                        try
                        {
                            //LogStatus("Pulsing: {0}", log.Schedule);
                            log.Pulse();
                        }
                        // if an unregister is waiting to be performed, a
                        // ScheduleFinishedException might be thrown - ignore it
                        catch (ScheduleFinishedException) { }
                    }
                }
            }
            _heartbeatTimer.Start();
        }

        #endregion

        #region - Schedule Thread Handling -

        /// <summary>
        /// Gets the current scheduler thread, creating and assigning one first
        /// if it doesn't already exist.
        /// </summary>
        /// <returns>The scheduler thread that this scheduler is registered on.
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private SchedulerThread GetThread()
        {
            if (_thread == null)
            {
                _thread = new SchedulerThread(this);
                _thread.StatusUpdated += delegate (string msg)
                { UpdateLog(StatusUpdated, msg); };
            }
            return _thread;
        }

        /// <summary>
        /// Starts the scheduler thread, and listens for trigger events
        /// to be fired, checking the registered number of milliseconds
        /// into the past for any missed schedules.
        /// </summary>
        public virtual void Start(int millisToReview)
        {
            UpdateLog(StatusUpdated, "Starting background thread scheduler : " + (millisToReview == 0
                ? "not checking for missed schedules"
                : "checking " + millisToReview + "ms back for missed schedules"));
            SchedulerThread t = GetThread();
            t.Triggered += OnTriggered;
            t.Start(millisToReview);
        }

        /// <summary>
        /// Checks if this scheduler is currently running and listening
        /// for events which indicate that schedules are to be executed.
        /// </summary>
        /// <returns>true if the background thread is currently running
        /// and this scheduler is dealing with trigger events</returns>
        public virtual bool IsRunning()
        {
            return GetThread().IsRunning();
        }

        /// <summary>
        /// Suspends this scheduler causing it not to fire any schedules
        /// that occur until the scheduler is resumed.
        /// </summary>
        public virtual void Suspend()
        {
            UpdateLog(StatusUpdated, "Suspending background thread scheduler");
            GetThread().Pause();
            UpdateLog(StatusUpdated, "Background thread scheduler suspended");
        }

        /// <summary>
        /// Checks if this scheduler is currently suspended 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuspended()
        {
            return GetThread().IsPaused();
        }

        /// <summary>
        /// Resumes this scheduler, processing any missed instances
        /// which occurred while it was suspended as directed.
        /// </summary>
        /// <param name="fireMissedInstances"></param>
        public virtual void Resume(bool fireMissedInstances)
        {
            UpdateLog(StatusUpdated, "Resuming background thread scheduler : " +
                (fireMissedInstances ? "Firing missed schedules" : "Skipping missed schedules"));
            GetThread().Resume(fireMissedInstances);
            UpdateLog(StatusUpdated, "Background thread scheduler resumed");
        }

        /// <summary>
        /// Resumes this scheduler, ignoring any instances which would
        /// have been processed if this scheduler hadn't been paused.
        /// </summary>
        public virtual void Resume()
        {
            Resume(false);
        }

        /// <summary>
        /// Stops this scheduler.
        /// </summary>
        public virtual void Stop()
        {
            UpdateLog(StatusUpdated, "Stopping background thread scheduler");
            SchedulerThread t = GetThread();
            t.Triggered -= OnTriggered;
            t.Stop();
            t.Join();

            UpdateLog(StatusUpdated, "Background thread stopped - checking for running schedules");
            if (_running.Count == 0)
                return;

            clsSet<ISchedule> set = new clsSet<ISchedule>(_running.Keys);
            foreach (ISchedule sched in set)
            {
                try
                {
                    sched.Abort("Scheduler is being stopped");
                }
                catch (Exception e)
                {
                    LogStatus("Schedule '{0}' abort caused exception : {1}. Skipping", sched, e);

                    // There's a lot of scary stuff about not calling Thread.Abort() out there.
                    // I think we'd need some more testing before we went with it... currently
                    // it will fail with a null pointer exception when accessing a null gSv
                    // variable - as long as we code to deal with that, we shouldn't need
                    // a Thread.Abort()
                    // LogStatus("Schedule '{0}' abort caused exception : {1}. Aborting thread", sched, e);                 
                    // running[sched].Abort();
                }
            }
            // Leave the thread there, so it can be restarted and can
            // pick up where it left off if it is so.
        }

        /// <summary>
        /// Logs the given status message to any StatusUpdate event listeners.
        /// </summary>
        /// <param name="message">The status message to log.</param>
        /// <param name="args">The arguments for the formatted message.
        /// </param>
        private void LogStatus(string message, params object[] args)
            => UpdateLog(StatusUpdated, string.Format(message, args));

        /// <summary>
        /// Logs the given status message to any StatusUpdate event listeners using the method provided.
        /// </summary>
        /// <param name="loggingType">The method that is going to be used to log status changes.</param>
        /// <param name="logMessage">The text to be logged
        /// </param>
        private void UpdateLog(LogStatus loggingType, string logMessage)
        {
            try
            {
                lock (LOG_LOCK)
                {
                    loggingType?.Invoke(logMessage);
                }
            }
            catch { } // ignore any exceptions in event handlers
        }

        #endregion

        #region - IScheduler Implementations -

        /// <summary>
        /// We've only one scheduler at the moment, though there may be multiple
        /// ones in the future.
        /// </summary>
        public String Name
        {
            get { return (_name ?? ""); }
        }

        /// <summary>
        /// The diary on which this scheduler's jobs will run.
        /// </summary>
        public IDiary Diary
        {
            get { return this; }
        }

        /// <summary>
        /// The currently running schedules.
        /// </summary>
        public ICollection<ISchedule> RunningSchedules
        {
            get { return _running.Keys; }
        }

        /// <summary>
        /// The store from which this scheduler picks up its data.
        /// </summary>
        public IScheduleStore Store
        {
            get { return _store; }
            set
            {
                if (_store != null && !Object.ReferenceEquals(_store, value))
                {
                    _store.Owner = null;
                    _store.SchedulesUpdated -= HandleUpdatedSchedules;
                }
                _store = value;
                if (value != null)
                {
                    // Check that we're not already assigned to this store to
                    // inhibit infinite loops
                    if (!object.ReferenceEquals(this, value.Owner))
                        value.Owner = this;

                    value.SchedulesUpdated += HandleUpdatedSchedules;
                }
            }
        }

        #endregion

        #region - IJobDiary Implementations -

        /// <summary>
        /// Event fired whenever the diary is updated.
        /// </summary>
        public event HandleDiaryUpdated DiaryUpdated;

        /// <summary>
        /// Gets the next activation time for any schedules held in this
        /// scheduler after the given date/time.
        /// </summary>
        /// <param name="after">The date/time after which the next activation
        /// time is required.</param>
        /// <returns>The next point in time at which an activation occurs on
        /// any schedule within this scheduler's store.</returns>
        public DateTime GetNextActivationTime(DateTime after)
        {
            var earliest = DateTime.MaxValue;
            foreach (var schedule in Store.GetActiveSchedules())
            {
                var convertedTime = ConvertServerToScheduleTime(after, schedule);
                var instance = schedule.Triggers.GetNextInstance(convertedTime);
                if (instance != null)
                {
                    var serverInstanceTime = ConvertInstanceToServerTime(instance.When, instance);
                    if (serverInstanceTime < earliest)
                    {
                        earliest = serverInstanceTime;
                    }
                }
            }
            return earliest;
        }

        private DateTime ConvertServerToScheduleTime(DateTime time, ISchedule schedule)
        {
            var data = schedule?.Triggers?.PrimaryMetaData;
            return ConvertTime(time, data, false);
        }

        private DateTime ConvertInstanceToServerTime(DateTime time, ITriggerInstance instance)
        {
            var data = instance?.Trigger?.PrimaryMetaData;
            return ConvertTime(time, data, true);
        }

        private DateTime ConvertTime(DateTime time, TriggerMetaData data, bool instanceToServer)
        {
            var isRunNowInstance = !data?.IsUserTrigger;
            // if instance is a 'run now' instance
            if ((isRunNowInstance ?? false) && instanceToServer)
            {
                // convert the 'run now' utc time back to server time
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(time, DateTimeKind.Utc), _serverTimeZone);
            }

            var offset = data?.UtcOffset;
            // if schedule does not adjust for daylight savings, convert using the base offsets (no daylight savings adjustment)
            if (offset != null)
            {
                return ConvertTimeWithoutDaylightSavings(time, offset, instanceToServer);
            }

            // if schedule adjusts for daylight savings
            if (!string.IsNullOrWhiteSpace(data?.TimeZoneId))
            {
                return ConvertTimeWithDaylightSavings(time, data, instanceToServer);
            }

            return time;
        }

        private DateTime ConvertTimeWithoutDaylightSavings(DateTime time, TimeSpan? offset, bool instanceToServer)
        {
            if (instanceToServer)
            {
                return time - (offset.Value - _serverTimeZone.BaseUtcOffset);
            }
            else
            {
                return time - (_serverTimeZone.BaseUtcOffset - offset.Value);
            }
        }

        private DateTime ConvertTimeWithDaylightSavings(DateTime time, TriggerMetaData data, bool instanceToServer)
        {
            if (instanceToServer)
            {
                // if the instance time is invalid, i.e. a time that falls between when the clocks move forward
                if (TimeZoneInfo.FindSystemTimeZoneById(data.TimeZoneId).IsInvalidTime(time))
                {
                    // convert the time using the base utc offsets (no daylight savings adjustment), and the scheduler will handle it accordingly
                    return time - (TimeZoneInfo.FindSystemTimeZoneById(data.TimeZoneId).BaseUtcOffset - _serverTimeZone.BaseUtcOffset);
                }
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, data.TimeZoneId, _serverTimeZone.Id);
            }
            else
            {
                // if the server time is invalid, i.e. a time that falls between when the clocks move forward
                if (_serverTimeZone.IsInvalidTime(time))
                {
                    // then convert the time using the base utc offsets (no daylight savings adjustment), and the scheduler will handle it accordingly
                    return time - (_serverTimeZone.BaseUtcOffset - TimeZoneInfo.FindSystemTimeZoneById(data.TimeZoneId).BaseUtcOffset);
                }
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, _serverTimeZone.Id, data.TimeZoneId);
            }
        }

        /// <summary>
        /// Gets all trigger instances which occur for the given date/time.
        /// </summary>
        /// <param name="date">The date/time for which instances are required.
        /// </param>
        /// <returns>The trigger instances which occur for the given date/time.
        /// </returns>
        public ICollection<ITriggerInstance> GetInstancesFor(DateTime date)
        {
            IBPSet<ITriggerInstance> instanceSet = new clsSet<ITriggerInstance>();

            var start = date.AddSeconds(-1);
            var end = date.AddSeconds(1);

            foreach (var schedule in Store.GetActiveSchedules())
                instanceSet.Union(schedule.Triggers.GetInstances(
                    ConvertServerToScheduleTime(start, schedule),
                    ConvertServerToScheduleTime(end, schedule)));

            return instanceSet;
        }

        #endregion
    }
}
