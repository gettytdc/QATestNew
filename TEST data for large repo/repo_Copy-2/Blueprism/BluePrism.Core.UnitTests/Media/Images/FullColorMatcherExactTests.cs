#if UNITTESTS

using System.Drawing;
using BluePrism.Core.Media.Images;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Media.Images
{
    public class FullColorMatcherExactTests
    {
       [TestCaseSource("Cases1")]
        public void ExactColorsShouldMatch(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            AssertMatches(r1, g1, b1, r2, g2, b2, true);
        }

        protected static object[] Cases1 =
        {
            new object[]{255, 0, 0, 255, 0, 0},
            new object[]{0, 255, 0, 0, 255, 0},
            new object[]{0, 255, 0, 0, 255, 0},
            new object[]{1, 2, 3, 1, 2, 3},
            new object[]{3, 1, 2, 3, 1, 2},
            new object[]{1, 3, 2, 1, 3, 2}
        };

       [TestCaseSource("Cases2")]
        public void DifferentColorsShouldNotMatch(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            AssertMatches(r1, g1, b1, r2, g2, b2, false);
        }

        protected static object[] Cases2 =
        {
            new object[]{255, 0, 0, 254, 0, 0},
            new object[]{0, 255, 0, 0, 254, 0},
            new object[]{0, 0, 254, 0, 254, 0},
            new object[]{1, 2, 3, 1, 2, 4},
            new object[]{3, 1, 2, 3, 0, 2},
            new object[]{ 1, 3, 2, 1, 5, 2 }
        };

        private static void AssertMatches(int r1, int g1, int b1, int r2, int g2, int b2, bool expected)
        {
            var color1 = Color.FromArgb(255, r1, g1, b1);
            var color2 = Color.FromArgb(255, r2, g2, b2);
            var matcher = new FullColorMatcher(0);
            bool matches = matcher.Match(color1, color2);
            Assert.That(matches, Is.EqualTo(expected));
        }
    }
}

#endif