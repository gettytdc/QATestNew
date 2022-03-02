#if UNITTESTS

using System;
using System.Collections;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{

    /// Project  : BPCoreLib
    /// Class    : TestWindowEnumerators
    /// <summary>
    /// Test class for the clsWindowEnumerable and its sibling, the
    /// clsWindowEnumerator.
    /// </summary>
    [TestFixture]
    public class TestWindowEnumerators
    {
        /// <summary>
        /// Class to emit a null enumerator - used to test failure condition of
        /// an enumerable, er, emitting a null enumerator
        /// </summary>
        /// <typeparam name="T">The type of the null enumerator emitted(?!)
        /// </typeparam>
        private class NullEnumeratorEmitter<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator() => null;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Test that a null enumerable throws the appropriate exception
        /// </summary>
        [Test]
        public void TestNullEnumerable() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(null, -1, 0);
        }, Throws.InstanceOf<ArgumentNullException>());

        /// <summary>
        /// Tests that an enumerable which emits a null enumerator throws the
        /// appropriate exception when GetEnumerator() is called
        /// </summary>
        [Test]
        public void TestNullEnumerator() => Assert.That(() =>
        {
            var w = new clsWindowedEnumerable<int>(new NullEnumeratorEmitter<int>(), 1, 1);
            w.GetEnumerator();
        }, Throws.InstanceOf<ArgumentNullException>());

        /// <summary>
        /// Tests that a negative start index throws an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestNegativeStartIndex() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(new int[] { }, -1, 0);
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());

        /// <summary>
        /// Tests that a negative max number throws an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestNegativeMaxNumber() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(new int[] { }, 0, -1);
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());

        /// <summary>
        /// Tests that a start index of Integer.MaxValue and a non-zero maxNumber
        /// throws an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestMaxValuedStartIndex() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(new int[] { }, int.MaxValue, 1);
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());

        /// <summary>
        /// Tests that a max number of Integer.MaxValue and a non-zero start
        /// index throws an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestMaxValuedMaxNumber() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(new int[] { }, 1, int.MaxValue);
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());

        /// <summary>
        /// Tests that when both start index and max number are Integer.MaxValue,
        /// constructor throws an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestMaxValuedBothArguments() => Assert.That(() =>
        {
            _ = new clsWindowedEnumerable<int>(new int[] { }, int.MaxValue, int.MaxValue);
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());

        /// <summary>
        /// Tests the Integer.MaxValue with zero other arguments doesn't
        /// throw an ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void TestMaxValueWithZeroArguments()
        {
            try
            {
                _ = new clsWindowedEnumerable<int>(new int[] { }, int.MaxValue, 0);
            }
            catch (Exception ex)
            {
                Assert.Fail("clsWindowedEnumerable(arr,Integer.MaxValue,0) threw " + ex);
            }

            try
            {
                _ = new clsWindowedEnumerable<int>(new int[] { }, 0, int.MaxValue);
            }
            catch (Exception ex)
            {
                Assert.Fail("clsWindowedEnumerable(arr,0, Integer.MaxValue) threw " + ex);
            }
        }

        /// <summary>
        /// Tests that an enumerable with no elements is handled correctly
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void TestEmptyCollection()
        {
            foreach (var _ in new clsWindowedEnumerable<int>(new List<int>(), 0, 0))
            {
                Assert.Fail("ForEach (empty,0,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(new List<int>(), 0, 1))
            {
                Assert.Fail("ForEach (empty,0,1) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(new List<int>(), 1, 0))
            {
                Assert.Fail("ForEach (empty,1,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(new List<int>(), 1, 1))
            {
                Assert.Fail("ForEach (empty,1,1) should not enter the loop");
            }
        }

        /// <summary>
        /// Tests that enumerables with a single element are handled correctly
        /// </summary>
        [Test]
        public void TestSingleElementCollection()
        {
            var one = new [] { 5 };
            foreach (var _ in new clsWindowedEnumerable<int>(one, 0, 0))
            {
                Assert.Fail("ForEach (one,0,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(one, 1, 0))
            {
                Assert.Fail("ForEach (one,1,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(one, 1, 1))
            {
                Assert.Fail("ForEach (one,1,1) should not enter the loop");
            }

            var count = 0;
            foreach (var i in new clsWindowedEnumerable<int>(one, 0, 1))
            {
                count += 1;
                Assert.That(count, Is.EqualTo(1), "ForEach(one,0,1) should loop only once - found count: " + count);
                Assert.That(i, Is.EqualTo(5), "ForEach(one,0,1) should return element: 5");
            }

            Assert.That(count, Is.EqualTo(1), "ForEach(one,0,1) found no elements");
        }

        /// <summary>
        /// Tests that enumerables with multiple elements are handled correctly
        /// </summary>
        [Test]
        public void TestMultipleElementCollection()
        {
            var hundred = new int[100];
            for (int i = 0, loopTo = hundred.Length - 1; i <= loopTo; i++)
            {
                hundred[i] = i * i;
            }

            // Zero max numbers or start indexes beyond bounds of collection
            // should not enter the loop
            foreach (var _ in new clsWindowedEnumerable<int>(hundred, 0, 0))
            {
                Assert.Fail("ForEach (hundred,0,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(hundred, 1, 0))
            {
                Assert.Fail("ForEach (hundred,1,0) should not enter the loop");
            }

            foreach (var _ in new clsWindowedEnumerable<int>(hundred, 100, 1))
            {
                Assert.Fail("ForEach (hundred,100,1) should not enter the loop");
            }

            // max number of 1 should return exactly 1 element (that being the one at the
            // specified start index)
            var count = 0;
            foreach (var i in new clsWindowedEnumerable<int>(hundred, 13, 1))
            {
                count += 1;
                Assert.That(count, Is.EqualTo(1), "For Each (hundred,13,1) should only execute once");
                Assert.That(i, Is.EqualTo(13 * 13), "For Each (hundred,13,1) should return the element '" + (13 * 13) + "'");
            }

            Assert.That(count, Is.EqualTo(1), "For Each (hundred,13,1) should execute exactly once");

            // Shifting the window returns the correct values and no more than the specified
            // max number of values.
            for (var i = 0; i <= 9; i++)
            {
                count = 0;
                foreach (var j in new clsWindowedEnumerable<int>(hundred, i * 10, 10))
                {
                    var expected = (count + (i * 10)) * (count + (i * 10));
                    Assert.That(j, Is.EqualTo(expected));
                    count += 1;
                    Assert.That(count, Is.LessThanOrEqualTo(10), "For Each (hundred, 0, 10) should loop 10 times max - found count: " + count);
                }
                // After the loop has exited, it should always equal 10
                Assert.That(count, Is.EqualTo(10), "ForEach(hundred," + (i * 10) + ",10) should loop exactly 10 times");
            }

            // When the start index + max number goes beyond the bounds of the enumerable,
            // only those up to the end of the enumerable are iterated over and no errors
            // are encountered when enumerating up to them.
            count = 0;
            foreach (var _ in new clsWindowedEnumerable<int>(hundred, 95, 10))
            {
                count += 1;
                Assert.That(count, Is.LessThanOrEqualTo(5), "ForEach (hundred, 95, 10) should loop up to 5 times");
            }

            Assert.That(count, Is.EqualTo(5), "ForEach(hundred,95,10) should loop exactly 5 times");
        }
    }
}

#endif
