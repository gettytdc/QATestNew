#if UNITTESTS
using BluePrism.Data.DataModels.WorkQueueAnalysis;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BluePrism.WorkQueueAnalysis.UnitTests
{
    class SnapshotDayConfigurationUnitTests
    {
        [Test]
        public void TestIsEqualTo_ConfigsAreEqual_ReturnsTrue()
        {
            SnapshotDayConfiguration config1 = new SnapshotDayConfiguration(
                true, false, true, true, true, true, false);

            SnapshotDayConfiguration config2 = new SnapshotDayConfiguration(
                true, false, true, true, true, true, false);
            var result = config1.IsEqualTo(config2);
            Assert.AreEqual(result, true);
        }

        [Test]
        public void TestIsEqualTo_ConfigsAreNotEqual_ReturnsFalse()
        {
            SnapshotDayConfiguration config1 = new SnapshotDayConfiguration(
                true, true, true, false, true, true, true);

            SnapshotDayConfiguration config2 = new SnapshotDayConfiguration(
                true, false, true, true, true, false, true);
            var result = config1.IsEqualTo(config2);
            Assert.AreEqual(result, false);
        }
    }
}

#endif