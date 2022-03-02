using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

using BluePrism.Scheduling.Events;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// The background thread which awaits trigger events and fires them as appropriate.
    /// After creation it must be started with a call to its Start() method.
    /// It can be paused and resumed via its Pause and Resume methods, and a stop request
    /// is made with a call to Stop().
    /// If any of the control methods are called with the thread in an invalid state
    /// (eg. calling Pause() on an already paused thread) an exception will be thrown
    /// indicating which states are allowed, and the current state of the thread.
    /// </summary>
    /// <remarks>
    /// Note that this class is internal because outside of the scheduler, there is no
    /// need for classes to directly manipulate this thread - it can be done through
    /// the Scheduler class, which access its instance of this class itself.
    /// </remarks>
    public class SchedulerThread
    {
        /// <summary>
        /// Delegate to handle 'Triggered' events firing in this thread.
        /// </summary>
        /// <param name="args">The arguments defining the triggered event which
        /// occurred in this thread.</param>
        public delegate void HandleTriggeredEvent(TriggeredEventArgs args);

        /// <summary>
        /// Delegate to handle lifecycle events within this thread.
        /// </summary>
        /// <param name="mode">The new mode which is being entered as part of
        /// the lifecycle of this scheduler thread.</param>
        public delegate void HandleLifecycleEvent(ExecutionMode mode);

        /// <summary>
        /// The event fired when a trigger is activated. 
        /// The handler will receive details about the activation in the
        /// TriggeredEventArgs.
        /// </summary>
        public event HandleTriggeredEvent Triggered;

        /// <summary>
        /// The event fired when the lifecycle of this thread has changed.
        /// </summary>
        public event HandleLifecycleEvent ExecutionModeChanged;

        /// <summary>
        /// Object to provide a monitor-lock for interaction between the background
        /// thread and the thread-modification methods in this class.
        /// </summary>
        protected readonly object LOCK = new object();

        /// <summary>
        /// Gets the lock for interaction between the background thread maintained
        /// by this object and calling threads.
        /// </summary>
        /// <returns>An object, specific to this instance, which can be used as a 
        /// monitor to manage multi-thread interaction safely.</returns>
        private object GetLock()
        {
            return LOCK;
        }

        /// <summary>
        /// A unique (within current memory) ID for each thread instance
        /// </summary>
        private static int threadNo;


        private readonly object _schedulerThread = new object();

        /// <summary>
        /// The allowed states for this thread.
        /// </summary>
        public enum State
        {
            Unstarted, Starting, Running, Pausing, Paused, Resuming, Stopping, Stopped
        }

        private ExecutionMode _mode;
        private IDiary _diary;
        protected State _state;
        private Thread _thread;
        private DateTime _lastTime;
        private DateTime _nextTime;

        /// <summary>
        /// The job diary that this thread uses to schedule the jobs being
        /// executed. Any changes to the diary
        /// </summary>
        internal IDiary Diary
        {
            get { return _diary; }
            set
            {
                if (value == _diary) // already there?
                    return;

                lock (GetLock())
                {
                    // Remove the listener from the current diary
                    if (_diary != null)
                        _diary.DiaryUpdated -= HandleDiaryUpdate;

                    value.DiaryUpdated += HandleDiaryUpdate;
                    _diary = value;
                    // Wake the background thread so that it can pick up 
                    // the new diary.
                    Monitor.PulseAll(GetLock());
                }
            }

        }

        internal event LogStatus StatusUpdated;

        /// <summary>
        /// Logs a status update to any listeners.
        /// </summary>
        /// <param name="msg">The message indicating the reason for the 
        /// status update.
        /// </param>
        private void LogStatusUpdate(string msg)
        {
            try
            {
                if (StatusUpdated != null)
                    StatusUpdated(msg);
            }
            catch { }
        }

        /// <summary>
        /// Logs a status update from within a background thread.
        /// </summary>
        /// <param name="msg">The update message to log.</param>
        private void LogThreadUpdate(string msg)
        {
            LogStatusUpdate(msg);
        }

        /// <summary>
        /// Logs a status update from within a background thread.
        /// </summary>
        /// <param name="msg">The update message with string formatting
        /// markers.</param>
        /// <param name="args">The arguments for the formatted string.
        /// </param>
        private void LogThreadUpdate(string msg, params Object[] args)
        {
            LogThreadUpdate(String.Format(msg, args));
        }

        /// <summary>
        /// Creates a new scheduler thread from the given scheduler.
        /// </summary>
        /// <param name="sched">The scheduler for whom this thread is operating</param>
        public SchedulerThread(IDiary diary)
        {
            Diary = diary;
            _diary.DiaryUpdated += HandleDiaryUpdate;

            _state = State.Unstarted;
            _lastTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets or creates a new thread which executes the Run() method on 
        /// this object. The name given is "SchedulerThread_{n}" where {n}
        /// represents an incrementing integer.
        /// </summary>
        /// <returns>The thread, registered with this object, which can be used 
        /// to monitor and execute schedules.
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private Thread GetThread()
        {
            if (_thread == null)
            {
                _thread = new Thread(this.Run);
                _thread.IsBackground = true;
                _thread.Name = String.Format("SchedulerThread_{0}", ++threadNo);
                LogStatusUpdate("[BT]  Creating system thread : " + _thread.Name);
            }
            return _thread;
        }

        /// <summary>
        /// We need to handle the job store change as it may change when the
        /// background thread wakes up to handle the next trigger activation.
        /// </summary>
        /// <param name="sender">The job store which has caused this event.
        /// </param>
        private void HandleDiaryUpdate(IDiary sender)
        {
            LogStatusUpdate("[" + (_thread == null ? "null" : _thread.Name) + "]  "+
                "Diary updated... checking times");
            // If it's running, all we need to do is pulse the thread to
            // wake it up - next time round the loop it queries the job
            // store to see a) if it's missed any instances since the
            // last time it was run and b) how long it should wait 
            // before the next time it is due to run.
            lock (GetLock())
                Monitor.PulseAll(GetLock());
        }

        /// <summary>
        /// Notifies the background thread that the job map it is
        /// using to determine when to run has changed and it may
        /// need to query it again to get a new time to activate.
        /// This shouldn't need to be called, since this thread is
        /// listening for jobstore change events on its job
        /// store anyway.
        /// </summary>
        public void HandleDiaryUpdate()
        {
            HandleDiaryUpdate(Diary);
        }

        /// <summary>
        /// Checks if this thread is unstarted.
        /// </summary>
        /// <returns>True if this thread is still in an unstarted state,
        /// false otherwise.</returns>
        public bool IsUnstarted()
        {
            lock (GetLock())
                return (_state == State.Unstarted);
        }

        /// <summary>
        /// Checks if this thread is currently running (or about to run)
        /// </summary>
        /// <returns>true if this thread is running or due to transition to 
        /// a running state; false otherwise.
        /// More specifically, this will return true if the thread is:
        /// starting, running or resuming. </returns>
        public bool IsRunning()
        {
            lock (GetLock())
            {
                switch (_state)
                {
                    case State.Starting:
                    case State.Running:
                    case State.Resuming:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Checks if this thread is paused (or about to pause)
        /// </summary>
        /// <returns>true if this thread is currently paused, or about to
        /// transition to a paused state; false otherwise.</returns>
        public bool IsPaused()
        {
            lock (GetLock())
                return (_state == State.Pausing || _state == State.Paused);
        }

        /// <summary>
        /// Checks if this thread is currently stopping
        /// </summary>
        /// <returns>true if this thread is currently awaiting being
        /// stopped; false otherwise.</returns>
        public bool IsStopping()
        {
            lock (GetLock())
                return (_state == State.Stopping);
        }

        /// <summary>
        /// Checks if this thread has been stopped
        /// </summary>
        /// <returns>true if this thread is stopped; false otherwise.
        /// </returns>
        public bool IsStopped()
        {
            lock (GetLock())
                return (_state == State.Stopped);
        }

        /// <summary>
        /// Sets the execution mode of this thread, ensuring that any
        /// events are fired as necessary.
        /// </summary>
        /// <param name="mode">The mode to set in this thread and to 
        /// inform any listeners of.</param>
        private void SetExecutionMode(ExecutionMode mode)
        {
            _mode = mode;
            if (ExecutionModeChanged != null)
                ExecutionModeChanged(mode);
        }


        /// <summary>
        /// Checks if this thread is currently active. A thread is
        /// considered active if it is currently running, paused or
        /// transitioning to either of those states.
        /// It is considered inactive if it is unstarted, stopping
        /// or stopped.
        /// </summary>
        /// <returns>true if this thread is still 'active', false if
        /// it is inactive.</returns>
        public bool IsActive()
        {
            lock (GetLock())
            {
                switch (_state)
                {
                    case State.Unstarted:
                    case State.Stopping:
                    case State.Stopped:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// The main loop for this daemon thread. This handles the state
        /// changes requested in this object's public methods.
        /// The handling of the 'Running' state is where the primary 
        /// </summary>
        private void Run()
        {
            // Handle all state checks inside a lock
            lock (GetLock())
            {
                // When stop is requested, exit the main loop
                while (_state != State.Stopping)
                {
                    try
                    {
                        ProcessRunState();
                    }
                    catch (IllegalThreadStateException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        // Wait for connection to be restored
                        LogThreadUpdate("Error while processing state; "+
                            "waiting to try again: {0}", e); 
                        Monitor.Wait(GetLock(), 60 * 1000);
                    }
                }

                // Set the thread back to null to allow a new thread to
                // be created if Start() is called again.
                lock (_schedulerThread)
                {
                    _thread = null;
                }

                // Set our state to stopped and let anyone waiting on our signal know.
                _state = State.Stopped;
                Monitor.PulseAll(GetLock());

            }

            LogThreadUpdate("System thread complete");

        }

        /// <summary>
        /// Processes the current run state of the active scheduler thread.
        /// </summary>
        private void ProcessRunState()
        {
            switch (_state)
            {
                // Deal with the 'transition' states first

                case State.Pausing:
                    LogThreadUpdate("Pausing");
                    _state = State.Paused;
                    // let anyone waiting for the pause know that it's occurring
                    Monitor.PulseAll(GetLock());
                    break;

                case State.Starting:
                case State.Resuming:
                    LogThreadUpdate(_state.ToString());

                    // ensure that the thread is aware that any triggers are
                    // operating in startup mode.
                    SetExecutionMode(_state == State.Starting
                        ? ExecutionMode.Startup
                        : ExecutionMode.Resume);

                    _state = State.Running;
                    Monitor.PulseAll(GetLock());
                    break;

                // case State.Stopping can safely no-op
                // it will exit out on the next iteration through the loop.

                // Now the 'hard' states

                case State.Paused:
                    LogThreadUpdate("Paused");
                    Monitor.Wait(GetLock());
                    break;

                case State.Stopped:
                case State.Unstarted:
                    LogThreadUpdate("Bad state " + _state);
                    throw new IllegalThreadStateException(
                        "In active thread while state is: " + _state);

                case State.Running:

                    // reset nextTime to ensure that we ge the correct instance
                    _nextTime = Diary.GetNextActivationTime(_lastTime);

                    LogThreadUpdate("Last activation time: " + _lastTime + "; " +
                        "Next activation time " + _nextTime);

                    // First off, a fringe case - are there no more activations
                    // for eternity?
                    if (_nextTime == DateTime.MaxValue)
                    {
                        LogThreadUpdate("Waiting forever");
                        // Might as well wait forever
                        Monitor.Wait(GetLock());
                    }
                    // Or _nextTime is yet to come... wait til it arrives
                    else if (_nextTime > DateTime.Now)
                    {
                        // Wait until _nextTime
                        long ticksToSleep = _nextTime.Ticks - DateTime.Now.Ticks;
                        long millisToSleep = ticksToSleep / TimeSpan.TicksPerMillisecond;

                        // Ensure there's no overflow (see bug #5291) - I'm sure the
                        // system can cope with being woken up a little early.
                        if (millisToSleep > int.MaxValue)
                            millisToSleep = int.MaxValue;

                        LogThreadUpdate("Waiting for {0}", 
                            TimeSpan.FromMilliseconds(millisToSleep));

                        // Just check that we've not passed the next time in the
                        // scant few lines of code up to here...
                        // If we have, just loop round and we'll pick it up in the
                        // next iteration. Otherwise, sleep until it's due.
                        if (millisToSleep > 0)
                            Monitor.Wait(GetLock(), (int)millisToSleep);
                    }
                    else // ie. _nextTime is in the past, meaning we have something to run
                    {
                        // process all of them until we reach the current time...
                        while (_nextTime < DateTime.Now)
                        {
                            LogThreadUpdate("Activating triggers at " + _nextTime);

                            // GO! GO! GO!
                            ActivateTriggers(_nextTime);

                            // record when we last ran...
                            _lastTime = _nextTime;

                            // reset nextTime to ensure that we ge the correct instance
                            _nextTime = Diary.GetNextActivationTime(_nextTime);
                        }

                        // go through the loop and await the next time
                    }
                    // whatever mode we came in as, we're now operating in 'normal'
                    // mode since we've caught up any schedules which should have
                    // executed while this thread wasn't running
                    SetExecutionMode(ExecutionMode.Normal);
                    break;
            }
        }

        /// <summary>
        /// Activates the trigger instances for a particular point in time.
        /// </summary>
        /// <param name="instant">The instant in time for which the triggers should
        /// be activated.</param>
        /// <param name="mode">The mode in which these triggers should be activated
        /// </param>
        /// <exception cref="IndeterminateTriggerException">If a trigger instance
        /// which is to be activated has no determined activation mode and the 
        /// misfire response indicated that the trigger should be activated.
        /// </exception>
        private void ActivateTriggers(DateTime instant)
        {
            // get all the instances we need to deal with
            ICollection<ITriggerInstance> instances = Diary.GetInstancesFor(instant);

            LogThreadUpdate("Activating {0} instances at {1:dd/MM/yyyy HH:mm:ss} in mode {2}", 
                instances.Count, instant, _mode);

            // For each (firing) instance, get the job related to it
            foreach (ITriggerInstance inst in instances)
            {
                // ignore those instances that have an invalid time
                if (!inst.IsTimeValid())
                {
                    LogThreadUpdate($"'{inst}': Instance time is not valid in {inst.Trigger.PrimaryMetaData.TimeZoneId} - not firing.", inst);
                    continue;
                }

                // ignore those already activated
                if (inst.HasActivated())
                {
                    LogThreadUpdate("'{0}': Already activated - not firing", inst); 
                    continue;
                }

                if (Triggered != null)
                {
                    LogThreadUpdate("'{0}': Firing", inst); 
                    try
                    {
                        Triggered(new TriggeredEventArgs(inst, _mode));
                    }
                    // log (but ultimately ignore) any errors thrown by listeners
                    catch (Exception e)
                    {
                        LogThreadUpdate("Error thrown when firing {0}: {1}", inst, e); 
                    }
                }
                else
                {
                    LogThreadUpdate(
                        "Nothing is listening for the Triggered event : Instance {0} was ignored", 
                        inst);
                }
            }
        }

        /// <summary>
        /// Checks that the current state of this thread is one of the given
        /// allowed states that enable the specified function.
        /// </summary>
        /// <param name="function">The function that has been requested.</param>
        /// <param name="allowedStates">The valid states in which the function
        /// can be correctly carried out.</param>
        /// <exception cref="IllegalThreadStateException">If the current state
        /// does not match one of the specified allowed states.</exception>
        private void CheckState(string function, params State[] allowedStates)
        {
            if (!((ICollection<State>)allowedStates).Contains(_state))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Resources.InvalidState0InOrderToPerformThe1FunctionTheStateMustBeOneOf, _state, function);
                foreach (State state in allowedStates)
                    sb.Append(state).Append(',');
                sb.Length--;
                throw new IllegalThreadStateException(sb.ToString());
            }
        }

        /// <summary>
        /// <para>
        /// Blocks the current thread until the background thread's state changes.
        /// </para><para>
        /// <strong>Note: </strong> this should only be called with a monitor
        /// lock on this instance of SchedulerThread.
        /// </para>
        /// </summary>
        /// <param name="states">The expected states which will cause a natural
        /// return from this method. If the state changes to a state not specified
        /// here, an IllegalThreadStateException will be thrown.</param>
        /// <exception cref="IllegalThreadStateException">If the state of the
        /// thread changed to a state not specified in the given states.
        /// </exception>
        private void WaitForStateChange(params State[] states)
        {
            WaitForStateChange(true, 0, states);
        }

        /// <summary>
        /// <para>Blocks the current thread until the background thread's state changes
        /// to one of those specified, optionally failing if the state changes to
        /// a state not provided in the anticipated states.
        /// </para><para>
        /// <strong>Note: </strong> this should only be called with a monitor
        /// lock on this instance of SchedulerThread.
        /// </para>
        /// </summary>
        /// <param name="states">The expected states which will cause a natural
        /// return from this method. If the state changes to a state not specified
        /// here, an IllegalThreadStateException will be thrown.</param>
        /// <param name="failOnUnanticipatedChange">true to make this method throw
        /// an exception if the state changes to a different value than those
        /// provided in the given <paramref name="states"/></param>
        /// <exception cref="IllegalThreadStateException">If the state of the
        /// thread changed to a state not specified in the given states and
        /// <paramref name="failOnUnanticipatedChange"/> is <c>true</c>.
        /// </exception>
        private void WaitForStateChange(bool failOnUnanticipatedChange, params State[] states)
        {
            WaitForStateChange(failOnUnanticipatedChange, 0, states);
        }

        /// <summary>
        /// <para>Blocks the current thread until the background thread's state changes
        /// to one of those specified, optionally failing if the state changes to
        /// a state not provided in the anticipated states or returning (cleanly) if
        /// a given positive timeout is reached.
        /// </para><para>
        /// <strong>Note: </strong> this should only be called with a monitor
        /// lock on this instance of SchedulerThread.
        /// </para>
        /// </summary>
        /// <param name="states">The expected states which will cause a natural
        /// return from this method. If the state changes to a state not specified
        /// here, an IllegalThreadStateException will be thrown.</param>
        /// <param name="failOnUnanticipatedChange">true to make this method throw
        /// an exception if the state changes to a different value than those
        /// provided in the given <paramref name="states"/></param>
        /// <param name="timeoutMillis">A number of milliseconds to allow for the
        /// required state changes to occur, returning if the desired state/s
        /// have not been reached within that period. 0 or a negative number
        /// indicates that no timeout should be used.</param>
        /// <exception cref="IllegalThreadStateException">If the state of the
        /// thread changed to a state not specified in the given states and
        /// <paramref name="failOnUnanticipatedChange"/> is <c>true</c>.
        /// </exception>
        private void WaitForStateChange(
            bool failOnUnanticipatedChange, int timeoutMillis, params State[] states)
        {
            // NOTE: Any calling method must already have a sync lock on 
            // this instance, or an exception will be thrown here by the
            // Monitor.Wait() method.

            // Get the start time to deal with the timeout
            long start = DateTime.Now.Ticks;

            // Cast array to ICollection for the 'Contains' method
            ICollection<State> coll = (ICollection<State>)states;

            // Get the initial state to deal with unanticipated state changes
            State initialState = _state;

            // Check for the allowed states... 
            // if the state changes from the initial state to something else,
            // and 'failOnUnanticipatedChange' is set, throw an exception.
            // This is to deal with multiple threads calling multiple methods
            // on the scheduler, and another thread getting its call dealt
            // with first - eg. 
            // if state is 'Starting' and: t1 calls Pause() and t2 calls Stop() 
            // both wait for thread to reach 'Running' state - if t2's call is
            // carried out first, then from t1's point of view, the state
            // goes from 'Starting' to 'Stopping', and thus it must fail.
            while (!coll.Contains(_state))
            {
                if (_state != initialState && failOnUnanticipatedChange)
                {
                    // state has changed externally to this thread...
                    // we need to bail
                    StringBuilder sb = new StringBuilder(Resources.WaitingForStates);
                    foreach (State state in states)
                    {
                        sb.Append(state).Append(',');
                    }
                    sb.Length--;
                    sb.AppendFormat(Resources.StateChangedFrom0To1, initialState, _state);

                    throw new IllegalThreadStateException(sb.ToString());
                }
                if (timeoutMillis < 1) // no timeout
                {
                    Monitor.Wait(GetLock());
                }
                else
                {
                    int period = (int)(DateTime.Now.Ticks - start);
                    if (period > timeoutMillis) // we've gone over our timeout
                        return;

                    // else we wait for the remainder of the specified timeout
                    Monitor.Wait(GetLock(), timeoutMillis - period);
                }
            }
        }

        /// <summary>
        /// Starts the thread. It must be in an unstarted or stopped state
        /// when this method is called.
        /// </summary>
        /// <param name="millisToCheck">The number of milliseconds into the
        /// past that the thread should check for missed schedules when it
        /// has started. 0 indicates that it should start from the current
        /// point in time and not look into the past at all.</param>
        /// <exception cref="IllegalThreadStateException">If this thread is
        /// in a state other than State.Unstarted or State.Stopped.
        /// </exception>
        public void Start(int millisToCheck)
        {
            lock (GetLock())
            {
                CheckState("Start", State.Unstarted, State.Stopped);
                _state = State.Starting;
                // Get the time we should check from... only set it
                // if we have not already checked part of that time
                // ie. if the checktime is later than the last time
                // that this thread checked for triggers.
                // Otherwise, we'll be checking triggers which have
                // already been processed.
                DateTime checkTime = DateTime.Now.AddMilliseconds(-millisToCheck);
                if (checkTime > _lastTime)
                    _lastTime = checkTime;

                GetThread().Start();

            }
        }

        /// <summary>
        /// Starts the thread. It must be in an unstarted or stopped state
        /// when this method is called.
        /// This will <em>not</em> check for any missed schedules in the 
        /// past but operate from the moment the Start() method was called.
        /// </summary>
        /// <exception cref="IllegalThreadStateException">If this thread is
        /// in a state other than State.Unstarted or State.Stopped.
        /// </exception>
        public void Start()
        {
            Start(0);
        }

        /// <summary>
        /// Pauses the thread.
        /// It must be in a state of either 'Starting', 'Running', or 'Resuming'
        /// when this method is called
        /// </summary>
        /// <exception cref="IllegalThreadStateException">If this thread is not
        /// currently starting, running or resuming.</exception>
        public void Pause()
        {
            lock (GetLock())
            {
                CheckState("Pause", State.Starting, State.Running, State.Resuming);
                // we need to wait until the thread is actually running before
                // we can get it pausing again to make sure the thread completes
                // any initialisation work it needs to do first...
                // block until such a state exists
                WaitForStateChange(State.Running); // fails if state changes to anything else

                _state = State.Pausing;
                Monitor.PulseAll(GetLock());
            }
        }

        /// <summary>
        /// Resumes a currently paused thread.
        /// It must be in a state of either 'Paused' or 'Pausing' when this
        /// method is called.
        /// </summary>
        /// <param name="fireMissedTriggers">true to indicate that any triggers
        /// which should have fired while the scheduler was paused should now
        /// be fired; false to only start firing triggers which occur from the
        /// moment that this method was called.</param>
        /// <exception cref="IllegalThreadStateException">If this object is not
        /// in a paused or pausing state.</exception>
        public void Resume(bool fireMissedTriggers)
        {
            lock (GetLock())
            {
                CheckState("Resume", State.Paused, State.Pausing);

                WaitForStateChange(State.Paused);

                _state = State.Resuming;
                Monitor.PulseAll(GetLock());
                if (!fireMissedTriggers)
                    _lastTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Requests that this thread be stopped.
        /// It must have been started and not previously stopped when this method
        /// is called.
        /// </summary>
        /// <exception cref="IllegalThreadStateException">If this object is either
        /// unstarted, stopping or already stopped.</exception>
        public void Stop()
        {
            lock (GetLock())
            {
                CheckState("Stop", State.Unstarted, State.Starting, State.Running,
                    State.Pausing, State.Paused, State.Resuming);
                // special case, if it's unstarted, just move it straight to Stopped
                // and miss out actually creating the thread
                if (_state == State.Unstarted)
                {
                    _state = State.Stopped;
                }
                else
                {
                    // Wait for a non-transitory state
                    WaitForStateChange(State.Running, State.Paused);
                    _state = State.Stopping;
                }
                Monitor.PulseAll(GetLock());
            }
        }

        /// <summary>
        /// Joins this thread.. usually called when a 'Stop' has been signalled, and
        /// awaiting the actual stopping of the thread.
        /// </summary>
        public void Join()
        {
            Join(0);
        }

        /// <summary>
        /// Joins this thread.. usually called when a 'Stop' has been signalled, and
        /// awaiting the actual stopping of the thread.
        /// This will timeout if the given number of milliseconds is reached and the
        /// thread has not been 'Stopped'.
        /// </summary>
        public void Join(int timeoutMillis)
        {
            long start = DateTime.Now.Ticks;
            // the only invalid state for this is 'unstarted'.
            // though quite why you'd want to join a running thread (or a 
            // starting thread) I have no idea - it's still theoretically valid.
            lock (GetLock())
            {
                if (_state == State.Unstarted)
                {
                    throw new IllegalThreadStateException(
                        Resources.SchedulerThreadIsNotStartedCannotJoinIt);
                }

                // quick check to see if we've already stopped... 
                // no point messing about if we have
                if (_state == State.Stopped)
                    return;

                // join can't be inside a lock - a thread doesn't release any locks
                // it has when it enters a join, and thus no other thread can continue
                // while it's blocking (meaning the thread would never exit and thus
                // the timeout would always be reached).
                // So, I'm "emulating" a Join using a looped Wait() looking for the
                // 'Stopped' state. Any state changes in between are valid, and 
                // the timeout should be as we've had it passed to us, less any time
                // spent acquiring the monitor lock on this instance
                int timePassed = (int)(DateTime.Now.Ticks - start);

                // have we hit the timeout already?
                if (timeoutMillis > 0 && timePassed >= timeoutMillis)
                    return;

                // the new timeout is any difference left over after time passed is
                // subtracted from the given timeout.
                // If no timeout was given, ensure that that fact is passed onward too.
                int newTimeout = (timeoutMillis < 1 ? 0 : timeoutMillis - timePassed);

                WaitForStateChange(false, newTimeout, State.Stopped);
            }
        }
    }
}
