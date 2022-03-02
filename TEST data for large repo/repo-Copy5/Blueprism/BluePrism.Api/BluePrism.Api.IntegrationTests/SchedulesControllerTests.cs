namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using ControllerClients;
    using Domain;
    using Domain.Errors;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Mappers;
    using Models;
    using Moq;
    using NUnit.Framework;
    using Scheduling;
    using Server.Domain.Models;
    using Server.Domain.Models.DataFilters;
    using Server.Domain.Models.Pagination;
    using BpLibScheduleParameters = Server.Domain.Models.ScheduleParameters;
    using IScheduleStore = BpLibAdapters.IScheduleStore;
    using ScheduledTask = Server.Domain.Models.ScheduledTask;
    using RetirementStatus = Server.Domain.Models.RetirementStatus;
    using ScheduleLog = Server.Domain.Models.ScheduleLog;
    using ScheduleLogParameters = Server.Domain.Models.ScheduleLogParameters;
    using ScheduleLogStatus = Models.ScheduleLogStatus;

    using static Func.OptionHelper;
    using static Func.ResultHelper;
    using IntervalType = Scheduling.IntervalType;
    using NthOfMonth = Scheduling.NthOfMonth;
    using ScheduledSession = Server.Domain.Models.ScheduledSession;

    [TestFixture]
    public class SchedulesControllerTests : ControllerTestBase<SchedulesControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object);

                    builder.Register(_ => GetMock<BpLibAdapters.IScheduleStore>().Object).As<BpLibAdapters.IScheduleStore>().SingleInstance();
                    return builder;
                });
            });

        [Test]
        public async Task GetSchedules_ShouldReturnSchedules_WhenSuccessful()
        {
            var schedules = new[]
            {
                new ScheduleSummary
                {
                    Id = 1,
                },
            };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(schedules);

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters())
                .Map(x => x.Content.ReadAsAsync<ItemsPage<ScheduleModel>>());

            var resultScheduleId = result.Items.Select(x => x.Id);
            var expectedScheduleId = schedules.Select(x => x.Id);

            resultScheduleId.Should().BeEquivalentTo(expectedScheduleId);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusOK_WhenNoSchedules()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(Array.Empty<ScheduleSummary>());

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));


            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusBadRequest_WhenInvalidFilterSupplied()
        {
            var result = await Subject.GetSchedulesUsingQueryString("retirementStatus=qwer");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusOK_WhenEmptyFiltersSupplied()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(Array.Empty<ScheduleSummary>());

            var result = await Subject.GetSchedulesUsingQueryString("name.strtw=&retirementStatus=");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSchedules_WithCommaSeparatedStatuses_AppliesAllRetiredFilters()
        {
            var passedParameters = default(BpLibScheduleParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Callback((BpLibScheduleParameters x) => passedParameters = x)
                .Returns(new ScheduleSummary[0]);

            var result = await Subject.GetSchedulesUsingQueryString("retirementStatus=active, retired");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<RetirementStatus>)passedParameters.RetirementStatus)
                .OfType<EqualsDataFilter<RetirementStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { RetirementStatus.Active, RetirementStatus.Retired });
        }

        [Test]
        public async Task GetSchedules_WithMultipleSeparateStatuses_AppliesAllRetiredFilters()
        {
            var passedParameters = default(BpLibScheduleParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Callback((BpLibScheduleParameters x) => passedParameters = x)
                .Returns(new ScheduleSummary[0]);

            var result = await Subject.GetSchedulesUsingQueryString("retirementStatus=active&retirementStatus=retired");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<RetirementStatus>)passedParameters.RetirementStatus)
                .OfType<EqualsDataFilter<RetirementStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { RetirementStatus.Active, RetirementStatus.Retired });
        }

        [Test]
        public async Task GetSchedules_WithFullPathSpecifications_AppliesAllRetiredFilters()
        {
            var passedParameters = default(BpLibScheduleParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Callback((BpLibScheduleParameters x) => passedParameters = x)
                .Returns(new ScheduleSummary[0]);

            var result = await Subject.GetSchedulesUsingQueryString("scheduleParameters.retirementStatus=active, retired");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<RetirementStatus>)passedParameters.RetirementStatus)
                .OfType<EqualsDataFilter<RetirementStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { RetirementStatus.Active, RetirementStatus.Retired });
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenSchedulesHasNoItems()
        {
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(Array.Empty<ScheduleSummary>());

            var result = await Subject.GetSchedulesWithParameters(new Models.ScheduleParameters());

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var sessionLogsPage = await result.Map(x => x.Content.ReadAsAsync<ItemsPageModel<ScheduleModel>>());
            sessionLogsPage.Items.Should().BeEmpty();
            sessionLogsPage.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusCodeOK_WhenSuccessful()
        {
            const int scheduleId = 2;
            var schedule = new ScheduleSummary { Id = scheduleId };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Returns(schedule);

            var result = await Subject.GetScheduleByScheduleId(scheduleId);
            
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnSchedule_WhenSuccessful()
        {
            const int scheduleId = 2;

            var schedule = new ScheduleSummary {
                Id = scheduleId,
                Name = "test name",
                Description = "test description",
                InitialTaskId = 8,
                IntervalType = IntervalType.Minute,
                CalendarId = 2,
                CalendarName = "test name",
                DayOfMonth = NthOfMonth.Fourth,
                DayOfWeek = DaySet.FullWeek,
                EndDate = new DateTimeOffset(new DateTime(2021,04,29,15,44,12)),
                EndPoint = new DateTimeOffset(new DateTime(2021, 04, 29, 18, 05, 35)),
                IsRetired = false,
                StartDate = new DateTimeOffset(new DateTime(2021, 04, 29, 12, 16, 44)),
                StartPoint = new DateTimeOffset(new DateTime(2021, 04, 29, 13, 10, 22)),
                TasksCount = 4,
                TimePeriod = 3,
            };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Returns(schedule);

            var result = await Subject.GetScheduleByScheduleId(scheduleId).Map(x => x.Content.ReadAsAsync<ScheduleModel>());

            var expectedSchedule = schedule.ToDomainObject().ToModelObject();

            result.ShouldBeEquivalentTo(expectedSchedule);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusNotFound_WhenScheduleNotFound()
        {
            const int scheduleId = 2;

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Throws(new NoSuchScheduleException(7));

            var result = await Subject.GetScheduleByScheduleId(scheduleId);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetScheduleByScheduleId(5);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetScheduleByScheduleId(5);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(5))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetScheduleByScheduleId(5);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            const int scheduleId = 2;

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetScheduleByScheduleId(scheduleId);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetScheduleLogs_WithCommaSeparatedScheduleLogStatus_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString("scheduleLogStatus=pending, completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [Test]
        public async Task GetScheduleLogsWithId_WithCommaSeparatedScheduleLogStatus_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(1, "scheduleLogStatus=pending, completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [Test]
        public async Task GetScheduleLogs_WithMultipleSeparateScheduleLogStatus_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString("scheduleLogStatus=pending&scheduleLogStatus=completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [Test]
        public async Task GetScheduleLogsWithId_WithMultipleSeparateScheduleLogStatus_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(1, "scheduleLogStatus=pending&scheduleLogStatus=completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [Test]
        public async Task GetScheduleLogs_WithFullPathSpecifications_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString("scheduleLogParameters.scheduleLogStatus=pending, completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [Test]
        public async Task GetScheduleLogsWithId_WithFullPathSpecifications_AppliesAllStatusFilters()
        {
            var passedParameters = default(ScheduleLogParameters);

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Callback((ScheduleLogParameters x) => passedParameters = x)
                .Returns(new ScheduleLog[0]);

            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(1, "scheduleLogParameters.scheduleLogStatus=pending, completed");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ItemStatus>)passedParameters.ScheduleLogStatus)
                .OfType<EqualsDataFilter<ItemStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { ItemStatus.Pending, ItemStatus.Completed });
        }

        [TestCase("-5")]
        [TestCase("5.5")]
        [TestCase("testStatus")]
        public async Task GetScheduleLogs_ShouldReturnBadRequest_WhenInvalidScheduleLogStatusProvided(string itemsPerPage)
        {
            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString($"scheduleLogParameters.scheduleLogStatus={itemsPerPage}, completed");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCase("-5")]
        [TestCase("5.5")]
        [TestCase("testStatus")]
        public async Task GetScheduleLogsWithId_ShouldReturnBadRequest_WhenInvalidScheduleLogStatusProvided(string itemsPerPage)
        {
            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(1, $"scheduleLogParameters.scheduleLogStatus={itemsPerPage}, completed");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnSchedulesWithPagingToken_WhenSuccessful()
        {
            var scheduleSummaries = SchedulesHelper.GetTestBluePrismScheduleSummary(3, DateTimeOffset.UtcNow).ToList();

            var lastItem = scheduleSummaries.OrderBy(x => x.Id).Last();

            var scheduleParameters = new Models.ScheduleParameters { ItemsPerPage = 2 };

            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                DataType = lastItem.StartDate.GetType().Name,
                ParametersHashCode = scheduleParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            scheduleParameters.PagingToken = testPagingToken.ToString();

            var testSchedules = scheduleSummaries.Select(x => x.ToDomainObject().ToModelObject()).ToList();
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(scheduleSummaries);

            var result = await Subject.GetSchedulesWithParameters(scheduleParameters)
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<ScheduleModel>>());

            result.Items.ShouldBeEquivalentTo(testSchedules);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

        [Test]
        public async Task GetSchedules_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
        {
            var scheduleSummaries = SchedulesHelper.GetTestBluePrismScheduleSummary(3);

            var lastItem = scheduleSummaries.OrderBy(x => x.Name).Last();

            var initialScheduleParameters = SchedulesHelper.GetTestDomainScheduleParameters(3);

            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                DataType = lastItem.StartDate.GetType().Name,
                ParametersHashCode = initialScheduleParameters.GetHashCodeForValidation(),
            };

            var testScheduleParameters = new Models.ScheduleParameters()
            {
                ItemsPerPage = 3,
                Name = new StartsWithStringFilterModel { Eq = "newvalue" },
                PagingToken = testPagingToken.ToString()
            };

            var result = await Subject.GetSchedulesWithParameters(testScheduleParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnHttpStatusOk_WhenUsingPagingTokenAndCommaDelimitedCollectionFilter()
        {
            var scheduleSummaries = SchedulesHelper.GetTestBluePrismScheduleSummary(1).ToList();

            var initialItem = scheduleSummaries.First();

            var scheduleParameters = SchedulesHelper.GetTestDomainScheduleParameters(1);

            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = initialItem.Name,
                DataType = initialItem.StartDate.GetType().Name,
                ParametersHashCode = scheduleParameters.GetHashCodeForValidation(),
            };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(scheduleSummaries);

            var result = await Subject.GetSchedulesUsingQueryString($"itemsPerPage=1&name.strtw=test&pagingToken={testPagingToken}&retirementStatus=Active,Retired");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnErrorMessage_WhenRetirementStatusIsInvalid()
        {
            var statuses = "active, abc";
            var result = await Subject.GetSchedulesUsingQueryString($"scheduleParameters.retirementStatus={statuses}");

            var content = await result.Map(x => x.Content.ReadAsAsync<IEnumerable<ValidationErrorModel>>());

            var validationModel = content.First();
            validationModel.InvalidField.Should().Be("RetirementStatus");
            validationModel.Message.Should().Be($"The value '{statuses}' is not valid for RetirementStatus");
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task GetSchedules_ShouldReturnBadRequest_WhenItemsPerPageIsBelowOne(int value)
        {
            var scheduleParameters = new Models.ScheduleParameters { ItemsPerPage = value };

            var result = await Subject.GetSchedulesWithParameters(scheduleParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnBadRequest_WhenItemsPerPageIsADecimal()
        {
            var result = await Subject.GetSchedulesUsingQueryString("itemsPerPage=1.5");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSchedules_ShouldReturnBadRequestWithInvalidFields_WhenInvalidInputsSupplied()
        {
            var result = await Subject.GetSchedulesUsingQueryString("itemsPerPage=1001&retirementStatus=abc&pagingToken=£$£$%$£^%&%NFGHDZS3214242");
            var content = await result.Map(x => x.Content.ReadAsAsync<IEnumerable<ValidationErrorModel>>());

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content
                .Select(x => x.InvalidField)
                .Should()
                .Contain(new[] { "ItemsPerPage", "PagingToken", "RetirementStatus" });
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnHttpStatusNoContent_WhenUpdateScheduleRetirementStatusSuccessful()
        {
            var scheduleId = 1;
            var updateSchedule = new UpdateScheduleModel { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Succeed());

            var result = await Subject.UpdateSchedule(scheduleId.ToString(), updateSchedule);

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            var scheduleId = 1;
            var updateSchedule = new UpdateScheduleModel { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.UpdateSchedule(scheduleId.ToString(), updateSchedule);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnHttpStatusNotFound_WhenScheduleNotFound()
        {
            var scheduleId = 1;
            var updateSchedule = new UpdateScheduleModel { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleNotFoundError>());

            var result = await Subject.UpdateSchedule(scheduleId.ToString(), updateSchedule);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnHttpStatusConflict_WhenScheduleAlreadyRetired()
        {
            var scheduleId = 1;
            var updateSchedule = new UpdateScheduleModel { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleAlreadyRetiredError>());

            var result = await Subject.UpdateSchedule(scheduleId.ToString(), updateSchedule);

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnHttpStatusConflict_WhenScheduleAlreadyNotRetired()
        {
            var scheduleId = 1;
            var updateSchedule = new UpdateScheduleModel { IsRetired = false };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleNotRetiredError>());

            var result = await Subject.UpdateSchedule(scheduleId.ToString(), updateSchedule);

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnBadRequest_WhenInvalidParameterSupplied()
        {
            var result = await Subject.UpdateSchedule("a", new UpdateScheduleModel());
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateSchedule_ShouldReturnBadRequestWithInvalidFields_WhenInvalidInputsSupplied()
        {
            var result = await Subject.UpdateSchedule("1", new { IsRetired = "INVALID_TYPE" });
            var content = await result.Map(x => x.Content.ReadAsAsync<IEnumerable<ValidationErrorModel>>());

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content
                .Select(x => x.InvalidField)
                .Should()
                .Contain(new[] { "scheduleChanges.IsRetired" });
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnScheduleLogs_WhenSuccessful()
        {
            var scheduleLogs = GetTestScheduleLogs();

            var scheduleLogModels = scheduleLogs.Select(x => x.ToDomainObject().ToModelObject()).ToList();

            var expected = new ItemsPage<ScheduleLogModel> { Items = scheduleLogModels };

            GetMock<IServer>()
                 .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                 .Returns(scheduleLogs);

            var result = await Subject.GetScheduleLogsByScheduleId(12, 4)
                .Map(x => x.Content.ReadAsAsync<ItemsPage<ScheduleLogModel>>());

            result.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusOK_WhenNoScheduleLogs()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnHttpStatusNotFound_WhenScheduleDoesNotExists()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Throws(new NoSuchScheduleException(7));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 2);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase("INVALID_TYPE", "5")]
        [TestCase("7", "-5")]
        [TestCase("7", "0")]
        [TestCase("7", "5.5")]
        [TestCase("5", "9999999")]
        public async Task GetScheduleLogsByScheduleId_ShouldReturnBadRequest_WhenInvalidParametersProvided(string scheduleId, string itemsPerPage)
        {
            var result = await Subject.GetScheduleLogsByScheduleId(scheduleId, itemsPerPage);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCaseSource(nameof(ScheduleLogsDateTimeFilterParameters))]
        public async Task GetScheduleLogsByScheduleId_OnInvalidStartTimeOrEndTime_ShouldReturnBadRequestStatusCode(string timeProperty)
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(12, $"scheduleLogParameters.{timeProperty}=Test");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCaseSource(nameof(ScheduleLogsValidDateTimeParameterInputs))]
        public async Task GetScheduleLogsByScheduleId_OnValidStartTimeOrEndTime_ShouldReturnHttpStatusOK(string timeProperty, string timeValue)
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(12, $"scheduleLogParameters.{timeProperty}={timeValue}");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnScheduleLogs_WhenSuccessful()
        {
            var startTime1 = new DateTime(2021, 02, 01, 15, 45, 12, 242);
            var startTime2 = new DateTime(2021, 02, 02, 09, 12, 42, 457);
            var startTime3 = new DateTime(2021, 02, 02, 08, 47, 01, 123);

            var endTime1 = new DateTime(2021, 02, 01, 18, 02, 04, 787);
            var endTime2 = new DateTime(2021, 02, 02, 10, 15, 16, 678);

            var scheduleLogs = new[]
            {
                new ScheduleLog
                {
                    ScheduleId = 1,
                    ScheduleName = "Test Schedule Name 1",
                    ServerName = "Test Server Name 1",
                    ScheduleLogId = 1,
                    Status = ItemStatus.Completed,
                    StartTime = Some(startTime1),
                    EndTime = Some(endTime1),
                },
                new ScheduleLog
                {
                    ScheduleId = 2,
                    ScheduleName = "Test Schedule Name 2",
                    ServerName = "Test Server Name 2",
                    ScheduleLogId = 2,
                    Status = ItemStatus.Failed,
                    StartTime = Some(startTime2),
                    EndTime = Some(endTime2),
                },
                new ScheduleLog
                {
                    ScheduleId = 3,
                    ScheduleName = "Test Schedule Name 3",
                    ServerName = "Test Server Name 3",
                    ScheduleLogId = 3,
                    Status = ItemStatus.Running,
                    StartTime = Some(startTime3),
                    EndTime = None<DateTime>(),
                },
            };

            var scheduleLogModels = new[]
            {
                new ScheduleLogModel
                {
                    ScheduleId = 1,
                    ScheduleName = "Test Schedule Name 1",
                    ServerName = "Test Server Name 1",
                    ScheduleLogId = 1,
                    Status = ScheduleLogStatus.Completed,
                    StartTime = new DateTimeOffset(startTime1, TimeSpan.Zero),
                    EndTime =  new DateTimeOffset(endTime1, TimeSpan.Zero),
                },
                new ScheduleLogModel
                {
                    ScheduleId = 2,
                    ScheduleName = "Test Schedule Name 2",
                    ServerName = "Test Server Name 2",
                    ScheduleLogId = 2,
                    Status = ScheduleLogStatus.Terminated,
                    StartTime = new DateTimeOffset(startTime2, TimeSpan.Zero),
                    EndTime =  new DateTimeOffset(endTime2, TimeSpan.Zero),
                },
                new ScheduleLogModel
                {
                    ScheduleId = 3,
                    ScheduleName = "Test Schedule Name 3",
                    ServerName = "Test Server Name 3",
                    ScheduleLogId = 3,
                    Status = ScheduleLogStatus.Running,
                    StartTime = new DateTimeOffset(startTime3, TimeSpan.Zero),
                    EndTime = null,
                },
            };

            var expected = new ItemsPage<ScheduleLogModel> { Items = scheduleLogModels };

            GetMock<IServer>()
                 .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                 .Returns(scheduleLogs);

            var result = await Subject.GetScheduleLogs(4)
                .Map(x => x.Content.ReadAsAsync<ItemsPage<ScheduleLogModel>>());

            result.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusOK_WhenNoScheduleLogs()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusNotFound_WhenScheduleDoesNotExists()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Throws(new NoSuchScheduleException(7));

            var result = await Subject.GetScheduleLogsByScheduleId(7, 10);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase("-5")]
        [TestCase("0")]
        [TestCase("5.5")]
        [TestCase("9999999")]
        public async Task GetScheduleLogs_ShouldReturnBadRequest_WhenInvalidParametersProvided(string itemsPerPage)
        {
            var result = await Subject.GetScheduleLogs(itemsPerPage);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCaseSource(nameof(ScheduleLogsDateTimeFilterParameters))]
        public async Task GetScheduleLogs_OnInvalidStartTimeOrEndTime_ShouldReturnBadRequestStatusCode(string timeProperty)
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString($"scheduleLogParameters.{timeProperty}=Test");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCaseSource(nameof(ScheduleLogsValidDateTimeParameterInputs))]
        public async Task GetScheduleLogs_OnValidStartTimeOrEndTime_ShouldReturnHttpStatusOK(string timeProperty, string timeValue)
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString($"scheduleLogParameters.{timeProperty}={timeValue}");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenScheduleHasNoLogs()
        {
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(Array.Empty<ScheduleLog>());

            var result = await Subject.GetScheduleLogs(10);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var scheduleLogsPage = await result.Map(x => x.Content.ReadAsAsync<ItemsPageModel<ScheduleLogModel>>());

            scheduleLogsPage.Items.Should().BeEmpty();
            scheduleLogsPage.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnSchedulesWithPagingToken_WhenSuccessful()
        {
            var scheduleLogs = GetTestScheduleLogs();

            var orderedScheduleLogs = scheduleLogs.OrderByDescending(x => x.StartTime is Some<DateTime> dt ? dt.Value : DateTime.MinValue).ToList();
            var lastItem = orderedScheduleLogs.Last();

            var scheduleLogsParameters = new Models.ScheduleLogParameters { ItemsPerPage = 3 };

            var testPagingToken = new PagingToken<int>
            {
                PreviousIdValue = lastItem.ScheduleLogId,
                DataType = lastItem.StartTime.GetTypeName(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItem.StartTime),
                ParametersHashCode = scheduleLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            scheduleLogsParameters.PagingToken = testPagingToken.ToString();

            var testSchedules = scheduleLogs.Select(x => x.ToDomainObject().ToModelObject()).ToList();
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetCurrentAndPassedLogs(It.IsAny<ScheduleLogParameters>()))
                .Returns(orderedScheduleLogs);

            var result = await Subject.GetScheduleLogs(3)
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<ScheduleLogModel>>());

            result.Items.ShouldBeEquivalentTo(testSchedules);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
        {
            var scheduleLogs = GetTestScheduleLogs();

            var orderedScheduleLogs = scheduleLogs.OrderByDescending(x => x.StartTime is Some<DateTime> dt ? dt.Value : DateTime.MinValue).ToList();
            var lastItem = orderedScheduleLogs.Last();

            var initialDateTimeFilterValue = new DateTime(2022, 05, 23, 10, 14, 12);

            var initialScheduleParameters = new Models.ScheduleLogParameters()
            {
                ItemsPerPage = 3,
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(initialDateTimeFilterValue) },
            };

            var testPagingToken = new PagingToken<int>
            {
                PreviousIdValue = lastItem.ScheduleLogId,
                DataType = lastItem.ScheduleLogId.GetTypeName(),
                ParametersHashCode = initialScheduleParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            initialScheduleParameters.PagingToken = testPagingToken.ToString();

            var testScheduleParameters = new Models.ScheduleLogParameters()
            {
                ItemsPerPage = 3,
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(initialDateTimeFilterValue.AddHours(2)) },
                PagingToken = testPagingToken.ToString(),
            };

            var result = await Subject.GetScheduleLogsWithParameters(testScheduleParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnBadRequestWithInvalidFields_WhenInvalidInputsSupplied()
        {
            var result = await Subject.GetScheduleLogsUsingScheduleIdAndQueryString("itemsPerPage=1001&pagingToken=£$£$%$£^%&%NFGHDZS3214242");
            var content = await result.Map(x => x.Content.ReadAsAsync<IEnumerable<ValidationErrorModel>>());

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content
                .Select(x => x.InvalidField)
                .Should()
                .Contain(new[] { "ItemsPerPage", "PagingToken" });
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(new List<ScheduledTask>());
            var result = await Subject.GetScheduledTasks(1);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnScheduledTasks_WhenSuccessful()
        {
            var scheduledTasks = new[]
            {
                 new ScheduledTask
                 {
                     Id = 1,
                     Name = "name",
                     Description = "desc",
                     DelayAfterEnd = 1,
                     FailFastOnError = true,
                     OnSuccessTaskId = 1,
                     OnSuccessTaskName = "name",
                     OnFailureTaskId = 1,
                     OnFailureTaskName = "name"
                 },
             };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(scheduledTasks);

            var result = await Subject.GetScheduledTasks(1)
                .Map(x => x.Content.ReadAsAsync<IEnumerable<ScheduledTaskModel>>());

            var expected = scheduledTasks.Select(x => x.ToDomainModel().ToModelObject());

            result.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnEmptyList_WhenScheduleHaveNoTasks()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(1))
                .Returns(new List<ScheduledTask>());

            var result = await Subject.GetScheduledTasks(1)
                .Map(x => x.Content.ReadAsAsync<IEnumerable<ScheduledTaskModel>>());

            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnHttpStatusNotFound_WhenScheduleDoesNotExist()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws(new NoSuchScheduleException(1));
            var result = await Subject.GetScheduledTasks(1);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnHttpStatusNotFound_WhenScheduleHasBeenDeleted()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws(new DeletedScheduleException(1));
            var result = await Subject.GetScheduledTasks(1);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetScheduledTasks(1);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetScheduledTasks(1);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(Array.Empty<ScheduledSession>());
            var result = await Subject.GetScheduledSessions(1);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnScheduledSessions_WhenSuccessful()
        {
            var scheduledSessions = new[]
            {
                 new ScheduledSession
                 {
                     ProcessName = "",
                     ResourceName = ""
                 }
            };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(scheduledSessions);

            var result = await Subject.GetScheduledSessions(1)
                .Map(x => x.Content.ReadAsAsync<IEnumerable<ScheduledSessionModel>>());

            var expected = scheduledSessions.Select(x => x.ToDomainModel());

            result.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnEmptyList_WhenTaskHaveNoSessions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(1))
                .Returns(Array.Empty<ScheduledSession>());

            var result = await Subject.GetScheduledSessions(1)
                .Map(x => x.Content.ReadAsAsync<IEnumerable<ScheduledSessionModel>>());

            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnHttpStatusNotFound_WhenScheduleHasBeenDeleted()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws(new DeletedScheduleException(1));
            var result = await Subject.GetScheduledSessions(1);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnHttpStatusNotFound_WhenTaskDoesNotExist()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws(new NoSuchTaskException(1));
            var result = await Subject.GetScheduledSessions(1);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetScheduledSessions(1);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetScheduledSessions(1);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private static ScheduleLog[] GetTestScheduleLogs()
        {
            var startTime1 = new DateTime(2021, 02, 01, 15, 45, 12, 242);
            var startTime2 = new DateTime(2021, 02, 02, 09, 12, 42, 457);
            var startTime3 = new DateTime(2021, 02, 02, 08, 47, 01, 123);

            var endTime1 = new DateTime(2021, 02, 01, 18, 02, 04, 787);
            var endTime2 = new DateTime(2021, 02, 02, 10, 15, 16, 678);

            var scheduleLogs = new[]
            {
                new ScheduleLog
                {
                    ScheduleId = 1,
                    ScheduleName = "Test Schedule Name 1",
                    ServerName = "Test Server Name 1",
                    ScheduleLogId = 1,
                    Status = ItemStatus.Completed,
                    StartTime = Some(startTime1),
                    EndTime = Some(endTime1),
                },
                new ScheduleLog
                {
                    ScheduleId = 2,
                    ScheduleName = "Test Schedule Name 2",
                    ServerName = "Test Server Name 2",
                    ScheduleLogId = 2,
                    Status = ItemStatus.Failed,
                    StartTime = Some(startTime2),
                    EndTime = Some(endTime2),
                },
                new ScheduleLog
                {
                    ScheduleId = 3,
                    ScheduleName = "Test Schedule Name 3",
                    ServerName = "Test Server Name 3",
                    ScheduleLogId = 3,
                    Status = ItemStatus.Running,
                    StartTime = Some(startTime3),
                    EndTime = None<DateTime>(),
                },
            };
            return scheduleLogs;
        }

        private static IEnumerable<TestCaseData> ScheduleLogsDateTimeFilterParameters() =>
            new[]
            {
                "StartTime.gte",
                "StartTime.lte",
                "StartTime.eq",
                "EndTime.gte",
                "EndTime.lte",
                "EndTime.eq"
            }.ToTestCaseData();

        private static IEnumerable<TestCaseData> ScheduleLogsValidDateTimeParameterInputs => new[]
        {
            ("StartTime.gte", "2021-02-24T11:41:01.047"),
            ("StartTime.lte", "2021-02-24T11:41:01.047"),
            ("StartTime.eq", "2021-02-24T11:41:01.047"),
            ("EndTime.gte", "2021-02-24T11:41:01.047"),
            ("EndTime.lte", "2021-02-24T11:41:01.047"),
            ("EndTime.eq", "2021-02-24T11:41:01.047"),
            ("StartTime.gte", ""),
            ("StartTime.lte", ""),
            ("StartTime.eq", ""),
            ("EndTime.gte", ""),
            ("EndTime.lte", ""),
            ("EndTime.lte", ""),
            ("EndTime.eq", ""),
        }.ToTestCaseData();
    }
}
