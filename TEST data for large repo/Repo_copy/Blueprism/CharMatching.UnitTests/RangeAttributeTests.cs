#if UNITTESTS

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace BluePrism.CharMatching.UnitTests.UI
{
    public class RangeAttributeTests
    {

        public static IEnumerable<object[]> GetWithinRangeTestCases()
        {
            yield return new object[] { 0, 100, 50 };
            yield return new object[] { 0, 1, 1 };
            yield return new object[] { 0, 1, 0 };
            yield return new object[] { -7.3, 42.534, 2.5123 };
            yield return new object[] { TimeSpan.Zero, TimeSpan.FromDays(10), TimeSpan.FromHours(200)};
            yield return new object[] { new DateTime(1987, 8, 23), new DateTime(2017, 8, 23), new DateTime(2017, 8, 3) };
            yield return new object[] { 'a', 'z', 'b' };
        }

        public static IEnumerable<object[]> GetGreaterThanRangeTestCases()
        {
            yield return new object[] { 0, 100, 150 };
            yield return new object[] { -7.3, 42.534, 222.5123 };
            yield return new object[] { TimeSpan.Zero, TimeSpan.FromDays(10), TimeSpan.FromHours(300) };
            yield return new object[] { new DateTime(1987, 8, 23), new DateTime(2017, 8, 23), new DateTime(2018, 8, 3) };
            yield return new object[] { 'a', 'b', 'z' };
        }

        public static IEnumerable<object[]> GetLessThanRangeTestCases()
        {
            yield return new object[] { 0, 100, -100 };
            yield return new object[] { -7.3, 42.534, -20.5123 };
            yield return new object[] { TimeSpan.Zero, TimeSpan.FromDays(10), TimeSpan.FromHours(-200) };
            yield return new object[] { new DateTime(1987, 8, 23), new DateTime(2017, 8, 23), new DateTime(1967, 8, 3) };
            yield return new object[] { 'b', 'z', 'a' };
        }
                
        [TestCaseSource("GetWithinRangeTestCases")]
        public void ShouldBeWithinRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsInRange(value),Is.EqualTo(true));
        }

        [TestCaseSource("GetGreaterThanRangeTestCases")]
        [TestCaseSource("GetLessThanRangeTestCases")]
        public void ShouldNotBeWithinRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsInRange(value), Is.EqualTo(false));
        }

        [TestCaseSource("GetGreaterThanRangeTestCases")]
        public void ShouldBeGreaterThanRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsGreaterThanRange(value), Is.EqualTo(true));
        }

        [TestCaseSource("GetLessThanRangeTestCases")]
        [TestCaseSource("GetWithinRangeTestCases")]
        public void ShouldNotBeGreaterThanRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsGreaterThanRange(value), Is.EqualTo(false));
        }

        [TestCaseSource("GetLessThanRangeTestCases")]
        public void ShouldBeLessThanRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsLessThanRange(value), Is.EqualTo(true));
        }

        [TestCaseSource("GetGreaterThanRangeTestCases")]
        [TestCaseSource("GetWithinRangeTestCases")]
        public void ShouldNotBeLessThanRange(object min, object max, object value)
        {
            var range = new CharMatching.UI.RangeAttribute(min, max);
            Assert.That(range.IsLessThanRange(value), Is.EqualTo(false));
        }

        [Test]
        public void ShouldNotAllowMaxToBeLessThanMin()
        {
            Assert.Throws<ArgumentException>(() => new CharMatching.UI.RangeAttribute(10, 9));
        }

        [Test]
        public void ShouldNotAllowMaxOrMinToBeTypesThatDoNotImplementIComparable()
        {
            var v1 = new { Value1 = 108, Value2 = "Hello" };
            var v2 = new { Value2 = 103, Value1 = "Why" };
            Assert.Throws<ArgumentException>(() => new CharMatching.UI.RangeAttribute(v1, v2));
        }

        [Test]
        public void ShouldNotAllowMaxAndMinToBeDifferentTypes()
        {
            Assert.Throws<ArgumentException>(() => new CharMatching.UI.RangeAttribute(2, "Hello"));
        }


        

    }
}

#endif
