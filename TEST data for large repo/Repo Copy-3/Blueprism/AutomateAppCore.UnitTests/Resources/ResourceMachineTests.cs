using System;
using BluePrism.AutomateAppCore.Resources;
using NUnit.Framework;
using FluentAssertions;

namespace AutomateAppCore.UnitTests.Resources
{
    [TestFixture]
    public class ResourceMachineTests
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
        public void CheckResourcePCStatus_GivenConnectionStateIsNotConnected_ThenNotConnectedMessageReturned()
        {
            //Arrange
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_Resource0IsNotConnected, _resourceName);
            var resourceMachine = new ResourceMachine(ResourceConnectionState.Error, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = ResourceMachine.ResourceDBStatus.Offline
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

        [Test]
        public void CheckResourcePCStatus_GivenConnectionStateIsConnected_ThenTrueReturned()
        {
            //Arrange
            var resourceMachine = new ResourceMachine(ResourceConnectionState.Connected, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
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
        public void CheckResourcePCStatus_GivenConnectionStateIsConnectedAndDbStatusNotReady_ThenErrorReturned()
        {
            //Arrange
            var dbStatus = ResourceMachine.ResourceDBStatus.Offline;
            var expectedErrorMessage = string.Format(BluePrism.AutomateAppCore.My.Resources.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, _resourceName, ResourceMachine.GetDBStatusFriendlyName(dbStatus));
            var resourceMachine = new ResourceMachine(ResourceConnectionState.Connected, _resourceName, _resourceId, BluePrism.Core.Resources.ResourceAttribute.None)
            {
                DisplayStatus = BluePrism.AutomateAppCore.ResourceStatus.Idle,
                DBStatus = ResourceMachine.ResourceDBStatus.Offline
            };

            var errorMessage = string.Empty;

            //Act
            var result = resourceMachine.CheckResourcePCStatus(_resourceName, ref errorMessage);

            //Assert
            result.Should().Be(false);
            errorMessage.Should().Be(expectedErrorMessage);
        }

    }
}
