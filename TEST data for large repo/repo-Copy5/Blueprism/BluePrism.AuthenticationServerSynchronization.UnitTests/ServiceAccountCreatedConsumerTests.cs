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
    public class ServiceAccountCreatedConsumerTests : ConsumerTestBase<ServiceAccountCreatedConsumer>
    {
        protected override ServiceAccountCreatedConsumer TestClassConstructor()
        {
            return new ServiceAccountCreatedConsumer(GetMock<IServer>().Object);
        }

        public override void Setup()
        {
            base.Setup();
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(true);
        }

        [Test]
        public async Task Consume_ServiceAccountIsCreated_MessageConsumedWithNoExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("client1");
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_ServiceAccountExistsWithSameAuthServerClientId_ExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<AuthenticationServerClientIdAlreadyInUseException>();
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountCreated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountCreated>().First().Exception.Should().BeOfType<AuthenticationServerClientIdAlreadyInUseException>();
        }

        [Test]
        public async Task Consume_NameIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("");
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountCreated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountCreated>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_IdIsEmpty_ArgumentExceptionThrown()
        {
            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("");
            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            Bus.Consumed.Select<ServiceAccountCreated>().Should().HaveCount(1);
            Bus.Consumed.Select<ServiceAccountCreated>().First().Exception.Should().BeOfType<ArgumentException>();
        }

        [Test]
        public async Task Consume_AuthServerIntegrationDisabled_ShouldNotCreateServiceAccount()
        {
            GetMock<IServer>().Setup(x => x.IsAuthenticationServerIntegrationEnabled()).Returns(false);

            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("");

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task Consume_MessageContainsBluePrismApiPermission_ShouldCreateServiceAccountWithHasScopeSetToTrue()
        {

            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("");

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>() { ServiceAccountPermission.BluePrismApi }
            });

            var consumed = Bus.Consumed.Select<ServiceAccountCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(s => s == true)), Times.Once);
        }

        [Test]
        public async Task Consume_MessageDoesNotContainBluePrismApiPermission_ShouldCreateServiceAccountWithHasScopeSetToFalse()
        {

            GetMock<IServer>().Setup(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("");

            await Bus.InputQueueSendEndpoint.Send<ServiceAccountCreated>(new
            {
                Name = "client1",
                Id = "client1Id",
                Date = DateTimeOffset.Now,
                AllowedPermissions = new List<ServiceAccountPermission>()
            });

            var consumed = Bus.Consumed.Select<ServiceAccountCreated>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
            GetMock<IServer>().Verify(x => x.CreateNewServiceAccount(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(s => s == false)), Times.Once);
        }
    }
}
