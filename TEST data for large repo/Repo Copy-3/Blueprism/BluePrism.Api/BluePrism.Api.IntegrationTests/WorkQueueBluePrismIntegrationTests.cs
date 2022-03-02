namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Apps72.Dev.Data.DbMocker;
    using Autofac;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Services;

    [TestFixture]
    public class WorkQueueBluePrismIntegrationTests : BluePrismIntegrationTestBase<WorkQueuesService>
    {
        WorkQueue TestWorkQueue = new WorkQueue
        {
            Id = Guid.NewGuid(),
            Name = "!!!TESTWORKQUEUE!!!",
            KeyField = "TestKeyField",
            Status = QueueStatus.Running,
            MaxAttempts = 5,
            IsEncrypted = false,
            EncryptionKeyId = 0,
            ProcessId = Guid.Empty,
            ResourceGroupId = Guid.Empty,
            CompletedItemCount = 2,
            PendingItemCount = 3,
            ExceptionedItemCount = 4,
            TotalCaseDuration = TimeSpan.FromSeconds(5),
            AverageWorkTime = TimeSpan.FromSeconds(6),
            TotalItemCount = 9,
            GroupId = Guid.Empty,
            GroupName = "test group",
            Ident = 123
        };

        public override void Setup()
        {
            base.Setup();

            GetMock<ITokenAccessor>()
                .SetupGet(m => m.TokenString)
                .Returns(string.Empty);
        }

        [Test]
        public async Task GetWorkQueues_CorrectlyRetrievesDataFromDatabase()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.IndexOf("from BPAWorkQueue", StringComparison.OrdinalIgnoreCase) >= 0)
                .ReturnsTable(MockTable
                    .WithColumns("id", "ident", "name", "keyfield", "running", "maxattempts", "encryptid", "processid", "snapshotconfigurationid", "processname", "resourcegroupid", "requiredFeature", "resourcegroupname", "total", "completed", "pending", "exceptioned", "totalworktime", "averageworkedtime", "groupname", "groupid")
                    .AddRow(
                        TestWorkQueue.Id,
                        123,
                        TestWorkQueue.Name,
                        TestWorkQueue.KeyField,
                        TestWorkQueue.Status == QueueStatus.Running ? 1 : 0,
                        TestWorkQueue.MaxAttempts,
                        TestWorkQueue.EncryptionKeyId,
                        DBNull.Value,
                        0,
                        DBNull.Value,
                        DBNull.Value,
                        "",
                        DBNull.Value,
                        9,
                        TestWorkQueue.CompletedItemCount,
                        TestWorkQueue.PendingItemCount,
                        TestWorkQueue.ExceptionedItemCount,
                        TestWorkQueue.TotalCaseDuration.TotalSeconds,
                        TestWorkQueue.AverageWorkTime.TotalSeconds,
                        TestWorkQueue.GroupName,
                        TestWorkQueue.GroupId));

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetWorkQueues(new WorkQueueParameters
            {
                ItemsPerPage = 10,
                SortBy = WorkQueueSortByProperty.NameAsc,
                NameFilter = new NullFilter<string>(),
                AverageWorkTimeFilter = new NullFilter<int>(),
                MaxAttemptsFilter = new NullFilter<int>(),
                CompletedItemCountFilter = new NullFilter<int>(),
                ExceptionedItemCountFilter = new NullFilter<int>(),
                KeyFieldFilter = new NullFilter<string>(),
                LockedItemCountFilter = new NullFilter<int>(),
                PendingItemCountFilter = new NullFilter<int>(),
                QueueStatusFilter = new NullFilter<Domain.QueueStatus>(),
                TotalCaseDurationFilter = new NullFilter<int>(),
                TotalItemCountFilter = new NullFilter<int>()
            });

            ((Success<ItemsPage<WorkQueue>>)result).Value.Items.Single().Should().Be(TestWorkQueue);
        }

        [Test]
        public async Task GetWorkQueue_CorrectlyRetrievesDataFromDatabase()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.IndexOf("from BPAWorkQueue", StringComparison.OrdinalIgnoreCase) >= 0)
                .ReturnsTable(MockTable
                    .WithColumns("id", "ident", "name", "keyfield", "running", "maxattempts", "encryptid", "processid", "snapshotconfigurationid", "processname", "resourcegroupid", "requiredFeature", "resourcegroupname", "total", "completed", "pending", "exceptioned", "totalworktime", "averageworkedtime", "groupname", "groupid")
                    .AddRow(
                        TestWorkQueue.Id,
                        TestWorkQueue.Ident,
                        TestWorkQueue.Name,
                        TestWorkQueue.KeyField,
                        TestWorkQueue.Status == QueueStatus.Running ? 1 : 0,
                        TestWorkQueue.MaxAttempts,
                        TestWorkQueue.EncryptionKeyId,
                        DBNull.Value,
                        0,
                        DBNull.Value,
                        DBNull.Value,
                        "",
                        DBNull.Value,
                        9,
                        TestWorkQueue.CompletedItemCount,
                        TestWorkQueue.PendingItemCount,
                        TestWorkQueue.ExceptionedItemCount,
                        TestWorkQueue.TotalCaseDuration.TotalSeconds,
                        TestWorkQueue.AverageWorkTime.TotalSeconds,
                        TestWorkQueue.GroupName,
                        TestWorkQueue.GroupId));

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetWorkQueue(new Guid());

            ((Success<WorkQueue>)result).Value.Should().Be(TestWorkQueue);
        }

        [Test]
        public async Task CreateWorkQueue_AddsExpectedDataToDatabase()
        {
            var hasBeenCalled = false;
            bool SetHasBeenCalled() => hasBeenCalled = true;

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().StartsWith("select", StringComparison.OrdinalIgnoreCase)
                    && cmd.CommandText.IndexOf("from BPAWorkQueue", StringComparison.OrdinalIgnoreCase) >= 0)
                .ReturnsTable(MockTable
                    .WithColumns("id", "ident", "name", "keyfield", "running", "maxattempts", "encryptid", "processid", "snapshotconfigurationid", "processname", "resourcegroupid", "requiredFeature", "resourcegroupname", "total", "completed", "pending", "exceptioned", "totalworktime", "averageworkedtime"));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().StartsWith("IF NOT Exists (SELECT 1 FROM BPAWorkQueue where name = @name)", StringComparison.OrdinalIgnoreCase)
                    && cmd.Parameters.ValueByName<Guid>("@id") == TestWorkQueue.Id
                    && cmd.Parameters.ValueByName<string>("@name") == TestWorkQueue.Name
                    && cmd.Parameters.ValueByName<string>("@keyfield") == TestWorkQueue.KeyField
                    && cmd.Parameters.ValueByName<bool>("@running") == (TestWorkQueue.Status == QueueStatus.Running)
                    && cmd.Parameters.ValueByName<int>("@maxAttempts") == TestWorkQueue.MaxAttempts
                    && cmd.Parameters.ValueByName<object>("@encryptid") == DBNull.Value
                    && cmd.Parameters.ValueByName<object>("@processid") == DBNull.Value
                    && cmd.Parameters.ValueByName<object>("@resourcegroupid") == DBNull.Value
                    && SetHasBeenCalled()
                )
                .ReturnsScalar(1);

            ConfigureFallbackForUpdateAndInsert();

            await Subject.CreateWorkQueue(TestWorkQueue);

            hasBeenCalled.Should().BeTrue();
        }
    }
}
