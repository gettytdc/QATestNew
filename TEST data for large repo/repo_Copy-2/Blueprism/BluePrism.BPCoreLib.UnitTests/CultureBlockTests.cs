#if UNITTESTS
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{

    /// <summary>
    /// Suite of tests for the <see cref="CultureBlock"/> class.
    /// </summary>
    [TestFixture]
    public class CultureBlockTests
    {

        /// <summary>
        /// Broadly tests the <see cref="CultureBlock"/> class, ensuring that it works
        /// as it's supposed to.
        /// </summary>
        [Test]
        public void TestCultureBlock()
        {
            // Get the current culture (typically en-GB)
            var ci = Thread.CurrentThread.CurrentCulture;

            // Have a block dedicated to France
            using (new CultureBlock(CultureInfo.CreateSpecificCulture("fr-FR")))
            {
                var curr = Thread.CurrentThread.CurrentCulture;
                Assert.That(ci, Is.Not.EqualTo(curr));
                Assert.That(curr.Name, Is.EqualTo("fr-FR"));
                Assert.That(decimal.TryParse("1.5", out var dec), Is.False);
                Assert.That(decimal.TryParse("1,5", out dec), Is.True);
                Assert.That(dec, Is.EqualTo(1.5m));
            }

            // Note that this bit of the test assumes an English style culture - ie. that
            // the current culture uses "." as decimal separator and "," as thousand
            // separator. The test may fail if run in a culture for which this isn't true.
            if (true) // Scoping block
            {
                var curr = Thread.CurrentThread.CurrentCulture;
                Assert.That(ci, Is.EqualTo(curr));
                Assert.That(curr.Name, Is.Not.EqualTo("fr-FR"));
                // 1,5 - "," is treated as a thousand separator, hence number parsed as 15
                Assert.That(decimal.TryParse("1,5", out var dec), Is.True);
                Assert.That(dec, Is.EqualTo(15m));
                Assert.That(decimal.TryParse("1.5", out dec), Is.True);
                Assert.That(dec, Is.EqualTo(1.5m));
            }

            // Test nested blocks - first French, then invariant, then back to French
            using (new CultureBlock(CultureInfo.CreateSpecificCulture("fr-FR")))
            {
                var curr = Thread.CurrentThread.CurrentCulture;
                Assert.That(ci, Is.Not.EqualTo(curr));
                Assert.That(curr.Name, Is.EqualTo("fr-FR"));
                Assert.That(decimal.TryParse("1.5", out var dec), Is.False);
                Assert.That(decimal.TryParse("1,5", out dec), Is.True);
                Assert.That(dec, Is.EqualTo(1.5m));
                using (new CultureBlock(CultureInfo.InvariantCulture))
                {
                    curr = Thread.CurrentThread.CurrentCulture;
                    Assert.That(ci, Is.Not.EqualTo(curr));
                    Assert.That(curr.Name, Is.Empty); // Invariant has no name...
                    Assert.That(decimal.TryParse("1,5", out dec), Is.True);
                    Assert.That(dec, Is.EqualTo(15m));
                    Assert.That(decimal.TryParse("1.5", out dec), Is.True);
                    Assert.That(dec, Is.EqualTo(1.5m));
                }

                curr = Thread.CurrentThread.CurrentCulture;
                Assert.That(ci, Is.Not.EqualTo(curr));
                Assert.That(curr.Name, Is.EqualTo("fr-FR"));
                Assert.That(decimal.TryParse("1.5", out dec), Is.False);
                Assert.That(decimal.TryParse("1,5", out dec), Is.True);
                Assert.That(dec, Is.EqualTo(1.5m));
            }
        }
    }
}

#endif
