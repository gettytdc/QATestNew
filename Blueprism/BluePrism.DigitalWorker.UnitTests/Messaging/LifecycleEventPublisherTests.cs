using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messages.Events;
using BluePrism.DigitalWorker.Messages.Events.Factory;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    using Autofac;

    public class LifecycleEventPublisherTests : UnitTestBase<LifecycleEventPublisher>
    {
        private static readonly DigitalWorkerName TestDigitalWorkerName = new DigitalWorkerName("Worker1");
        private readonly DigitalWorkerContext TestContext = new DigitalWorkerContext(new DigitalWorkerStartUpOptions { Name = TestDigitalWorkerName });
        private DateTimeOffset Now {get;} = DateTimeOffset.Now;
        
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterInstance(TestContext);
            });

            GetMock<ISystemClock>().Setup(x => x.Now).Returns(Now);
        }

        private void SetupPublish<T>(Action<T> action = null) where T : class
        {
            GetMock<IMessageBusWrapper>().Setup(x => x.Publish(It.IsAny<T>()))
                .Callback((T x) => action?.Invoke(x))
                .Returns(Task.CompletedTask);
        }

        private void SetupPublishWithPublishContext<T>(Action<T> action = null) where T : class
        {
            GetMock<IMessageBusWrapper>().Setup(x => x.Publish(It.IsAny<T>(), It.IsAny<Action<PublishContext<T>>>()))
                .Callback((T x, Action<PublishContext<T>> s) => action?.Invoke(x))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Start_ShouldPublishMessage()
        {
            DigitalWorkerStarted publishedMessage = null;
            SetupPublish<DigitalWorkerStarted>(x => publishedMessage = x);
                
            await ClassUnderTest.Start();

            var expectedMessage = DigitalWorkerEvents.DigitalWorkerStarted(TestDigitalWorkerName.FullName, Now);
            publishedMessage.ShouldBeEquivalentTo(expectedMessage);
        }

        
        [Test]
        public async Task Start_ShouldPublishHeartbeatMessages()
        {
            Action timerCallback = null;
            GetMock<ISystemTimer>().Setup(x =>
                    x.Start(It.IsAny<Action>(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60)))
                .Callback((Action action, TimeSpan _, TimeSpan __) => timerCallback = action);
            var heartbeatMessages = new List<DigitalWorkerHeartbeat>();
            SetupPublish<DigitalWorkerStarted>();
            SetupPublishWithPublishContext<DigitalWorkerHeartbeat>(x => heartbeatMessages.Add(x));
            await ClassUnderTest.Start();

            var firstHeartbeatDate = Now;
            var secondHeartbeatDate = Now.AddMinutes(1);
            timerCallback.Invoke();
            GetMock<ISystemClock>().Setup(x => x.Now).Returns(secondHeartbeatDate);
            timerCallback.Invoke();

            var expectedMessages = new[]
            {
                DigitalWorkerEvents.DigitalWorkerHeartbeat(TestDigitalWorkerName.FullName, firstHeartbeatDate),
                DigitalWorkerEvents.DigitalWorkerHeartbeat(TestDigitalWorkerName.FullName, secondHeartbeatDate)
            };
            heartbeatMessages.ShouldAllBeEquivalentTo(expectedMessages);
        }
        
        [Test]
        public async Task Stop_ShouldPublishMessage()
        {  
            DigitalWorkerStopped publishedMessage = null;
            SetupPublish<DigitalWorkerStarted>();
            SetupPublish<DigitalWorkerStopped>(x => publishedMessage = x); 
            await ClassUnderTest.Start();
          
            await ClassUnderTest.Stop();

            var expectedMessage = DigitalWorkerEvents.DigitalWorkerStopped(TestDigitalWorkerName.FullName, Now);
            publishedMessage.ShouldBeEquivalentTo(expectedMessage);
        }

        [Test]
        public async Task Stop_ShouldStopHeartbeats()
        {  
            SetupPublish<DigitalWorkerStarted>();
            SetupPublish<DigitalWorkerStopped>(); 
            await ClassUnderTest.Start();
          
            await ClassUnderTest.Stop();

            GetMock<ISystemTimer>().Verify(x => x.Stop());
        }
    }
}
