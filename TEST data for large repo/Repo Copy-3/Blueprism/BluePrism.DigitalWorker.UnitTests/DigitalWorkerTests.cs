using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.DigitalWorker.Notifications;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using NUnit.Framework;
using BluePrism.DigitalWorker.Messaging;
using GreenPipes.Internals.Extensions;
using Moq;
using MassTransit;

namespace BluePrism.DigitalWorker.UnitTests
{
    [TestFixture]
    public class DigitalWorkerTests : UnitTestBase<DigitalWorker>
    {
        private static readonly DigitalWorkerName TestDigitalWorkerName = new DigitalWorkerName("Worker1");
        private readonly DigitalWorkerStartUpOptions _startUpOptions = new DigitalWorkerStartUpOptions { Name = TestDigitalWorkerName };
        private static readonly TimeSpan TestMessageBusStartTimeout = TimeSpan.FromDays(1);
        private List<ResourceNotification> _notifications;
        private DateTime Now { get; } = DateTime.MinValue;

        protected override DigitalWorker TestClassConstructor()
            => new TestDigitalWorker(
                GetMock<IMessageBusWrapper>().Object,
                new DigitalWorkerContext(_startUpOptions),
                GetMock<IResourcePCView>().Object,
                GetMock<ISessionServiceClient>().Object,
                GetMock<IRunProcessQueueCoordinator>().Object,
                GetMock<INotificationHandler>().Object,
                GetMock<ILifecycleEventPublisher>().Object,
                GetMock<ITaskDelay>().Object);

        public override void Setup()
        {
            base.Setup();
            _notifications = new List<ResourceNotification>();
            GetMock<IResourcePCView>().Setup(x => x.DisplayNotification(It.IsAny<ResourceNotification>()))
                .Callback((ResourceNotification x) => _notifications.Add(x));

            // Immediately return task delay so registration retry doesn't increase test time
            GetMock<ITaskDelay>().Setup(x => x.Delay(It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);
        }

        [Test]
        public void SessionsRunning_IsFalse()
        {
            ClassUnderTest.SessionsRunning().Should().BeFalse();
        }

        [Test]
        public async Task Init_ShouldStartBus()
        {
            await RunInit();

            GetMock<IMessageBusWrapper>().Verify(x => x.Start(TestMessageBusStartTimeout), Times.Once);
            AssertNotification(ResourceNotificationLevel.Comment, "Message bus started");
        }

        [Test]
        public async Task Init_StartBusFails_ShouldAbortStartupAndDisplayWarning()
        {
            var testException = new Exception("Bus Startup Failed");
            GetMock<IMessageBusWrapper>().Setup(x => x.Start(TestMessageBusStartTimeout))
                .Throws(testException);

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Warning, "Message queue not available.");
        }

