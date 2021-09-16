using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.DigitalWorker.Sessions.Coordination;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
// Certain tests don't await calls to async methods by design
#pragma warning disable 4014

namespace BluePrism.DigitalWorker.UnitTests.Sessions.Coordination
{
    public class SessionCoordinatorTests : UnitTestBase<SessionCoordinator>
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromMilliseconds(250);
        private SessionContextBuilder _contextBuilder;

        public override void Setup()
        {
            _contextBuilder = new SessionContextBuilder();
            base.Setup();
        }
        
        [Test]
        public void Start_Ready_ShouldRunSession()
        {
            var context = _contextBuilder.Background().Build();
            var session = SetupSessionRunner(context);
            
            var task = ClassUnderTest.RunProcess(context, new CancellationToken());
            session.AssertStarted();
            task.IsCompleted.Should().BeFalse();
        }

        [Test]
        public async Task Start_ShouldCompleteWhenFinished()
        {
            var context = _contextBuilder.Background().Build();
            var runningSession = SetupSessionRunner(context);
            
            var task = ClassUnderTest.RunProcess(context, new CancellationToken());
            runningSession.AssertStarted();
            runningSession.SetFinished();
            await task;
            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task Start_ExclusiveProcessWithOtherProcessesRunning_ShouldNotStartUntilFinished()
        {
            var background1 = _contextBuilder.Background().Build();
            var background2 = _contextBuilder.Background().Build();
            var foreground1 = _contextBuilder.Foreground().Build();
            var exclusive = _contextBuilder.Exclusive().Build();
            var sessions = SetupSessionRunner(background1, background2, foreground1, exclusive);

            var background1Task = ClassUnderTest.RunProcess(background1, new CancellationToken());
            var background2Task = ClassUnderTest.RunProcess(background2, new CancellationToken());
            var foreground1Task = ClassUnderTest.RunProcess(foreground1, new CancellationToken());
            var exclusiveTask = ClassUnderTest.RunProcess(exclusive, new CancellationToken());

            sessions[background1].AssertStarted();
            sessions[background2].AssertStarted();
            sessions[foreground1].AssertStarted();
            
            sessions[exclusive].AssertNotStarted();
            sessions[background1].SetFinished();
            sessions[background2].SetFinished();
            await Task.WhenAll(background1Task, background2Task);
            sessions[exclusive].AssertNotStarted();

            sessions[foreground1].SetFinished();
            await foreground1Task;
            sessions[exclusive].AssertStarted();
        }

        [Test]
        public async Task Start_WhenExclusiveProcessRunning_ShouldNotStartUntilFinished()
        {
            var exclusive = _contextBuilder.Exclusive().Build();
            var background1 = _contextBuilder.Background().Build();
            var background2 = _contextBuilder.Background().Build();
            var foreground1 = _contextBuilder.Foreground().Build();
            
            var sessions = SetupSessionRunner(exclusive, background1, background2, foreground1);

            var exclusiveTask = ClassUnderTest.RunProcess(exclusive, new CancellationToken());
            ClassUnderTest.RunProcess(background1, new CancellationToken());
            ClassUnderTest.RunProcess(background2, new CancellationToken());
            ClassUnderTest.RunProcess(foreground1, new CancellationToken());
            sessions[exclusive].AssertStarted();

            sessions[background1].AssertNotStarted();
            sessions[background2].AssertNotStarted();
            sessions[foreground1].AssertNotStarted();

            sessions[exclusive].SetFinished();
            await exclusiveTask;
            sessions[background1].AssertStarted();
            sessions[background2].AssertStarted();
            sessions[foreground1].AssertStarted();
        }

        [Test]
        public void Start_ExclusiveProcess_ShouldLockWhenRunning()
        {
            var context = _contextBuilder.Exclusive().Build();
            var session = SetupSessionRunner(context);

            ClassUnderTest.RunProcess(context, new CancellationToken());
            session.AssertStarted();

            GetMock<IExclusiveProcessLock>().Verify(x => x.Lock());
            GetMock<IExclusiveProcessLock>().Verify(x => x.Unlock(), Times.Never);
        }

        [Test]
        public async Task Start_ExclusiveProcess_ShouldUnlockWhenFinished()
        {
            var context = _contextBuilder.Exclusive().Build();
            var session = SetupSessionRunner(context);

            var task = ClassUnderTest.RunProcess(context, new CancellationToken());
            session.AssertStarted();
            session.SetFinished();
            await task;
            GetMock<IExclusiveProcessLock>().Verify(x => x.Unlock(), Times.Once);
        }

        private Dictionary<SessionContext, RunningSession> SetupSessionRunner(params SessionContext[] sessions)
        {
            var runningSessions = new Dictionary<SessionContext, RunningSession>();
            foreach (var session in sessions)
            {
                runningSessions[session] = SetupSessionRunner(session);
            }
            return runningSessions;
        }

        private RunningSession SetupSessionRunner(SessionContext session)
        {
            var startCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var finishCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var runningSession = new RunningSession(startCompletionSource, finishCompletionSource);

            GetMock<ISessionRunner>().Setup(x => x.RunAsync(session))
                .Returns(() =>
                {
                    startCompletionSource.SetResult(true);
                    return finishCompletionSource.Task;
                });
            return runningSession;
        }

        private class RunningSession
        {
            private readonly TaskCompletionSource<bool> _startCompletionSource;

            private TaskCompletionSource<bool> _finishCompletionSource;

            public RunningSession(TaskCompletionSource<bool> startCompletionSource, TaskCompletionSource<bool> finishCompletionSource)
            {
                _startCompletionSource = startCompletionSource;
                _finishCompletionSource = finishCompletionSource;
            }
            
            public void AssertStarted()
            {
                // This should be near-instant if we're expecting the session to have started
                bool started = _startCompletionSource.Task.Wait(WaitTimeout);
                started.Should().BeTrue();
            }

            public void AssertNotStarted()
            {
                bool started = _startCompletionSource.Task.Wait(WaitTimeout);
                started.Should().BeFalse();
            }

            /// <summary>
            /// Sets Task returned by mocked ISessionRunner.RunAsync to complete, indicating
            /// that the session has finished.
            /// </summary>
            public void SetFinished() => _finishCompletionSource.SetResult(true);
        }
    }
}