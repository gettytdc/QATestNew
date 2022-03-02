using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.AuthenticationServerSynchronization.Consumers;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using BPC.IMS.Messages.Events;
using FluentAssertions;
using ImsServer.Core.Enums;
using Moq;
using NUnit.Framework;

namespace BluePrism.AuthenticationServerSynchronization.UnitTests
{
    [TestFixture]
    public class ServiceAccountUpdatedConsumerTests : ConsumerTestBase<ServiceAccountUpdatedConsumer>
    {
        protected override ServiceAccountUpdatedConsumer TestClassConstructor()
        {
            return new ServiceAccountUpdatedConsumer(GetMock<IServer>().Object);
        }

        public override void Setup()
        {
            base.Setup();
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(true);
        }

        [Test]
        public async Task Consume_ServiceAccountIsUpdated_MessageConsumedWithNoExceptionThrown()
        {
            
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            var name = "client1";
            var id = "clientId";
            var date = DateTimeOffset.Now;
            var allowedPermissions = new List<ServiceAccountPermission>();

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = name,
                Id = id,
                Date = date,
                AllowedPermissions = allowedPermissions
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.UpdateServiceAccount(
                It.Is<string>(y => y == id),
                It.Is<string>(y => y == name),
                It.Is<bool>(y => y == false),
                It.Is<DateTimeOffset>(y => y == date)),
            Times.Once);
        }

        [Test]
        public async Task Consume_NameIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountUpdated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountUpdated>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_IdIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountUpdated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountUpdated>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_AuthServerIntegrationDisabled_ShouldNotUpdateServiceAccount()
        {
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(false);

            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()), Times.Never);
        }

        [Test]
        public async Task Consume_MessageContainsBluePrismApiPermission_ShouldUpdateServiceAccountWithHasScopeSetToTrue()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>() { ServiceAccountPermission.BluePrismApi }
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(s => s == true), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Test]
        public async Task Consume_MessageDoesNotContainBluePrismApiPermission_ShouldUpdateServiceAccountWithHasScopeSetToFalse()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(s => s == false), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Test]
        public async Task Consume_MessageContainsUpdatedName_ShouldUpdateServiceAccountWithNewName()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()));

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.UpdateServiceAccount(It.IsAny<string>(), It.Is<string>(y => y == "client1"), It.IsAny<bool>(), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Test]
        public async Task Consume_UpdateServiceAccountThrowsSynchronizationOutOfSequenceException_ShouldConsumeMessageSuccessfully()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>())).Throws<SynchronizationOutOfSequenceException>();

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountUpdated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();       
        }

        [Test]
        public async Task Consume_UpdateServiceAccountThrowsAuthenticationServerUserNotFoundException_AuthenticationServerUserNotFoundExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.UpdateServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTimeOffset>())).Throws<AuthenticationServerUserNotFoundException>();

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountUpdated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountUpdated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountUpdated>().First().Exception.Should().BeOfType<AuthenticationServerUserNotFoundException>();
        }
    }
}
