using BluePrism.AutomateAppCore.DataMonitor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System;

namespace AutomateAppCore.UnitTests.DataMonitor
{
    [TestFixture]
    public class DataMonitorTests
    {
        /// <summary>
        /// Gets a basic data store with entries for the Scheduler, Font and Roles data
        /// names, currently set to 27, 1 and 7, respectively.
        /// </summary>
        /// <returns>A pre-initialised monitored datastore wrapped by a dictionary.
        /// </returns>
        private DictionaryMonitoredDataStore GetTestStore()
        {
            return new DictionaryMonitoredDataStore(
                new Dictionary<string, long>()
                {
                    {DataNames.Scheduler, 27},
                    {DataNames.Font, 1},
                    {DataNames.Roles, 7}
                });
        }

        /// <summary>
        /// Some basic tests to give us some confidence that the Dictionary Monitored
        /// Data Store works correctly, not least because we use it throughout most (all,
        /// at time of going to press) of the other tests.
        /// </summary>
        [Test]
        public void TestDictionaryMonitoredDataStore()
        {
            var store = new DictionaryMonitoredDataStore();
            long ver = 0;

            // Should be empty at first
            Assert.That(store.GetMonitoredData(), Is.EqualTo(new Dictionary<string, long>()));

            // Now we can add things - that should set them to 1L:
            store.Increment(DataNames.Scheduler);
            Assert.That(store.GetMonitoredData(),
                Is.EqualTo(new Dictionary<string, long>()
                {
                    {DataNames.Scheduler, 1L}
                }));

            Assert.That(store.HasDataUpdated(DataNames.Scheduler, ref ver), Is.True);
            Assert.That(ver, Is.EqualTo(1L));

            store.Increment(DataNames.Scheduler);
            Assert.That(store.GetMonitoredData(),
                Is.EqualTo(new Dictionary<string, long>()
                {
                    {DataNames.Scheduler, 2L}
                }));

            Assert.That(store.HasDataUpdated(DataNames.Scheduler, ref ver), Is.True);
            Assert.That(ver, Is.EqualTo(2L));

            // ver is set to the latest, we've not changed it;
            Assert.That(store.HasDataUpdated(DataNames.Scheduler, ref ver), Is.False);
        }

        /// <summary>
        /// Tests that an update to the store is recognised when the monitor is polled
        /// </summary>
        [Test]
        public void TestSimpleDataMonitorUpdate()
        {
            var store = GetTestStore();
            var mon = new SimpleDataMonitor(store);
            string updated = null;

            mon.MonitoredDataUpdated += delegate (object sender, MonitoredDataUpdateEventArgs e) { updated = e.Name; };

            store.Increment(DataNames.Roles);
            ((IDataMonitor)mon).Poll();

            Assert.That(updated, Is.EqualTo(DataNames.Roles));
        }

        /// <summary>
        /// Tests that an update to the store is recognised when the monitor is polled
        /// </summary>
        [Test]
        public void TestSimpleDataMonitorUnchanged()
        {
            var store = GetTestStore();
            var mon = new SimpleDataMonitor(store);
            string updated = null;

            mon.MonitoredDataUpdated += delegate (object sender, MonitoredDataUpdateEventArgs e) { updated = e.Name; };

            // Note that we do nothing to the store here...
            ((IDataMonitor)mon).Poll();

            Assert.That(updated, Is.Null);
        }

        /// <summary>
        /// Tests that a Poll() which captures multiple changes reports them correctly.
        /// </summary>
        [Test]
        public void TestSimpleDataMonitorMultiChanges()
        {
            var store = GetTestStore();
            var mon = new SimpleDataMonitor(store);

            var updated = new List<string>();
            mon.MonitoredDataUpdated += delegate (object sender, MonitoredDataUpdateEventArgs e)
            {
                updated.Add(e.Name);
            };

            store.Increment(DataNames.Roles);
            store.Increment(DataNames.Font);

            ((IDataMonitor)mon).Poll();

            // Note that we use 'EquivalentTo' because the order of the changes is not
            // known and therefore they could be reported in any order.
            // The important thing is that they are all reported
            Assert.That(updated, Is.EquivalentTo(new List<string>()
            {
                DataNames.Roles,
                DataNames.Font
            }));

            updated.Clear();

            // Try with two existing data names and a new one
            store.Increment(DataNames.Roles);
            store.Increment(DataNames.Font);
            store.Increment("Fish");

            ((IDataMonitor)mon).Poll();

            Assert.That(updated, Is.EquivalentTo(new List<string>()
            {
                DataNames.Roles,
                DataNames.Font, "Fish"
            }));
        }

        /// <summary>
        /// Tests that changes in the store are not reported if the monitor is disposed;
        /// also that the <see cref="IDataMonitor.Poll"/> call does not raise an
        /// exception in that case.
        /// </summary>
        [Test]
        public void TestSimpleDataMonitorDispose()
        {
            var store = GetTestStore();
            var mon = new SimpleDataMonitor(store);
            string updated = null;

            mon.MonitoredDataUpdated += delegate (object sender, MonitoredDataUpdateEventArgs e) { updated = e.Name; };

            mon.Dispose();

            store.Increment(DataNames.Roles);

            ((IDataMonitor)mon).Poll();

            Assert.That(updated, Is.Null);
        }

        /// <summary>
        /// Does some basic tests for the TimerDataMonitor, ensuring that it doesn't run
        /// when it is not enabled, and that it works correctly when it is.
        /// </summary>
        [Test]
        public void TestTimerDataMonitor()
        {
            var store = GetTestStore();
            using (var mon = new TimerDataMonitor(store) { Enabled = false, Interval = TimeSpan.FromSeconds(0.25) })
            {
                string updated = null;

                mon.MonitoredDataUpdated += delegate (object sender, MonitoredDataUpdateEventArgs e)
                {
                    updated = e.Name;
                };
                store.Increment(DataNames.Roles);
                // Interval is 0.25s; 1s should be plenty to ensure it hasn't run
                Thread.Sleep(TimeSpan.FromSeconds(1));

                Assert.That(updated, Is.Null);

                mon.Enabled = true;

                Thread.Sleep(TimeSpan.FromSeconds(1));

                Assert.That(updated, Is.EqualTo(DataNames.Roles));

                // Let's test a whole new entry
                store.Increment("Fish");

                Thread.Sleep(TimeSpan.FromSeconds(1));
                Assert.That(updated, Is.EqualTo("Fish"));
            }
        }
    }
}
