using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests.Collections
{
    public class CollectionUtilTests
    {
        [Test]
        public void GivenCollectionWithSingleElementsProvided_ThenFunctionReturnsFirstElement()
        {
            // Arrange
            var collection = new List<int>() { 1 };
            // Act
            var testCollection = CollectionUtil.First(collection);
            // Assert
            testCollection.Should().Be(1);
        }

        [Test]
        public void GivenCollectionWithMultipleElementsProvided_ThenFunctionReturnsFirstElement()
        {
            // Arrange
            var collection = new List<int>() { 1, 2, 3, 4 };
            // Act
            var testCollection = CollectionUtil.First(collection);
            // Assert
            testCollection.Should().Be(1);
        }

        [Test]
        public void GivenEmptyCollectionProvided_ThenFunctionReturnsNothing()
        {
            // Arrange
            var collection = new List<int>();
            // Act
            var testCollection = CollectionUtil.First(collection);
            // Assert
            testCollection.Should().Be(default);
        }

        [Test]
        public void GivenCollectionThatIsNotIntialisedHasBeenProvided_ThenFunctionReturnsNothing()
        {
            // Arrange
            var nullCollection = default(List<int>);
            // Act
#pragma warning disable BC42104
            var testCollection = CollectionUtil.First(nullCollection);
#pragma warning restore BC42104
            // Assert
            testCollection.Should().Be(default);
        }
    }
}
