using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands.Factory;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    public class SessionServiceClientTests : UnitTestBase<SessionServiceClient>
    {
        private const string TestWorkerName = "Worker1";
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");

        private void SetupResponse<TResponse, TRequest>(TResponse message, Action<TRequest> requestCallback = null)
            where TResponse : class where TRequest : class
        {
            var response = TestResponse.Create(message);
            var clientMock = GetMock<IRequestClient<TRequest>>();
            clientMock.Setup(x => x.GetResponse<TResponse>(It.IsAny<TRequest>(),
                    It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Callback((TRequest request, CancellationToken token, RequestTimeout timeout) => requestCallback?.Invoke(request))
                .ReturnsAsync(response);
        }

        [Test]
        public async Task Register_ShouldRegisterWithName()
        {
            var response = SessionServiceCommands.RegisterDigitalWorkerResponse(RegisteredStatus.Registered);
            RegisterDigitalWorker requestMessage = null;
            SetupResponse<RegisterDigitalWorkerResponse, RegisterDigitalWorker>(response, x => requestMessage = x);

            await ClassUnderTest.RegisterDigitalWorker(TestWorkerName);

            requestMessage.Should().NotBeNull();
            requestMessage.Name.Should().Be(TestWorkerName);
        }

        [TestCase(RegisteredStatus.AlreadyOnline)]
        [TestCase(RegisteredStatus.Registered)]
        public async Task Register_ShouldReturnStatusFromResponse(RegisteredStatus testStatus)
        {
            var response = SessionServiceCommands.RegisterDigitalWorkerResponse(testStatus);
            SetupResponse<RegisterDigitalWorkerResponse, RegisterDigitalWorker>(response);

            var status = await ClassUnderTest.RegisterDigitalWorker(TestWorkerName);

            status.Should().Be(testStatus);
        }

        [Test]
        public async Task RequestStartProcess_ShouldSendExpectedMessage()
        {
            var response = SessionServiceCommands.RequestStartProcessResponse(StartProcessStatus.ReadyToStart);
            RequestStartProcess requestMessage = null;
            SetupResponse<RequestStartProcessResponse, RequestStartProcess>(response, x => requestMessage = x);

            await ClassUnderTest.RequestStartProcess(TestSessionId, TestWorkerName);

            var expectedRequestMessage = SessionServiceCommands.RequestStartProcess(TestSessionId, TestWorkerName);
            requestMessage.ShouldBeEquivalentTo(expectedRequestMessage);
        }

        [TestCase(StartProcessStatus.ReadyToStart)]
        [TestCase(StartProcessStatus.AlreadyStarted)]
        public async Task RequestStartProcess_ShouldReturnStatusFromResponse(StartProcessStatus testStatus)
        {
            var response = SessionServiceCommands.RequestStartProcessResponse(testStatus);
            SetupResponse<RequestStartProcessResponse, RequestStartProcess>(response);

            var status = await ClassUnderTest.RequestStartProcess(TestSessionId, TestWorkerName);

            status.Should().Be(testStatus);
        }
    }
}
