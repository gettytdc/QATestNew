#if UNITTESTS

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    /// <summary>
    /// Tests for the clsDataMonitor class
    /// </summary>
    [TestFixture]
    public class TestDataMonitor
    {
        private bool _changed;

        /// <summary>
        /// Basic implementation class for the (abstract) data monitor
        /// </summary>
        [Serializable]
        private class BasicDataMonitor : clsDataMonitor
        {
            public void Change() => base.MarkDataChanged("Some Data", "From this", "To this");
        }

        /// <summary>
        /// Handler added to the monitor to ensure that the event is not serialized.
        /// This just sets an instance variable to indicate that the event has fired.
        /// </summary>
        /// <param name="sender">ignored...</param>
        /// <param name="args">ignored...</param>
        private void HandleDataChanged(object sender, DataChangeEventArgs args) => _changed = true;

        /// <summary>
        /// Checks the serialization of the given monitor works as expected.
        /// </summary>
        /// <param name="mon">The monitor to check.</param>
        /// <param name="changed">True if the monitor should be showing that its
        /// data has changed, False if it should be showing that it has not.
        /// </param>
        private void CheckMonitorSerialization(BasicDataMonitor mon, bool changed)
        {
            Assert.That(mon.HasChanged(), Is.EqualTo(changed));
            var fmt = new BinaryFormatter();
            Stream ms = null;
            try
            {
                ms = new MemoryStream();
                fmt.Serialize(ms, mon);

                // The monitor should not be changed by the serialization process
                Assert.That(mon.HasChanged(), Is.EqualTo(changed));
                ms.Seek(0L, SeekOrigin.Begin);
                var newMon = (BasicDataMonitor)fmt.Deserialize(ms);
                Assert.That(newMon.HasChanged(), Is.EqualTo(mon.HasChanged()));
            }
            finally
            {
                ms?.Close();
            }
        }

        /// <summary>
        /// Tests that the data monitor serializes and deserializes correctly, ensuring
        /// that events are not serialized when the data is.
        /// </summary>
        [Test]
        public void TestSerialization()
        {
            CheckMonitorSerialization(new BasicDataMonitor(), false);
            var mon = new BasicDataMonitor();
            mon.Mark();
            CheckMonitorSerialization(mon, true);

            // Start afresh.
            mon = new BasicDataMonitor();
            mon.DataChanged += HandleDataChanged;
            var fmt = new BinaryFormatter();
            var ms = new MemoryStream();

            // mon should currently be dirty
            mon.Mark();
            Assert.That(mon.HasChanged(), Is.True);

            // Serialize it, then change it.
            fmt.Serialize(ms, mon);
            // Now clear it...
            mon.ResetChanged();
            Assert.That(mon.HasChanged(), Is.False);
            // And deserialize it
            ms.Seek(0L, SeekOrigin.Begin);
            var newMon = (BasicDataMonitor)fmt.Deserialize(ms);
            Assert.That(mon.HasChanged(), Is.Not.EqualTo(newMon.HasChanged()));

            // Test that the change event still fires on the original object.
            _changed = false;
            mon.Change();
            Assert.That(_changed, Is.True);

            // Test that the change event does *not* fire on the new object.
            _changed = false;
            newMon.Change();
            Assert.That(_changed, Is.False);
        }
    }
}

#endif
