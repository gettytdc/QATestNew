using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System;
using System.Linq;

namespace AutomateAppCore.UnitTests.Encryption
{
    /// <summary>
    /// Contains tests for the <see cref="clsEncryptionScheme"/> class
    /// </summary>
    [TestFixture]
    public class EncryptionSchemeTests
    {

        /// <summary>
        /// Test that in the ordered list of algorithms, which is used to populate combo
        /// boxes in the UI, retired algorithms appear at the bottom. This test should
        /// prevent anyone from changing the order of the list so a retired algorithm
        /// appears above an un-retired algorithm.
        /// </summary>
        [Test]
        public void EnsureRetiredAlgorithmsAppearAtBottomOfOrderedList()
        {
            Assert.That(clsEncryptionScheme.GetOrderedAlgorithms().SkipWhile(a => !a.IsRetired()).Any(a => !a.IsRetired()), Is.False);
        }

        /// <summary>
        /// Test that in the ordered list of algorithms, which is used to populate combo
        /// boxes in the UI, Algorithms.None is not returned.
        /// </summary>
        [Test]
        public void EnsureNoneIsNotReturnedAsPartOfOrderedList()
        {
            Assert.That(!clsEncryptionScheme.GetOrderedAlgorithms().Contains(EncryptionAlgorithm.None));
        }

        /// <summary>
        /// Test that all algorithms except None are returned. This should prevent anyone
        /// forgetting to add a new algorithm to the GetOrderedAlgorithms function.
        /// </summary>
        [Test]
        public void EnsureEveryAlgorithmExceptNoneReturnedAsPartOfOrderedList()
        {
            Assert.That(Enum.GetValues(typeof(EncryptionAlgorithm)).OfType<EncryptionAlgorithm>().Where(x => x != EncryptionAlgorithm.None)
                .Except(clsEncryptionScheme.GetOrderedAlgorithms()).Any(), Is.False);
        }
    }
}
