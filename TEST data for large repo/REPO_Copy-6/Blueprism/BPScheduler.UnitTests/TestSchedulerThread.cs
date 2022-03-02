#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Threading;
using BluePrism.Core.Utility;
using NUnit.Framework;
using static BluePrism.Scheduling.SchedulerThread;

namespace BluePrism.Scheduling
{
    [TestFixture]
    public class SchedulerThreadTests
    {
        [Test]
        public void TestTwoPauseRace()
        {
            var thread = new SchedulerThreadEx();
            SchedulerThreadHelper.TestRace(thread.CallPause, thread.CallPause);
        }

        [Test]
        public void TestUnstarted()
        {
            var thread = new SchedulerThreadEx();

            // Test the state flag accessors
            SchedulerThreadHelper.CheckStateFlags(thread, true, false, false, false, false, false);

            // many of the thread functions should fail if it is unstarted.
            Assert.That(thread.Pause, Throws.InstanceOf<IllegalThreadStateException>());
            // Without lambda expressions, testing exceptions is a pain for any
            // parameterized methods.
            SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);
            Assert.That(thread.Join, Throws.InstanceOf<IllegalThreadStateException>());

            // Stop() should work - it will stop the thread before it's started
            thread.Stop();
        }


        [Test]
        public void TestStart()
        {
            // NOTE: implementation-aware test

            var thread = new SchedulerThreadEx();

            // we want a lock on the thread so that it cannot transition states
            // within the actual worker thread
            lock (thread.GetLock())
            {
                thread.Start();
                // Check that the thread is starting... check externals
                SchedulerThreadHelper.CheckStateFlags(thread, false, true, true, false, false, false);

                // start should fail while it's starting up - also resume, since it's not paused
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);

                // And check that the status is 'Starting'
                Assert.AreEqual(thread.ThreadState, State.Starting);

                // once we leave the lock the worker thread is free to effect
                // the transition of the state.
                // Just give it a bit of a nudge..
                Monitor.PulseAll(thread.GetLock());
            }

            // that said, it's difficult to force the worker to do so
            // try a sleep.. though it really guarantees nothing...
            SchedulerThreadHelper.Wait(50);

