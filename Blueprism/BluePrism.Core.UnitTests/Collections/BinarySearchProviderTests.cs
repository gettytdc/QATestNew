#if UNITTESTS

namespace BluePrism.Core.UnitTests.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Core.Collections;

    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    public class BinarySearchProviderTests : UnitTestBase<BinarySearchProvider>
    {
        [Test]
        [TestCaseSource("Cases1")]
        public void IndexOfWithDefaultComparerReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.IndexOf(collection, value));

        [Test]
        [TestCaseSource("Cases1")]
        public void IndexOfWithDefinedComparerReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.IndexOf(collection, value, new IntegerComparer()));

        [Test]
        [TestCaseSource("Cases1")]
        public void IndexOfWithOldListReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.IndexOf((IList)collection, value, new IntegerComparer()));

        protected static object[] Cases1 =
        {
            new object[] {new int[] {1, 2, 3, 4, 5}, 4, 3},
            new object[] {new int[] {1, 2, 3, 4, 5}, 1, 0},
            new object[] {new int[] {1, 2, 3, 4, 5}, 5, 4},
            new object[] {new int[] {1, 2, 3, 4, 5}, 6, -1},
            new object[] {new int[] { }, 3, -1}
        };

        [Test]
        [TestCaseSource("Cases2")]
        public void InsertIndexOfWithDefaultComparerReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.InsertIndexOf(collection, value));

        [Test]
        [TestCaseSource("Cases2")]
        public void InsertIndexOfWithDefinedComparerReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.InsertIndexOf(collection, value, new IntegerComparer()));

        [Test]
        [TestCaseSource("Cases2")]
        public void InsertIndexOfWithOldListReturnsCorrectIndex(int[] collection, int value, int expectedValue) =>
            Assert.AreEqual(expectedValue, ClassUnderTest.InsertIndexOf((IList)collection, value, new IntegerComparer()));

        protected static object[] Cases2 =
        {
            new object[]{new[] { 1, 3, 5, 7, 9 }, 4,  2},
            new object[]{new[] { 1, 3, 5, 7, 9 }, 0,  0},
            new object[]{new[] { 1, 3, 5, 7, 9 }, 15, 5},
            new object[]{new[] { 1, 3, 5, 7, 9 }, 3,  1}
        };

        private class IntegerComparer : IComparer<int>, IComparer
        {
            public int Compare(int x, int y) => x - y;
            public int Compare(object x, object y) =>
                (x is int && y is int)
                ? Compare((int)x, (int)y)
                : throw new ArgumentException();
        }
    }
}
#endif