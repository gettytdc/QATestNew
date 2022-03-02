using System;
using BluePrism.AutomateAppCore.Resources;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Resources
{
    [TestFixture]
    public class ServerConnectedResourceMachineTests
    {
        private Guid _resourceId;
        private string _resourceName;

        [SetUp]
        public void SetupTestCase()
        {
            _resourceId = new Guid();
            _resourceName = "TestResource";
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsDisconnected_ThenErrorMessageReturned()
        {
            //Arrange
            var expectedErrorMessage =
                string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_AppServerCannotCommunicate, _resourceName);
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = false,
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsNotDisconnected_ThenErrorMessageNotReturned()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server,
                _resourceName,
                _resourceId,
                BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = true,
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsAlreadyDisconnected_ThenErrorMessageReturned()
        {
            //Arrange
            var expectedErrorMessage =
                string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_AppServerCannotCommunicate, _resourceName);

            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Disconnected, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = true,
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsPoolMember_ThenNotDisconnected()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = false,
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                IsInPool = true
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsLocalResource_ThenNotDisconnected()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = false,
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                Attributes = BluePrism.Core.Resources.ResourceAttribute.DefaultInstance
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsSleepAndDBReady_ThenReturnTrue()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Sleep,
                _resourceName,
                _resourceId,
                BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsSleepAndDBNotReady_ThenDefaultErrorMessageReturned()
        {
            //Arrange
            var dbStatus = ResourceMachine.ResourceDBStatus.Offline;
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, _resourceName, ResourceMachine.GetDBStatusFriendlyName(dbStatus));
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Sleep, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = dbStatus,
                SuccessfullyConnectedToAppServer = true
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsServerAndDisplayStatusNotMissingAndDBReady_ThenReturnTrue()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = ResourceMachine.ResourceDBStatus.Ready,
                SuccessfullyConnectedToAppServer = true
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus("testname", ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsServerAndDisplayStatusNotMissingAndDBNotReady_ThenDefaultErrorMessageReturned()
        {
            //Arrange
            var dbStatus = ResourceMachine.ResourceDBStatus.Offline;
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, _resourceName, ResourceMachine.GetDBStatusFriendlyName(dbStatus));
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = dbStatus,
                SuccessfullyConnectedToAppServer = true
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsConnectedAndDBNotReady_ThenNotAvailableMessageReturned()
        {
            //Arrange
            const ResourceMachine.ResourceDBStatus dbStatus = ResourceMachine.ResourceDBStatus.Offline;
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, _resourceName, ResourceMachine.GetDBStatusFriendlyName(dbStatus));
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Connected, "TestResource", _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = dbStatus
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenPoolAndConnectionStateIsOfflineAndDBNotReady_ThenErrorMessageReturned()
        {
            //Arrange
            const ResourceMachine.ResourceDBStatus dbStatus = ResourceMachine.ResourceDBStatus.Ready;
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Server, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Offline,
                DBStatus = dbStatus,
                Attributes = BluePrism.Core.Resources.ResourceAttribute.Pool
            };
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, _resourceName, ResourceInfo.GetResourceStatusFriendlyName(resourceMachine.DisplayStatus));

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenProvidedConnectionStateIsConnectedAndDBReady_ThenReturnTrue()
        {
            //Arrange
            var resourceMachine = new ServerConnectedResourceMachine(ResourceConnectionState.Connected, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DBStatus = ResourceMachine.ResourceDBStatus.Ready
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus("testname", ref errorMessage);

            //Assert
            result.Should().Be(true);
            errorMessage.Should().Be(string.Empty);
        }
    }
}
