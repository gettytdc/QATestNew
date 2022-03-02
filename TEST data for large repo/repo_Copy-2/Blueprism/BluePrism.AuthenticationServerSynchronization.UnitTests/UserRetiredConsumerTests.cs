using System;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.AuthenticationServerSynchronization.Consumers;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using BPC.IMS.Messages.Events;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace BluePrism.AuthenticationServerSynchronization.UnitTests
{
    [TestFixture]
    public class UserRetiredConsumerTests : ConsumerTestBase<UserRetiredConsumer>
    {
        private static readonly Guid TestAuthenticationServerUserId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");

        protected override UserRetiredConsumer TestClassConstructor()
        {
            return new UserRetiredConsumer(GetMock<IServer>().Object);
        }

        public override void Setup()
        {
            base.Setup();
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(true);
        }

        [Test]
        public async Task Consume_UserIsRetired_MessageConsumedWithNoExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<UserRetired>(new
            {
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<UserRetired>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_AuthServerIntegrationDisabled_ShouldNotRetireUser()
        {
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(false);
            GetMock<IServer>().Setup(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<UserRetired>(new
            {
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<UserRetired>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
        }

        [Test]
        public async Task Consume_MessageArrivesOutOfSequence_DoesNotThrowException()
        {
            GetMock<IServer>().Setup(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>())).Throws<SynchronizationOutOfSequenceException>();
            await Bus.InputQueueSendEndpoint.Send<UserRetired>(new
            {
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<UserRetired>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_AuthenticationServerUserNotFound_ExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>())).Throws<AuthenticationServerUserNotFoundException>();
            await Bus.InputQueueSendEndpoint.Send<UserRetired>(new
            {
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            Bus.Consumed.Select<UserRetired>().Should().HaveCount(1);
            Bus.Consumed.Select<UserRetired>().First().Exception.Should().BeOfType<AuthenticationServerUserNotFoundException>();
        }

        [Test]
        public async Task Consume_RetiringUserThrowsException_ShouldRethrowException()
        {
            var testException = new Exception("A bad exception has happened");

            GetMock<IServer>()
                .Setup(x => x.RetireAuthenticationServerUser(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>())).Throws(testException);

            await Bus.InputQueueSendEndpoint.Send<UserRetired>(new
            {
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var faultedUserRetiredMessage = Bus.Published.Select<Fault<UserRetired>>();
            var exception = faultedUserRetiredMessage.First().Context.Message.Exceptions.First();

            exception.Message.Should().Be(testException.Message);
            exception.ExceptionType.Should().Be(testException.GetType().ToString());
        }
    }
}
