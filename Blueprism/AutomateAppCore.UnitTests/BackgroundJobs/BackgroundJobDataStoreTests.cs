using System;
using NUnit.Framework;
using BluePrism.AutomateAppCore.BackgroundJobs;

namespace AutomateAppCore.UnitTests.BackgroundJobs
{
    public class BackgroundJobDataStoreTests
    {
        [Test]
        public void GetBackgroundJob_AfterUpdates_ShouldRetrieveData()
        {
            var dataStore = new BackgroundJobDataStore();
            var id = Guid.NewGuid();
            var storedData = new BackgroundJobData(BackgroundJobStatus.Running, 55, "Running", DateTime.UtcNow);
            dataStore.UpdateJob(id, storedData);
            var retrievedData = dataStore.GetBackgroundJob(id, false);
            Assert.That(retrievedData, Is.EqualTo(storedData));
        }

        [Test]
        public void GetBackgroundJob_AfterMultipleUpdates_ShouldRetrieveMostRecentData()
        {
            var dataStore = new BackgroundJobDataStore();
            var id = Guid.NewGuid();
            var storedData1 = new BackgroundJobData(BackgroundJobStatus.Running, 55, "Running", DateTime.UtcNow);
            var storedData2 = new BackgroundJobData(BackgroundJobStatus.Running, 65, "Running", DateTime.UtcNow);
            dataStore.UpdateJob(id, storedData1);
            dataStore.UpdateJob(id, storedData2);
            var retrievedData = dataStore.GetBackgroundJob(id, false);
            Assert.That(retrievedData, Is.EqualTo(storedData2));
        }

        [Test]
        public void GetBackgroundJob_WhenClearedCompleteJobs_ShouldReturnUnknownJobData()
        {
            var dataStore = new BackgroundJobDataStore();
            var id = Guid.NewGuid();
            var storedData = new BackgroundJobData(BackgroundJobStatus.Success, 55, "Running", DateTime.UtcNow);
            dataStore.UpdateJob(id, storedData);
            var retrievedData = dataStore.GetBackgroundJob(id, true);
            var retrievedDataAfterClear = dataStore.GetBackgroundJob(id, false);
            Assert.That(retrievedData, Is.EqualTo(storedData));
            Assert.That(retrievedDataAfterClear, Is.EqualTo(BackgroundJobData.Unknown));
        }

        [Test]
        public void GetBackgroundJob_WithUnknownJob_ShouldReturnUnknownJobData()
        {
            var dataStore = new BackgroundJobDataStore();
            var id = Guid.NewGuid();
            var data = dataStore.GetBackgroundJob(id, false);
            Assert.That(data, Is.EqualTo(BackgroundJobData.Unknown));
        }

        [Test]
        public void RemoveExpiredJobs_ShouldRemoveExpiredJobs()
        {
            var dataStore = new BackgroundJobDataStore();
            var currentDate = DateTime.UtcNow;
            var id1 = Guid.NewGuid();
            var data1 = new BackgroundJobData(BackgroundJobStatus.Running, 55, "Running", currentDate);
            dataStore.UpdateJob(id1, data1);
            var id2 = Guid.NewGuid();
            var data2 = new BackgroundJobData(BackgroundJobStatus.Running, 65, "Running", currentDate.AddMinutes(-45));
            dataStore.UpdateJob(id2, data2);
            dataStore.RemoveExpiredJobs(currentDate.AddMinutes(-30));
            var retrievedData1 = dataStore.GetBackgroundJob(id1, false);
            Assert.That(retrievedData1, Is.EqualTo(data1));
            var retrievedData2 = dataStore.GetBackgroundJob(id2, false);
            Assert.That(retrievedData2, Is.EqualTo(BackgroundJobData.Unknown));
        }
    }
}
