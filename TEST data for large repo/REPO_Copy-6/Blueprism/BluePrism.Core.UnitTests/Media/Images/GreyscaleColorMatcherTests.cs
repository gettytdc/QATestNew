#if UNITTESTS

using BluePrism.Core.Media.Images;
using NUnit.Framework;
using System.Drawing;

namespace BluePrism.Core.UnitTests.Media.Images
{
    public class GreyscaleColorMatcherTests
    {
        protected static object[] Cases1 =
        {
            new object[]{255, 0, 0, 255, 0, 0, 10},
            new object[]{0, 255, 0, 0, 245, 0, 10},
            new object[]{0, 255, 0, 0, 255, 0, 10},
            new object[]{0, 0, 0, 10, 10, 10, 10},
            new object[]{10, 10, 10, 0, 0, 0, 10},
            new object[]{127, 127, 127, 127, 127, 127, 10},
            new object[]{117, 117, 117, 127, 127, 127, 10},
            new object[]{ 127, 127, 127, 137, 137, 137, 10 }
        };

        [TestCaseSource("Cases1")]
        public void ColorsWithinToleranceRangeShouldMatch(int r1, int g1, int b1, int r2, int g2, int b2, int tolerance)
        {
            AssertMatches(r1, g1, b1, r2, g2, b2, tolerance, true);
        }

        protected static object[] Cases2 =
        {
            new object[] {0, 0, 0, 10, 10, 10, 9},
            new object[] {10, 10, 10, 0, 0, 0, 9},
            new object[] {117, 117, 117, 127, 127, 127, 9},
            new object[] {127, 127, 127, 137, 137, 137, 9}
        };
        
        [TestCaseSource("Cases2")]
        public void ColorsOutsideToleranceRangeShouldNotMatch(int r1, int g1, int b1, int r2, int g2, int b2, int tolerance)
        {
            AssertMatches(r1, g1, b1, r2, g2, b2, tolerance, false);
        }

        private static void AssertMatches(int r1, int g1, int b1, int r2, int g2, int b2, int tolerance, bool expected)
        {
            var color1 = Color.FromArgb(255, r1, g1, b1);
            var color2 = Color.FromArgb(255, r2, g2, b2);
            var matcher = new GreyScaleColorMatcher(tolerance);
            bool matches = matcher.Match(color1, color2);
            Assert.That(matches, Is.EqualTo(expected));
        }

    }
}

#endif