#if UNITTESTS
namespace BluePrism.WorkQueueAnalysis.UnitTests
{
    using BluePrism.Data.DataModels.WorkQueueAnalysis;
    using System;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Data;
    
    public class SnapshottingChangesetUnitTests
    {
        private DataTable table = new DataTable();
        private SnapshotConfiguration config;
        private SnapshotConfiguration configDisabled;

        private int configId = 99;
        private int previouslyConfiguredQueueId_enabled = 15794;
        private int previouslyConfiguredQueueId_disabled = 16000;
        
        private readonly List<int> emptyList = new List<int>();
        private readonly Dictionary<int, int> emptyDictionary = new Dictionary<int, int>();
        private Dictionary<int, int> configIDsAndConfiguredSnapshotRowCount = new Dictionary<int, int>();

        [SetUp]
        public void Setup()
        {
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = table.NewRow();
            row[0] = previouslyConfiguredQueueId_enabled;
            row[1] = configId;
            row[2] = true;
            table.Rows.Add(row);

            var row2 = table.NewRow();
            row2[0] = previouslyConfiguredQueueId_disabled;
            row2[1] = configId;
            row2[2] = false;
            table.Rows.Add(row2);

            config = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));

            configDisabled = new SnapshotConfiguration(
                configId,
                false,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));

            configIDsAndConfiguredSnapshotRowCount = new Dictionary<int, int>()
            {
                {config.Id , config.ConfiguredSnapshotRowsPerQueue()  }
            };
        }

        [TearDown]
        public void TearDown()
        {
         table = new DataTable();
         configIDsAndConfiguredSnapshotRowCount = new Dictionary<int, int>();
        }

        [Test]
        public void ConfigPropertiesHaveChanged_NoQueuesAssigned_DoesNothing()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.OneHour,
                TimeZoneInfo.FindSystemTimeZoneById("Morocco Standard Time"),
                new NodaTime.LocalTime(01, 00),
                new NodaTime.LocalTime(15, 00),
                new SnapshotDayConfiguration(true, false, false, true, true, false, false));

            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>(), new List<int>(),
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
        }

        [Test]
        public void ConfigPropertiesHaveChanged_QueueAssigned_ConfigRemainsDisabled_DoesNothing()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                false,
                "Test Name",
                SnapshotInterval.OneHour,
                TimeZoneInfo.FindSystemTimeZoneById("Morocco Standard Time"),
                new NodaTime.LocalTime(01, 00),
                new NodaTime.LocalTime(15, 00),
                new SnapshotDayConfiguration(true, false, false, true, true, false, false));

            var changeset = new SnapshottingChangeset(configDisabled, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
        }

        [Test]
        public void TimezoneHasChanged_QueueRemainsAssigned_DeletesAndInsertsCorrectRows()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("Morocco Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));
            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void IntervalHasChanged_QueueRemainsAssigned_DeletesAndInsertsCorrectRows()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.TwentyFourHours,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));
            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void StartTimeHasChanged_QueueRemainsAssigned_DeletesAndInsertsCorrectRows()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(10, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));
            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void EndTimeHasChanged_QueueRemainsAssigned_DeletesAndInsertsCorrectRows()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(22, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));
            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void DaysOfWeekHaveChanged_QueueRemainsAssigned_DeletesAndInsertsCorrectRows()
        {
            SnapshotConfiguration newConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(07, 00),
                new NodaTime.LocalTime(19, 00),
                new SnapshotDayConfiguration(true, false, false, true, true, false, true));
            var changeset = new SnapshottingChangeset(config, newConfig,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigBeingDisabled_HasQueuesAssigned_DeletesCorrectRows()
        {

            var changeset = new SnapshottingChangeset(config, configDisabled,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigBeingEnabled_HasQueuesAssigned_InsertsCorrectRows()
        {
            var changeset = new SnapshottingChangeset(configDisabled, config,
                new List<int>() { 1, 2 }, new List<int>() { 1, 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 1, configId }, { 2, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }


        [Test]
        public void ConfigWasPreviouslyEnabled_QueuesBeingRemoved_DeletesCorrectRows()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2, 3 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigWasPreviouslyEnabled_QueuesBeingAddedAndRemoved_DeletesAndInsertsCorrectRows()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));

            Assert.AreEqual(new List<int>() { 1, 2, 3 }, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 4, configId }, { 5, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigWasPreviouslyDisabled_QueuesBeingRemoved_DoesNotDelete()
        {
            var changeset = new SnapshottingChangeset(configDisabled, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
        }

        [Test]
        public void ConfigIsEnabled_QueuesBeingAdded_NoExistingConfiguration_InsertsCorrectRows()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, 4 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { 4, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigIsDisabled_QueuesBeingAdded_NoExistingConfiguration_DoesNotAddAnyRows()
        {
            var changeset = new SnapshottingChangeset(config, configDisabled,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, 4 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigIsEnabled_QueueBeingAddedWhichWasPreviouslyConfiguredAndEnabled_DeletesAndInsertsCorrectRows()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, previouslyConfiguredQueueId_enabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(new List<int>() { previouslyConfiguredQueueId_enabled },
                changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { previouslyConfiguredQueueId_enabled, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void
            ConfigIsDisabled_QueueBeingAddedWhichWasPreviouslyConfiguredAndEnabled_DeletesCorrectRowsButAddsNone()
        {
            var changeset = new SnapshottingChangeset(configDisabled, configDisabled,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, previouslyConfiguredQueueId_enabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(new List<int>() { previouslyConfiguredQueueId_enabled },
                changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void ConfigIsDisabled_QueueBeingAddedWhichWasPreviouslyConfiguredButDisabled_DoesNothing()
        {
            var changeset = new SnapshottingChangeset(configDisabled, configDisabled,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, previouslyConfiguredQueueId_disabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(emptyDictionary, changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void
            ConfigIsEnabled_QueueBeingAddedWhichWasPreviouslyConfiguredButDisabled_InsertsCorrectRowsButDeletesNone()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() { 1, 2, 3 }, new List<int>() { 1, 2, 3, previouslyConfiguredQueueId_disabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            Assert.AreEqual(emptyList, changeset.QueuesToDeleteSnapshotAndTrendData);
            Assert.AreEqual(new Dictionary<int, int>() { { previouslyConfiguredQueueId_disabled, configId } },
                changeset.QueuesAndConfigurationsToAddSnapshotRows);
        }

        [Test]
        public void TestConstructor_nullListQueues_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new SnapshottingChangeset(config, config,
                null, null,
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount)));
        }

        [Test]
        public void TestConstructor_EmptyListQueues_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new SnapshottingChangeset(config, config,
                new List<int>() { }, new List<int>() {  },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount)));
        }

        [Test]
        public void TestConstructor_OldConfigNull_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new SnapshottingChangeset(null, config,
                new List<int>() { }, new List<int>() { 456, 25 , 789},
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount)));
        }

        [Test]
        public void TestConstructor_EmptyConfigTable_DoesNotThrow()
        {
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add(new DataColumn("Id", typeof(int)));
            emptyTable.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            emptyTable.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            Assert.DoesNotThrow(() => new SnapshottingChangeset(config, config,
                new List<int>() {1,2 }, new List<int>() {1,2 },
                new QueueConfigurationsDataTable(emptyTable, configIDsAndConfiguredSnapshotRowCount)));
        }

        [Test]

        public void NetConfiguredSnapshotRowChange_NewConfig_OneQueue_ReturnsExpectedValue()
        {
            var newConfig = new SnapshotConfiguration(
                25,
                true,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));

            var changeset = new SnapshottingChangeset(null, newConfig,
                new List<int>() { }, new List<int>() {4},
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((96 * 7), result);
        }

        [Test]

        public void NetConfiguredSnapshotRowChange_NewConfig_TwoQueues_ReturnsExpectedValue()
        {
            var newConfig = new SnapshotConfiguration(
                25,
                true,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));

            var changeset = new SnapshottingChangeset(null, newConfig,
                new List<int>() { }, new List<int>() { 4, 7 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((2 * 96 * 7), result);
        }

        [Test]

        public void NetConfiguredSnapshotRowChange_NoChanges_ReturnsZero()
        {
            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() {1, 2}, new List<int>() {1, 2},
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual(0,result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_ExistingConfig_TwoNewQueues_ReturnsExpectedValue()
        {
            var testConfig = new SnapshotConfiguration(
                25,
                true,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));
            var changeset = new SnapshottingChangeset(testConfig, testConfig,
                new List<int>() { }, new List<int>() { 4, 7 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((2 * 96 * 7), result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_ExistingConfig_OneNewQueueOneDeletionOneRemaining_NoOtherChanges__ReturnsZero()
        {

            var configAndRowCount = new Dictionary<int, int>()
            {
                {config.Id, config.ConfiguredSnapshotRowsPerQueue()}
            };


            var existingConfigsTable = new DataTable();
            existingConfigsTable.Columns.Add(new DataColumn("Id", typeof(int)));
            existingConfigsTable.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            existingConfigsTable.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = existingConfigsTable.NewRow();
            row[0] = 1;
            row[1] = configId;
            row[2] = true;
            existingConfigsTable.Rows.Add(row);

            var row2 = existingConfigsTable.NewRow();
            row2[0] = 2;
            row2[1] = configId;
            row2[2] = true;
            existingConfigsTable.Rows.Add(row2);

            var changeset = new SnapshottingChangeset(config, config,
                new List<int>() {1,2}, new List<int>() { 2, 3 },
                new QueueConfigurationsDataTable(existingConfigsTable, configAndRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((0), result);
        }


        [Test]
        public void NetConfiguredSnapshotRowChange_ExistingConfig_OneNewQueueOneDeletionOneRemaining_ChangeInterval__ReturnsExpected()
        {
            var config15Minutely7days = new SnapshotConfiguration(
                configId,
                true,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));
            var configHourly5days = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.OneHour,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));

            var configAndRowCount = new Dictionary<int, int>()
            {
                {config15Minutely7days.Id, config15Minutely7days.ConfiguredSnapshotRowsPerQueue()}
            };

            var existingConfigsTable = new DataTable();
            existingConfigsTable.Columns.Add(new DataColumn("Id", typeof(int)));
            existingConfigsTable.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            existingConfigsTable.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = existingConfigsTable.NewRow();
            row[0] = 1;
            row[1] = configId;
            row[2] = true;
            existingConfigsTable.Rows.Add(row);

            var row2 = existingConfigsTable.NewRow();
            row2[0] = 2;
            row2[1] = configId;
            row2[2] = true;
            existingConfigsTable.Rows.Add(row2);

            var changeset = new SnapshottingChangeset(config15Minutely7days, configHourly5days,
                new List<int>() { 1, 2 }, new List<int>() { 2, 3 },
                new QueueConfigurationsDataTable(existingConfigsTable, configAndRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((24 * 5 * 2) - (2 * 96 * 7), result);
      
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_ExistingConfig_AddOneQueue_ReturnsExpected()
        {
            var configHourly7days9To5 = new SnapshotConfiguration(
                25,
                true,
                "New config",
                SnapshotInterval.OneHour ,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(09, 00),
                new NodaTime.LocalTime(17, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));

            var changeset = new SnapshottingChangeset(configHourly7days9To5, configHourly7days9To5,
                new List<int>() {  }, new List<int>() { 2 },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((9 * 7 * 1), result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_DisableConfig_1Queue_returnsExpected()
        {
            var changeset = new SnapshottingChangeset(config, configDisabled ,
                new List<int>() { previouslyConfiguredQueueId_enabled }, new List<int>() { previouslyConfiguredQueueId_enabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual(-1 * 49 * 5, result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_EnableConfig_1Queue_returnsExpected()
        {
            var changeset = new SnapshottingChangeset(configDisabled, config,
                new List<int>() { previouslyConfiguredQueueId_disabled }, new List<int>() { previouslyConfiguredQueueId_disabled },
                new QueueConfigurationsDataTable(table, configIDsAndConfiguredSnapshotRowCount));
            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual(1 * 49 * 5, result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_AddOneQueuePreviouslyConfiguredEnabled_returnsExpected()
        {
            var previousConfig = new SnapshotConfiguration(
                25,
                true,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));
            var testConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.OneHour,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));

            var table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = table.NewRow();
            row[0] = previouslyConfiguredQueueId_enabled;
            row[1] = previousConfig.Id;
            row[2] = true;
            table.Rows.Add(row);

            var configAndRowCount = new Dictionary<int, int>()
            {
                {previousConfig.Id, previousConfig.ConfiguredSnapshotRowsPerQueue()}
            };

            var changeset = new SnapshottingChangeset(testConfig, testConfig,
                  new List<int>() { 1 },
                  new List<int>() { 1, previouslyConfiguredQueueId_enabled },
                  new QueueConfigurationsDataTable(table, configAndRowCount));

            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((24 * 5) - (96 * 7), result);
        }

        [Test]
        public void NetConfiguredSnapshotRowChange_AddOneQueuePreviouslyConfiguredDisabled_returnsExpected()
        {
            var previousConfig = new SnapshotConfiguration(
                25,
                false,
                "New config",
                SnapshotInterval.FifteenMinutes,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, true, true));
            var testConfig = new SnapshotConfiguration(
                configId,
                true,
                "Test Name",
                SnapshotInterval.OneHour,
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                new NodaTime.LocalTime(00, 00),
                new NodaTime.LocalTime(00, 00),
                new SnapshotDayConfiguration(true, true, true, true, true, false, false));

            var table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = table.NewRow();
            row[0] = previouslyConfiguredQueueId_disabled;
            row[1] = previousConfig.Id;
            row[2] = true;
            table.Rows.Add(row);

            var configAndRowCount = new Dictionary<int, int>()
            {
                {previousConfig.Id, previousConfig.ConfiguredSnapshotRowsPerQueue()}
            };

            var changeset = new SnapshottingChangeset(testConfig, testConfig,
                  new List<int>() { 1 },
                  new List<int>() { 1, previouslyConfiguredQueueId_disabled },
                  new QueueConfigurationsDataTable(table, configAndRowCount));

            var result = changeset.NetConfiguredSnapshotRowChange();
            Assert.AreEqual((24 * 5) - (96 * 7), result);
        }

    }


}
#endif