        [Test]
        public async Task Init_SuccessfullyRegisteredWithSessionService_ShouldDisplayCorrectNotification()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Comment, $"{_startUpOptions.Name.FullName} registered");
        }

        [Test]
        public async Task Init_SuccessfullyRegisteredWithSessionService_ShouldStartPublishingLifecycleEvents()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            GetMock<ILifecycleEventPublisher>().Verify(x => x.Start());
        }

        [Test]
        public async Task Init_SuccessfullyRegisteredWithSessionService_ShouldBeReadyToStartReceivingProcesses()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
              .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            GetMock<IRunProcessQueueCoordinator>().Verify(x => x.BeginReceivingMessages(), Times.Once);
        }

        [Test]
        public async Task Init_AttemptRegistrationWhenDigitalWorkerAlreadyOnline_ShouldDisplayCorrectNotification()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
               .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Error, $"Error starting digital worker: {_startUpOptions.Name.FullName} is already online.");
        }

        [Test]
        public async Task Init_AttemptRegistrationWhenDigitalWorkerAlreadyOnline_ShouldNotStartPublishingLifecycleEvents()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
               .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            GetMock<ILifecycleEventPublisher>().Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public async Task Init_AttemptRegistrationWhenDigitalWorkerAlreadyOnline_ShouldNotBeReadyToStartReceivingProcesses()
        {
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
               .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            GetMock<IRunProcessQueueCoordinator>().Verify(x => x.BeginReceivingMessages(), Times.Never);
        }

        [Test]
        public async Task Init_RegistrationTimesOutThenNextAttemptSucceeds_ShouldDisplayTimeoutNotification()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
              .Throws(new RequestTimeoutException())
              .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Error, "Timed out waiting for registration response");
        }

        [Test]
        public async Task Init_RegistrationTimesOutThenNextAttemptSucceeds_ShouldNotWaitBeforeRetrying()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
              .Throws(new RequestTimeoutException())
              .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            GetMock<ITaskDelay>().Verify(x => x.Delay(TimeSpan.FromSeconds(5)), Times.Never);
        }


        [Test]
        public async Task Init_RegistrationTimesOutThenNextAttemptSucceeds_ShouldStartPublishingLifecycleEvents()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Throws(new RequestTimeoutException())
                .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            GetMock<ILifecycleEventPublisher>().Verify(x => x.Start(), Times.Once);
        }

        [Test]
        public async Task Init_RegistrationTimesOutThenNextAttemptSucceeds_ShouldBeReadyToStartReceivingProcesses()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
               .Throws(new RequestTimeoutException())
               .Returns(Task.FromResult(RegisteredStatus.Registered));

            await RunInit();

            GetMock<IRunProcessQueueCoordinator>().Verify(x => x.BeginReceivingMessages(), Times.Once);
        }

        [Test]
        public async Task Init_UnexpectedRegistrationErrorsThenNextAttemptTheWorkerIsAlreadyOnline_ShouldDisplayErrorNotification()
        {
            var exception = new Exception("The session service robot is on its break. Try again soon.");
            GetMock<ISessionServiceClient>()
                .SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Throws(exception)
                .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Error, $"Error starting digital worker: {exception.ToString()}.");
        }
                
        [Test]
        public async Task Init_UnexpectedRegistrationErrorsThenNextAttemptTheWorkerIsAlreadyOnline_ShouldWaitFiveSecondsBeforeNextRetry()
        {
            GetMock<ISessionServiceClient>()
                .SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Throws(new Exception("The session service robot sprained its ankle. Try again soon."))
                .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            GetMock<ITaskDelay>().Verify(x => x.Delay(TimeSpan.FromSeconds(5)), Times.Once);
        }

        [Test]
        public async Task Init_UnexpectedRegistrationErrorsThenNextAttemptTheWorkerIsAlreadyOnline_ShouldNotStartPublishingLifecycleEvents()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Throws(new Exception("The session service robot fell over. Try again soon."))
                .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            GetMock<ILifecycleEventPublisher>().Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public async Task Init_UnexpectedRegistrationErrorsThenNextAttemptTheWorkerIsAlreadyOnline_ShouldNotBeReadyToStartReceivingProcesses()
        {
            GetMock<ISessionServiceClient>().SetupSequence(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Throws(new Exception("The session service robot's dog ate its homework. Try again soon."))
                .Returns(Task.FromResult(RegisteredStatus.AlreadyOnline));

            await RunInit();

            GetMock<IRunProcessQueueCoordinator>().Verify(x => x.BeginReceivingMessages(), Times.Never);
        }
                

        [Test]
        public async Task Init_BeginReceivingProcessesFails_ShouldDisplayError()
        {
            var expectedException = new Exception("Error Receiving Messages");
            GetMock<IRunProcessQueueCoordinator>()
                .Setup(x => x.BeginReceivingMessages())
                .ThrowsAsync(expectedException);

            await RunInit();

            AssertNotification(ResourceNotificationLevel.Error,
                $"Error connecting message consumer: {expectedException.Message}.");
        }

        [Test]
        public async Task ViewShutdownRequested_WhenStarted_ShouldStopBus()
        {
            GetMock<IResourcePCView>().Setup(x => x.BeginRunOnUIThread(It.IsAny<Action>())).
                Callback((Action action) => action?.Invoke());
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Returns(Task.FromResult(RegisteredStatus.Registered));
            await RunInit();
            var completionSource = new TaskCompletionSource<bool>();
            GetMock<IMessageBusWrapper>().Setup(x => x.Stop())
                .Callback(() => completionSource.SetResult(true));

            GetMock<IResourcePCView>().Raise(x => x.ShutdownRequested += null, EventArgs.Empty);
            await completionSource.Task.OrTimeout(1000);

            GetMock<IMessageBusWrapper>().Verify(x => x.Stop(), Times.Once);
            AssertNotification(ResourceNotificationLevel.Comment, $"Stopping message bus");
        }

        [Test]
        public async Task ViewShutdownRequested_WhenStarted_ShouldEndLifecycle()
        {
            GetMock<IResourcePCView>().Setup(x => x.BeginRunOnUIThread(It.IsAny<Action>())).
                Callback((Action action) => action?.Invoke());
            GetMock<ISessionServiceClient>().Setup(x => x.RegisterDigitalWorker(TestDigitalWorkerName.FullName))
                .Returns(Task.FromResult(RegisteredStatus.Registered));
            await RunInit();

            GetMock<IResourcePCView>().Raise(x => x.ShutdownRequested += null, EventArgs.Empty);

            GetMock<ILifecycleEventPublisher>().Verify(x => x.Stop(), Times.Once);
            AssertNotification(ResourceNotificationLevel.Comment, $"{_startUpOptions.Name.FullName} stopped");
        }

        private async Task RunInit()
        {
            var completionSource = new TaskCompletionSource<bool>();
            ClassUnderTest.Init(() => completionSource.SetResult(true));
            await completionSource.Task;
        }
        
        private void AssertNotification(ResourceNotificationLevel level, string text)
        {
            _notifications
                .Should()
                .Contain(x => x.Level == level && x.Text.Contains(text));
            GetMock<IResourcePCView>().Verify(x => x.DisplayNotification(
                It.Is<ResourceNotification>(match => match.Level == level && match.Text.Contains(text))));
        }
        
        // Used during testing to stub out hard-to-test startup logic in CoreStart
        public class TestDigitalWorker : DigitalWorker
        {
            public TestDigitalWorker(IMessageBusWrapper bus, 
                DigitalWorkerContext context, 
                IResourcePCView view, 
                ISessionServiceClient sessionServiceClient, 
                IRunProcessQueueCoordinator queueCoordinator, 
                INotificationHandler notificationHandler, 
                ILifecycleEventPublisher lifecycleEventPublisher,
                ITaskDelay taskDelay) 
                : base(bus, context, view, sessionServiceClient, queueCoordinator, notificationHandler, lifecycleEventPublisher, taskDelay)
            {
            }

            protected override bool CoreStart()
            {
                return true;
            }
        }
    }
}
