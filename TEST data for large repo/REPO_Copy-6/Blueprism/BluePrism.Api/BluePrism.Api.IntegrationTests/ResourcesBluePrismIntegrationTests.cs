namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Apps72.Dev.Data.DbMocker;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Services;

    [TestFixture(Category = "Integration Test")]
    public class ResourcesBluePrismIntegrationTests : BluePrismIntegrationTestBase<ResourcesService>
    {
        [TestCaseSource(nameof(ResourceTestsCasesForDisplayStatus))]
        public async Task GetResources_ReturnsExpectedResources_OnSuccess(Resource expectedResource, DateTime lastUpdated, bool includeOnlineResource, Guid userId)
        {
            var resources = new[] {expectedResource};

            if (includeOnlineResource)
            {
                resources = resources.Concat(new[]
                {
                    new Resource
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Online Resource",
                        PoolId = expectedResource.Id,
                        PoolName = "Test Pool",
                        GroupId = Guid.NewGuid(),
                        GroupName = "Test Group",
                        Attributes = ResourceAttribute.None,
                        ActiveSessionCount = 3,
                        WarningSessionCount = 4,
                        PendingSessionCount = 5,
                        DatabaseStatus = ResourceDbStatus.Ready,
                        DisplayStatus = ResourceDisplayStatus.Idle,
                    }
                }).ToArray();
            }

            SetupDatabaseMocks(resources.Select(x => (x, lastUpdated, userId)).ToArray());

            var result = await Subject.GetResources(ResourceHelper.GetTestDomainNullFiltersResourceParameters(ResourceSortBy.ActiveCountAscending, 2));

            result.Should().BeAssignableTo<Success>();
            ((Success<ItemsPage<Resource>>)result).Value.Items.Single(x => x.Id == expectedResource.Id).Should().Be(expectedResource);
        }

        [TestCaseSource(typeof(EnumTestCaseSource<ResourceSortBy>))]
        public async Task GetResources_PassesExpectedOrderByToSql(ResourceSortBy sortBy)
        {
            var expectedOrderByRegex =
                ((Server.Domain.Models.ResourceSortBy)sortBy)
                .ToString()
                .Map(typeof(Server.Domain.Models.ResourceSortBy).GetMember)
                .Single()
                .GetCustomAttributes(typeof(Server.Domain.Models.Attributes.ColumnNameSortByAttribute), false)
                .Cast<Server.Domain.Models.Attributes.ColumnNameSortByAttribute>()
                .Single()
                .Map(x =>
                    $@"order(\r|\n|\s)+by(\r|\n|\s)+\[?{Regex.Escape(x.ColumnName)}\]?(\r|\n|\s)+{x.SortDirection}")
                .Map(x => new Regex(x));

            var hasBeenCalled = false;

            SetupDatabaseMocks(c =>
            {
                if (expectedOrderByRegex.IsMatch(c.CommandText))
                    hasBeenCalled = true;
                else
                    Assert.Fail("Unexpected order by clause sent");
            });

            await Subject.GetResources(ResourceHelper.GetTestDomainNullFiltersResourceParameters(sortBy));

            hasBeenCalled.Should().BeTrue();
        }

        [Test]
        public async Task GetResources_ReturnsExpectedDisplayStatus()
        {
            var expectedResource = new Resource
            {
                Id = Guid.NewGuid(),
                Name = "Test Resource",
                PoolId = Guid.NewGuid(),
                PoolName = "Test Pool",
                GroupId = Guid.NewGuid(),
                GroupName = "Test Group",
                Attributes = ResourceAttribute.Local | ResourceAttribute.LoginAgent,
                ActiveSessionCount = 3,
                WarningSessionCount = 4,
                PendingSessionCount = 5,
                DatabaseStatus = ResourceDbStatus.Offline,
                DisplayStatus = ResourceDisplayStatus.Offline,
            };

            SetupDatabaseMocks((expectedResource, DateTime.UtcNow, TestUserId));

            var result = await Subject.GetResources(ResourceHelper.GetTestDomainNullFiltersResourceParameters(ResourceSortBy.ActiveCountAscending, 2));

            result.Should().BeAssignableTo<Success>();
            ((Success<ItemsPage<Resource>>)result).Value.Items.Single().Should().Be(expectedResource);
        }

        private static IEnumerable<TestCaseData> ResourceTestsCasesForDisplayStatus => new (Resource Resource, DateTime LastUpdated, bool IncludeOnlineResource, Guid userId) []
            {
                (GetTestResource(ResourceDbStatus.Offline, ResourceDisplayStatus.Offline), DateTime.UtcNow, false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Idle, activeSessionCount: 0), DateTime.UtcNow, false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Missing, activeSessionCount: 0), DateTime.UtcNow.AddMinutes(-2), false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Private, attributes: ResourceAttribute.Private, activeSessionCount: 0), DateTime.UtcNow, false, Guid.NewGuid()),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Idle, attributes: ResourceAttribute.Private, activeSessionCount: 0), DateTime.UtcNow, false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Working, activeSessionCount: 1), DateTime.UtcNow, false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.Warning, warningSessionCount: 1), DateTime.UtcNow, false, TestUserId),
                (GetTestResource(ResourceDbStatus.Ready, ResourceDisplayStatus.LoggedOut, attributes: ResourceAttribute.LoginAgent), DateTime.UtcNow, false, TestUserId),
            }
            .ToTestCaseData();

        private static Resource GetTestResource(ResourceDbStatus status, ResourceDisplayStatus displayStatus, ResourceAttribute attributes = ResourceAttribute.None, int activeSessionCount = 3, int warningSessionCount = 0) =>
            new Resource
            {
                Id = Guid.NewGuid(),
                Name = "Test Resource",
                PoolId = Guid.NewGuid(),
                PoolName = "Test Pool",
                GroupId = Guid.NewGuid(),
                GroupName = "Test Group",
                Attributes = attributes,
                ActiveSessionCount = activeSessionCount,
                WarningSessionCount = warningSessionCount,
                PendingSessionCount = 5,
                DatabaseStatus = status,
                DisplayStatus = displayStatus,
            };

        private void SetupDatabaseMocks(params (Resource, DateTime, Guid)[] returnedResources) =>
            SetupDatabaseMocks(_ => { }, returnedResources);

        private void SetupDatabaseMocks(Action<MockCommand> getResourcesCallback, params (Resource, DateTime, Guid)[] returnedResources)
        {
            DatabaseConnection.Mocks
                .When(x =>
                    x.HasValidSqlServerCommandText()
                    && x.CommandText.Equals("select name from sysobjects where id = object_id(@name)", StringComparison.OrdinalIgnoreCase)
                    && x.Parameters.ValueByName<string>("@name").Equals("BPAIntegerPref", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar("BPAIntegerPref");

            DatabaseConnection.Mocks
                .When(x =>
                    x.HasValidSqlServerCommandText()
                    && x.CommandText.ToLowerInvariant().Contains("from bpapref")
                    && x.Parameters.ValueByName<string>("@name").Equals("system.settings.stagewarningthreshold"))
                .ReturnsScalar(1);

            DatabaseConnection.Mocks
                .When(x =>
                    x.HasValidSqlServerCommandText()
                    && x.CommandText.ToLowerInvariant().Contains("from bpapref")
                    && x.Parameters.ValueByName<string>("@name").Equals("system.settings.appserverresourceconnection"))
                .ReturnsScalar(0);

            var mockResourcesTable =
                MockTable.WithColumns("resourceid", "name", "pool", "poolName", "groupId", "groupName", "attributeid", "actionsrunning", "warningSessions", "pendingSessions", "statusid", "lastupdated", "userid", "displayStatus");

            foreach(var (resource, lastUpdated, userId) in returnedResources)
            {
                mockResourcesTable.AddRow(
                    resource.Id,
                    resource.Name,
                    resource.PoolId,
                    resource.PoolName,
                    resource.GroupId,
                    resource.GroupName,
                    (int)resource.Attributes,
                    resource.ActiveSessionCount,
                    resource.WarningSessionCount,
                    resource.PendingSessionCount,
                    (int)resource.DatabaseStatus,
                    lastUpdated,
                    userId,
                    (int)resource.DisplayStatus
                    );
            }

            DatabaseConnection.Mocks
                .When(x =>
                {
                    var regex = new Regex(
                        @"^with(\r|\n|\s)+sessions(.|\r|\n)+from(\r|\n|\s)+bparesource",
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    return x.HasValidSqlServerCommandText() && regex.IsMatch(x.CommandText.Trim());
                })
                .ReturnsTable(c =>
                {
                    getResourcesCallback(c);
                    return mockResourcesTable;
                });

            ConfigureFallbackForUpdateAndInsert();
        }
    }
}
