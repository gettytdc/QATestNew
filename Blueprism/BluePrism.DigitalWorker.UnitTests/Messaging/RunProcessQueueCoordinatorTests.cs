using BluePrism.DigitalWorker.Messaging;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using MassTransit;
using NUnit.Framework;
using System;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.DigitalWorker.Messaging.Observers;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    [TestFixture]
    public class RunProcessQueueCoordinatorTests : UnitTestBase<RunProcessQueueCoordinator>
    {
        [Test]
        public void Initialise_NoBusProvided_ErrorThrown()
        {
            Action initialise = () => new RunProcessQueueCoordinator(
                                        new DigitalWorkerContext(new DigitalWorkerStartUpOptions()),
                                        null,
                                        () => new RunProcessConsumer(null, null, null, null),
                                        GetMock<IExclusiveProcessLockObserver>().Object,
                                        () => new StopProcessConsumer(null),
                                        () => new RequestStopProcessConsumer(null),
                                        () => new GetSessionVariablesConsumer(null));

            initialise.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Initialise_NoConsumerFactoryProvided_ErrorThrown()
        {
            Action initialise = () => new RunProcessQueueCoordinator(
                                        new DigitalWorkerContext(new DigitalWorkerStartUpOptions()),
                                        GetMock<IBusControl>().Object,
                                        null,
                                        GetMock<IExclusiveProcessLockObserver>().Object,
                                        () => new StopProcessConsumer(null),
                                        () => new RequestStopProcessConsumer(null),
                                        () => new GetSessionVariablesConsumer(null));

            initialise.ShouldThrow<ArgumentNullException>();
        }
    }
}
