using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrism.AuthenticationServerSynchronization.Consumers;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using BPC.IMS.Messages.Events;
using FluentAssertions;
using ImsServer.Core.Enums;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace BluePrism.AuthenticationServerSynchronization.UnitTests
{
    [TestFixture]
    public class ServiceAccountDeletedConsumerUnitTests : ConsumerTestBase<ServiceAccountDeletedConsumer>
    {
        private const string TestAuthenticationServerClientId = "clientId1";

        protected override ServiceAccountDeletedConsumer TestClassConstructor()
        {
            return new ServiceAccountDeletedConsumer(GetMock<IServer>().Object);
        }

        public override void Setup()
        {
            base.Setup();
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(true);
        }

        [Test]
        public async Task Consume_ServiceAccountIsDeleted_MessageConsumedWithNoExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()));
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountDeleted>(new
            {
                Id = TestAuthenticationServerClientId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<ServiceAccountDeleted>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_IdIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()));
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountDeleted>(new
            {
                Id = string.Empty,
                Date = DateTimeOffset.Now
            });

            Bus.Consumed.Select<ServiceAccountDeleted>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountDeleted>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_AuthServerIntegrationDisabled_ShouldNotDeleteServiceAccount()
        {
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(false);

            GetMock<IServer>().Setup(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountDeleted>(new
            {
                Id = TestAuthenticationServerClientId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<ServiceAccountDeleted>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()), Times.Never);
        }

        [Test]
        public async Task Consume_AuthenticationServerUserNotFound_ExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Throws<AuthenticationServerUserNotFoundException>();
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountDeleted>(new
            {
                Id = TestAuthenticationServerClientId,
                Date = DateTimeOffset.Now
            });

            Bus.Consumed.Select<ServiceAccountDeleted>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountDeleted>().First().Exception.Should().BeOfType<AuthenticationServerUserNotFoundException>();
        }

        [Test]
        public async Task Consume_DeletingClientThrowsException_ShouldRethrowException()
        {
            var testException = new Exception("Error Deleting Service Account");

            GetMock<IServer>()
                .Setup(x => x.DeleteServiceAccount(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Throws(testException);

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountDeleted>(new
            {
                Id = TestAuthenticationServerClientId,
                Date = DateTimeOffset.Now
            });

            var faultedServiceAccountDeletedMessage = Bus.Published.Select<Fault<ServiceAccountDeleted>>();
            var exception = faultedServiceAccountDeletedMessage.First().Context.Message.Exceptions.First();

            exception.Message.Should().Be(testException.Message);
            exception.ExceptionType.Should().Be(testException.GetType().ToString());
        }
    }
}
