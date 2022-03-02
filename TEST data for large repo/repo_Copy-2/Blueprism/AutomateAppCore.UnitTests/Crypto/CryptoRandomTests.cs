using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AutomateAppCore.UnitTests.Crypto
{
    [TestFixture]
    public class CryptoRandomTests
    {
        [Test]
        public void TestRandom()
        {
            var randomIntegers = new List<int>();

            using (var rndGen = new CryptoRandom())
            {
                for (var i = 1; i <= 100; i++)
                    randomIntegers.Add(rndGen.Next());
            }

            // Can't really test random, but if there's a hundred numbers I'd expect at least 50 of them to be different every time.
            // Really just checking it isn't spitting out the same number each time.
            var uniqueCount = randomIntegers.Distinct().Count();
            Assert.Greater(uniqueCount, 50, "Check that at least 50 of the random numbers are unique out of 100");
        }

        [Test]
        public void TestBoundedRandom()
        {
            var randomIntegers = new List<int>();

            using (var rndGen = new CryptoRandom())
            {
                for (var i = 1; i <= 100; i++)
                    randomIntegers.Add(rndGen.Next(1, 100));
            }

            // Can't really test random, but if there's a hundred numbers I'd expect at least 10 of them to be different every time.
            // Really just checking it isn't spitting out the same number each time.
            var uniqueCount = randomIntegers.Distinct().Count();
            Assert.Greater(uniqueCount, 10, "Check that at least 10 of the random numbers are unique out of 100");

            // Check that none are < 1
            Assert.IsFalse(randomIntegers.Exists(x => x < 1), "Check that no number is less than 1");
            Assert.IsFalse(randomIntegers.Exists(x => x > 100), "Check that no number is greater than 100");
        }

        [Test]
        public void TestUpperBoundedRandom()
        {
            var randomIntegers = new List<int>();

            using (var rndGen = new CryptoRandom())
            {
                for (var i = 1; i <= 100; i++)
                    randomIntegers.Add(rndGen.Next(50));
            }

            // Can't really test random, but if there's a hundred numbers I'd expect at least 10 of them to be different.
            // Really just checking it isn't spitting out the same number each time.
            var uniqueCount = randomIntegers.Distinct().Count();
            Assert.Greater(uniqueCount, 10, "Check that at least 10 of the random numbers are unique out of 100");

            // Check that none are more than 50
            Assert.IsFalse(randomIntegers.Exists(x => x > 50), "Check that no number is less than 50");
        }

        [Test]
        public void TestRandomDouble()
        {
            var randomDoubles = new List<double>();

            using (var rndGen = new CryptoRandom())
            {
                for (var i = 1; i <= 100; i++)
                    randomDoubles.Add(rndGen.NextDouble());
            }

            // Can't really test random, but if there's a hundred numbers I'd expect at least 50 of them to be different every time.
            // Really just checking it isn't spitting out the same number each time.
            var uniqueCount = randomDoubles.Distinct().Count();
            Assert.Greater(uniqueCount, 50, "Check that at least 50 of the random numbers are unique out of 100");
        }
    }
}