            lock (thread.GetLock())
            {
                // if we're not running now, then there's no point in bothering
                // with further tests...
                Assume.That(thread.ThreadState == State.Running); // inconclusive without this state.

                // The checks are identical to when it's starting up... you
                // simply can't see inside the class at that sort of level..
                // ...nevertheless, it might catch some issues

                // Check that the thread has started
                SchedulerThreadHelper.CheckStateFlags(thread, false, true, true, false, false, false);

                // start should fail once it's started, also again... Resume
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);

            }
            thread.Stop();
        }

        [Test]
        public void TestPauseAndResume()
        {
            var thread = new SchedulerThreadEx();
            thread.Start();

            // Allow the thread to go...
            SchedulerThreadHelper.Wait(50);

            // Make sure we're good to go...
            Assume.That(thread.IsRunning());

            // Okay, check that the state goes to pausing first
            lock (thread.GetLock())
            {
                thread.Pause();

                // on our way down?
                Assert.That(thread.ThreadState == State.Pausing);

                // active & paused are true, all else are false
                SchedulerThreadHelper.CheckStateFlags(thread, false, false, true, true, false, false);

                // When pausing, can't start or (re?)pause the thread.
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                Assert.That(thread.Pause, Throws.InstanceOf<IllegalThreadStateException>());

            }

            // outside the lock - let the thread go into a paused state.
            SchedulerThreadHelper.Wait(50);

            lock (thread.GetLock())
            {
                // Make sure we've entered a paused state
                Assume.That(thread.ThreadState == State.Paused);

                // active & paused are still true, all else are still false
                SchedulerThreadHelper.CheckStateFlags(thread, false, false, true, true, false, false);

                // while paused, can't start or repause the thread
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                Assert.That(thread.Pause, Throws.InstanceOf<IllegalThreadStateException>());

                // okay at this point we're paused
                // try a pulse to make sure that if it's woken it remains
                // in a paused state.
                Monitor.PulseAll(thread.GetLock());
            }

            SchedulerThreadHelper.Wait(50);

            lock (thread.GetLock())
            {
                // Still paused, right?
                Assert.That(thread.ThreadState == State.Paused);

                // Good - time to test resuming.
                thread.Resume(false);

                Assert.That(thread.ThreadState == State.Resuming);

                // Should be running and active - as if it had already resumed...
                // basically assume that the resume goes correctly.
                SchedulerThreadHelper.CheckStateFlags(thread, false, true, true, false, false, false);

                // Can't re-resume or start
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);
            }

            SchedulerThreadHelper.Wait(50);

            lock (thread.GetLock())
            {
                Assume.That(thread.ThreadState == State.Running);

                // Should be running and active - as if it was normally running
                // which... you know.. it kinda should be.
                SchedulerThreadHelper.CheckStateFlags(thread, false, true, true, false, false, false);

                // Can't re-resume or start
                Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
                SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);
            }

            // Okay, if you Resume() a thread while it's still in the transitory
            // state of Pausing, it blocks until the state of Paused has been
            // reached, and then acts as normal - make sure that is the case
            // also (though it should never happen given there's no public 
            // accessor to the thread lock) make sure it does not deadlock.
            lock (thread.GetLock())
            {
                thread.Pause();
                Assume.That(thread.ThreadState == State.Pausing);

                thread.Resume(false); // blocked until thread is unpaused...

                // at this point the thread should be 'resuming' again
                Assert.That(thread.ThreadState == State.Resuming);

            }

            SchedulerThreadHelper.Wait(50);

            // paused & resumed twice - make sure it's running correctly.
            SchedulerThreadHelper.CheckStateFlags(thread, false, true, true, false, false, false);

            // Can't re-resume or start
            Assert.That(thread.Start, Throws.InstanceOf<IllegalThreadStateException>());
            SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);

            thread.Stop();
        }

        [Test]
        public void TestStopFromUnstarted()
        {
            var thread = new SchedulerThreadEx();
            thread.Stop();
            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);
            SchedulerThreadHelper.AssertFullyStopped(thread);
        }

        [Test]
        public void TestStopFromStarting()
        {
            var thread = new SchedulerThreadEx();
            lock (thread.GetLock())
            {
                thread.Start();
                Assume.That(thread.ThreadState == State.Starting);
                thread.Stop();
                Assert.That(thread.ThreadState == State.Stopping);
                SchedulerThreadHelper.CheckStateFlags(thread, false, false, false, false, true, false);
            }

            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);
            SchedulerThreadHelper.AssertFullyStopped(thread);

        }

        [Test]
        public void TestStopFromRunning()
        {
            var thread = new SchedulerThreadEx();

            thread.Start();
            SchedulerThreadHelper.Wait(50);

            Assume.That(thread.ThreadState == State.Running);

            thread.Stop();
            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);

            SchedulerThreadHelper.AssertFullyStopped(thread);
        }

        [Test]
        public void TestStopFromPausing()
        {
            var thread = new SchedulerThreadEx();

            thread.Start();
            SchedulerThreadHelper.Wait(50);

            lock (thread.GetLock())
            {
                thread.Pause();
                Assume.That(thread.ThreadState == State.Pausing);

                thread.Stop();
                Assert.That(thread.ThreadState == State.Stopping);
                SchedulerThreadHelper.CheckStateFlags(thread, false, false, false, false, true, false);
            }

            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);

            SchedulerThreadHelper.AssertFullyStopped(thread);
        }

        [Test]
        public void TestStopFromPaused()
        {
            var thread = new SchedulerThreadEx();
            thread.Start();
            thread.Pause();
            SchedulerThreadHelper.Wait(50);
            Assume.That(thread.ThreadState == State.Paused);
            thread.Stop();
            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);
            SchedulerThreadHelper.AssertFullyStopped(thread);
        }

        [Test]
        public void TestStopFromResuming()
        {
            var thread = new SchedulerThreadEx();
            thread.Start();
            thread.Pause();
            SchedulerThreadHelper.Wait(50);
            lock (thread.GetLock())
            {
                thread.Resume(false);
                Assume.That(thread.ThreadState == State.Resuming);
                thread.Stop();
                Assert.That(thread.ThreadState == State.Stopping);
                SchedulerThreadHelper.CheckStateFlags(thread, false, false, false, false, true, false);
            }
            SchedulerThreadHelper.WaitAndCheckForThreadStateChanged(thread, State.Stopped);
            SchedulerThreadHelper.AssertFullyStopped(thread);
        }

        [Test]
        public void TestStopFromStopping()
        {
            var thread = new SchedulerThreadEx();
            thread.Start();
            SchedulerThreadHelper.Wait(50);
            lock (thread.GetLock())
            {
                thread.Stop();
                Assume.That(thread.ThreadState == State.Stopping);
                Assert.That(thread.Stop, Throws.InstanceOf<IllegalThreadStateException>());
            }
        }
    }

    internal class ThreadData
    {
        public readonly object YourLock = new object();
        public readonly SchedulerThread Thread;
        public Exception Exception;
        public ThreadData(SchedulerThread thread, object yourLock)
        {
            Thread = thread;
            YourLock = yourLock;
        }
    }

    internal class SchedulerThreadHelper
    {
        public static void Wait(int millisecondsToWait)
        {
            var clock = new SystemClockWrapper();
            var stop = clock.Now.AddMilliseconds(millisecondsToWait);
            while (clock.Now < stop)
            {
                // wait
            }
        }

        public static void CheckResumeThrowsIllegalThreadStateException(SchedulerThread thread)
        {
            try
            {
                thread.Resume(false);
                Assert.Fail("Expected IllegalThreadStateException - no exception thrown");
            }
            catch (IllegalThreadStateException) { }
            catch (Exception e)
            {
                Assert.Fail("Expected IllegalThreadStateException - got: " + e);
            }
        }

        public static void CheckStateFlags(SchedulerThread thread,
    bool unstarted, bool running, bool active, bool paused, bool stopping, bool stopped)
        {
            Assert.AreEqual(thread.IsUnstarted(), unstarted);
            Assert.AreEqual(thread.IsRunning(), running);
            Assert.AreEqual(thread.IsActive(), active);
            Assert.AreEqual(thread.IsPaused(), paused);
            Assert.AreEqual(thread.IsStopping(), stopping);
            Assert.AreEqual(thread.IsStopped(), stopped);
        }

        public static void WaitAndCheckForThreadStateChanged(SchedulerThreadEx thread, State state)
        {
            for (var i = 0; i < 10; i++)
            {
                Wait(50);
                if (thread.ThreadState == state)
                {
                    return;
                }
            }
        }

        public static void AssertFullyStopped(SchedulerThreadEx thread)
        {
            Assert.That(thread.ThreadState == State.Stopped);

            SchedulerThreadHelper.CheckStateFlags(thread, false, false, false, false, false, true);

            Assert.That(thread.Pause, Throws.InstanceOf<IllegalThreadStateException>());
            SchedulerThreadHelper.CheckResumeThrowsIllegalThreadStateException(thread);
            Assert.That(thread.Stop, Throws.InstanceOf<IllegalThreadStateException>());
        }

        public static void TestRace(ParameterizedThreadStart del1, ParameterizedThreadStart del2)
        {
            var thread = new SchedulerThreadEx();
            // okay, we create 2 separate threads which exist
            // to call 2 functions on the thread at the same time

            // it gets a bit fiddly here, trying to get 3 threads
            // working together in a semi-deterministic fashion, but I'll
            // give it a shot.
            var lock1 = new object();
            var lock2 = new object();

            var d1 = new ThreadData(thread, lock1);
            var d2 = new ThreadData(thread, lock2);

            var t1 = new Thread(del1);
            var t2 = new Thread(del2);

            // rightio, we're managing the locks here
            Monitor.Enter(lock1);
            Monitor.Enter(lock2);

            // Start them off but don't let them actually do their work yet.
            t1.Start(d1);
            t2.Start(d2);

            // now prepare the thread - we want to get it to starting state
            lock (thread.GetLock())
            {
                thread.Start();
                Assert.That(thread.ThreadState == State.Starting);

                // now we fire off the 2 threads... both should block
                // until the thread's state reaches Running
                Monitor.Exit(lock1); // this is when Monitor.Enter/Exit become very useful.
                Monitor.Exit(lock2);

                // leave the scheduler thread lock - Wait()
                // to let it do its thang
            }
            SchedulerThreadHelper.Wait(50);

            // join our 2 stooge threads to ensure they're done with.
            t1.Join(5000);
            t2.Join(5000);

            // Make sure they exited correctly - otherwise, could be a deadlock
            Assert.False(t1.IsAlive);
            Assert.False(t2.IsAlive);

            // Now one of those threads should have failed because the other
            // one succeeded - ie. they both saw a state of Starting, and they
            // both blocked until the state reached running... then either
            // the 'Pause()' thread saw the state change to 'Stopping/Stopped',
            // or the 'Stop()' thread saw the state change to 'Pausing/Paused'.
            // Neither are anticipated and thus they have to fail.
            Assert.That(
                d1.Exception is IllegalThreadStateException ||
                d2.Exception is IllegalThreadStateException
            );

            // Having said that, one of the threads should also have 
            // succeeded, so we'd better make sure of that too.
            Assert.That(d1.Exception == null || d2.Exception == null);

        }

    }

    internal class TestDiary : IDiary
    {
        public event HandleDiaryUpdated DiaryUpdated;

        public DateTime GetNextActivationTime(DateTime after)
        {
            return DateTime.MaxValue;
        }

        public ICollection<ITriggerInstance> GetInstancesFor(DateTime date)
        {
            return new ITriggerInstance[0];
        }

        private void OnlyHereToAllayTheIrritatingWarning()
        {
            DiaryUpdated(this);
        }
    }

    internal class SchedulerThreadEx : SchedulerThread
    {

        public SchedulerThreadEx()
            : base(new TestDiary()) { }
        
        public object GetLock()
        {
            return LOCK;
        }
      
        public void CallPause(object tdata)
        {
            var data = (ThreadData)tdata;
            lock (data.YourLock)
            {
                try
                {
                    data.Thread.Pause();
                }
                catch (Exception e)
                {
                    data.Exception = e;
                }
            }
        }

        public void CallStop(object tdata)
        {
            var data = (ThreadData)tdata;
            lock (data.YourLock)
            {
                try
                {
                    data.Thread.Stop();
                }
                catch (Exception e)
                {
                    data.Exception = e;
                }
            }
        }

        public State ThreadState {
            get { return _state; }
            private set { }
        }
        
    }
}

#endif