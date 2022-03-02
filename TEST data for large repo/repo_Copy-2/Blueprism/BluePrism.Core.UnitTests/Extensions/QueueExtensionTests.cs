using System.Collections.Generic;
using BluePrism.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Extensions
{
    public class QueueExtensionTests
    {
        [Test]
        public void GivenQueueIsEmpty_ThenNullIsReturned()
        {
            //Arrange
            var queue = new Queue<string>();

            //Act
            var queueItem = queue.DequeueOrDefault();

            //Assert
            queueItem.Should().BeNull();
        }

        [Test]
        public void GivenQueueIsPopulated_TenFirstItemIsRemovedAndReturnedFromQueue()
        {
            //Arrange
            var queue = new Queue<string>();
            var expectedItem = "item 1";
            queue.Enqueue(expectedItem);
            queue.Enqueue("item 2");

            //Act
            var deQueuedItem = queue.DequeueOrDefault();

            //Assert
            deQueuedItem.Should().Be(expectedItem);
        }
    }
}
