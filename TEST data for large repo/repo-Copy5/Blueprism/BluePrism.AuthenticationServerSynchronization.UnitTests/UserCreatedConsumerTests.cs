using System;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore;
using BluePrism.AuthenticationServerSynchronization.Consumers;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using BPC.IMS.Messages.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BluePrism.AuthenticationServerSynchronization.UnitTests
{
    [TestFixture]
    public class UserCreatedConsumerTests : ConsumerTestBase<UserCreatedConsumer>
    {
        private static readonly Guid TestAuthenticationServerUserId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");

        protected override UserCreatedConsumer TestClassConstructor()
        {
            return new UserCreatedConsumer(GetMock<IServer>().Object);
        }

        public override void Setup()
        {
            base.Setup();
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(true);
        }


        [Test]
        public async Task Consume_UserIsCreated_MessageConsumedWithNoExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.CreateNewAuthenticationServerUserWithUniqueName(It.IsAny<string>(), It.IsAny<Guid>())).Returns("Doris");
            await Bus.InputQueueSendEndpoint.Send<UserCreated>(new
            {
                UserName = "Doris",
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<UserCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_UserExistsWithSameAuthServerUserId_ExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewAuthenticationServerUserWithUniqueName(It.IsAny<string>(), It.IsAny<Guid>())).Throws<AuthenticationServerUserIdAlreadyInUseException>();
            await Bus.InputQueueSendEndpoint.Send<UserCreated>(new
            {
                UserName = "Doris",
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            Bus.Consumed.Select<UserCreated>().Should().HaveCount(1);
            Bus.Consumed.Select<UserCreated>().First().Exception.Should().BeOfType<AuthenticationServerUserIdAlreadyInUseException>();
        }

        [Test]
        public async Task Consume_UsernameIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewAuthenticationServerUserWithUniqueName(It.IsAny<string>(), It.IsAny<Guid>())).Returns("");
            await Bus.InputQueueSendEndpoint.Send<UserCreated>(new
            {
                UserName = "",
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            Bus.Consumed.Select<UserCreated>().Should().HaveCount(1);
            Bus.Consumed.Select<UserCreated>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_AuthServerIntegrationDisabled_ShouldNotCreateUser()
        {
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(false);

            GetMock<IServer>().Setup(x => x.CreateNewAuthenticationServerUserWithUniqueName(It.IsAny<string>(), It.IsAny<Guid>())).Returns("");

            await Bus.InputQueueSendEndpoint.Send<UserCreated>(new
            {
                UserName = "Doris",
                Id = TestAuthenticationServerUserId,
                Date = DateTimeOffset.Now
            });

            var consumed = Bus.Consumed.Select<UserCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.CreateNewAuthenticationServerUserWithUniqueName(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}
