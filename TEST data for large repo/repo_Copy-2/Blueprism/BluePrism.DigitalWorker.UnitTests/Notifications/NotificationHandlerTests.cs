using BluePrism.AutomateAppCore.Resources;
using BluePrism.DigitalWorker.Notifications;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace BluePrism.DigitalWorker.UnitTests
{
    [TestFixture]
    public class NotificationHandlerTests : UnitTestBase<NotificationHandler>
    {
        [Test]
        public void HandleNotification_RaisesEventCorrectly()
        {
            ResourceNotification raisedNotification = null;
            ClassUnderTest.Notify += (s, e) => raisedNotification = e.Notification;

            var notification = new ResourceNotification(ResourceNotificationLevel.Comment, "a notification", DateTime.Now);
            ClassUnderTest.HandleNotification(notification);

            raisedNotification.Should().Be(notification);
        }
    }
}
