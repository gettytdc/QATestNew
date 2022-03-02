#if UNITTESTS
namespace BluePrism.WorkQueueAnalysis.UnitTests
{
    using BluePrism.Data.DataModels.WorkQueueAnalysis;
    using System;
    using NUnit.Framework;
    using System.Data;
    using System.Collections.Generic;
    using System.Linq;

    public class QueueConfigurationsDataTableUnitTests
    {
        private DataTable _baseTable = new DataTable();

        private int _previouslyConfiguredQueueId_enabled = 15794;
        private int _previouslyConfiguredQueueId_disabled = 16000;
        private int _otherConfigurationIdEnabled = 656;
        private int _otherConfigurationIdDisabled = 680;

        [SetUp]
        public void Setup()
        {
            _baseTable.Columns.Add(new DataColumn("Id", typeof(int)));
            _baseTable.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            _baseTable.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            var row = _baseTable.NewRow();
            row[0] = _previouslyConfiguredQueueId_enabled;
            row[1] = _otherConfigurationIdEnabled;
            row[2] = true;
            _baseTable.Rows.Add(row);

            var row2 = _baseTable.NewRow();
            row2[0] = _previouslyConfiguredQueueId_disabled;
            row2[1] = _otherConfigurationIdDisabled;
            row2[2] = false;
            _baseTable.Rows.Add(row2);
        }

        [TearDown]
        public void TearDown()
        {
            _baseTable = new DataTable();
        }

        [Test]
        public void Constructor_NoRows_CorrectHeaders_DoesNotThrow()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            Assert.DoesNotThrow(() => new QueueConfigurationsDataTable(table, new Dictionary<int, int>()));
        }

        [Test]
        public void Constructor_CorrectlySetsConfiguredSnapshotValues()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(
                _baseTable,
                new Dictionary<int, int>()
                {
                    {_otherConfigurationIdEnabled, 672},
                    {_otherConfigurationIdDisabled, 7}
                });

            DataTable resultTable = new DataTable();
            resultTable.Columns.Add(new DataColumn("QueueId", typeof(int)));
            resultTable.Columns.Add(new DataColumn("ConfigurationId", typeof(int)));
            resultTable.Columns.Add(new DataColumn("ConfigurationEnabled", typeof(bool)));
            resultTable.Columns.Add(new DataColumn("ConfiguredSnapshotRowsPerQueue", typeof(int)));

            var row = resultTable.NewRow();
            row[0] = _previouslyConfiguredQueueId_enabled;
            row[1] = _otherConfigurationIdEnabled;
            row[2] = true;
            row[3] = 672;
            resultTable.Rows.Add(row);

            var row2 = resultTable.NewRow();
            row2[0] = _previouslyConfiguredQueueId_disabled;
            row2[1] = _otherConfigurationIdDisabled;
            row2[2] = false;
            row2[3] = 7;
            resultTable.Rows.Add(row2);
           
           Assert.AreEqual(true, TablesAreEqual(resultTable, testTable.Table));
        }

        [Test]
        public void Constructor_TooFewHeaders_ThrowsArgumentException()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));

            Assert.Throws<ArgumentException>(() => new QueueConfigurationsDataTable(table, new Dictionary<int, int>()));
        }

        [Test]
        public void Constructor_TooManyHeaders_ThrowsArgumentException()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(int)));
            table.Columns.Add(new DataColumn("Enabled", typeof(bool)));
            table.Columns.Add(new DataColumn("An extra column", typeof(bool)));

            Assert.Throws<ArgumentException>(() => new QueueConfigurationsDataTable(table, new Dictionary<int, int>()));
        }
        [Test]
        public void Constructor_WrongTypeHeaders_ThrowsArgumentException()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Id", typeof(bool)));
            table.Columns.Add(new DataColumn("ConfigId", typeof(bool)));
            table.Columns.Add(new DataColumn("Enabled", typeof(double)));

            Assert.Throws<ArgumentException>(() => new QueueConfigurationsDataTable(table, new Dictionary<int, int>()));
        }

        [Test]
        public void QueueHasExistingConfiguration_DoesHaveExisting_Disabled_ReturnsTrue()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(_baseTable, new Dictionary<int, int>());
            var result = testTable.QueueHasExistingConfiguration(_previouslyConfiguredQueueId_disabled);
            Assert.AreEqual(true, result);
        }
        [Test]
        public void QueueHasExistingConfiguration_DoesHaveExisting_Enabled_ReturnsTrue()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(_baseTable, new Dictionary<int, int>());
            var result = testTable.QueueHasExistingConfiguration(_previouslyConfiguredQueueId_enabled);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void QueueHasExistingConfiguration_DoesNotHaveExisting_ReturnsFalse()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(_baseTable, new Dictionary<int, int>());
            var result = testTable.QueueHasExistingConfiguration(78);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ExistingConfigIsEnabled_DoesHaveExistingAndIsEnabled_ReturnsTrue()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(
                _baseTable, 
                new Dictionary<int, int>()
                {
                    {_previouslyConfiguredQueueId_enabled, 672}
                });
            var result = testTable.ExistingConfigIsEnabled(_previouslyConfiguredQueueId_enabled);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void ExistingConfigIsEnabled_DoesHaveExistingAndIsNotEnabled_ReturnsFalse()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(_baseTable, new Dictionary<int, int>()
            {
                {_previouslyConfiguredQueueId_disabled, 672}
            });
            var result = testTable.ExistingConfigIsEnabled(_previouslyConfiguredQueueId_disabled);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void ExistingConfigIsEnabled_DoesNotHaveExisting_ReturnsFalse()
        {
            QueueConfigurationsDataTable testTable = new QueueConfigurationsDataTable(_baseTable, new Dictionary<int, int>());
            var result = testTable.ExistingConfigIsEnabled(78);
            Assert.AreEqual(false, result);
        }

        private bool TablesAreEqual(DataTable t1, DataTable t2)
        {
            if (t1 == null)
                return false;
            if (t2 == null)
                return false;
            if (t1.Rows.Count != t2.Rows.Count)
                return false;

            if (t1.Columns.Count != t2.Columns.Count)
                return false;

            if (t1.Columns.Cast<DataColumn>().Any(dc => !t2.Columns.Contains(dc.ColumnName)))
            {
                return false;
            }

            for (int i = 0; i <= t1.Rows.Count - 1; i++)
            {
                if (t1.Columns.Cast<DataColumn>().Any(dc1 => t1.Rows[i][dc1.ColumnName].ToString() != t2.Rows[i][dc1.ColumnName].ToString()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

#endif
