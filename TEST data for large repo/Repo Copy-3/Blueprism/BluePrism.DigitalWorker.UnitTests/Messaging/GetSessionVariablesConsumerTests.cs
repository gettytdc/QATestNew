using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Messages.Commands;
using BluePrism.DigitalWorker.Messages.Commands.Factory;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Sessions;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    using Autofac;
    using BluePrism.UnitTesting.TestSupport.MassTransit;

    [TestFixture]
    public class GetSessionVariablesConsumerTests : ConsumerTestBase<GetSessionVariablesConsumer>
    {
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private static readonly GetSessionVariables RequestWithSessionId = DigitalWorkerCommands.GetSessionVariables(TestSessionId);

        public override void Setup()
        {
            var runningSessionRegistry = new RunningSessionRegistry();

            base.Setup(builder =>
            {
                builder.RegisterInstance<IRunningSessionRegistry>(runningSessionRegistry);
            });

            var runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();
            runnerRecordMock
                .Setup(m => m.GetSessionVariables())
                .Returns(new Dictionary<string, clsProcessValue>
                {
                    { "A", new clsProcessValue(1) },
                    { "B", new clsProcessValue("test") }
                });

            runningSessionRegistry.Register(TestSessionId, runnerRecordMock.Object);
        }

        [Test]
        public async Task Consume_GetAllSessionVariables_ReturnsGetSessionVariablesResponse()
        {
            var requestClient = Bus.CreateRequestClient<GetSessionVariables>();

            var response = await requestClient.GetResponse<GetSessionVariablesResponse>(RequestWithSessionId);

            var expectedResponse = DigitalWorkerCommands.GetSessionVariablesResponse(true, new[]
            {
                new SessionVariable("A", null, new ProcessValue("1", TryParseDataType(DataType.number.ToString()))),
                new SessionVariable("B", null, new ProcessValue("test", TryParseDataType(DataType.text.ToString())))
            });

            response.Message.ShouldBeEquivalentTo(expectedResponse);
        }
        private static ProcessValueType TryParseDataType(string type)
        {
            Enum.TryParse<ProcessValueType>(type, true, out var dataType);

            return dataType;
        }
    }
}
