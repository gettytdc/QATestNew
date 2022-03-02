namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters;
    using ControllerClients;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Models;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    using static Func.ResultHelper;

    [TestFixture]
    public class ScheduleSessionsControllerTests : ControllerTestBase<ScheduleSessionsControllerClient>
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

                    builder.Register(_ => GetMock<IScheduleStore>().Object).As<IScheduleStore>().SingleInstance();
                    return builder;
                });
            });

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsAcceptedResponseCode()
        {
            var random = new Random();
            var scheduleId = random.Next();
            var requestedStartTime = new DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromHours(5)).AddMinutes(random.Next(24 * 59));

            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(scheduleId, requestedStartTime.UtcDateTime))
                .ReturnsAsync(Succeed(requestedStartTime.UtcDateTime));

            var result = await Subject.ScheduleOneOffScheduleRun(scheduleId, new ScheduleOneOffScheduleRunModel {StartTime = requestedStartTime });

            result.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsScheduledTime()
        {
            var random = new Random();
            var scheduleId = random.Next();
            var requestedStartTime = new DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromHours(5)).AddMinutes(random.Next(24 * 59));
            var scheduledStartTime = requestedStartTime.AddMinutes(random.Next(60));

            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(scheduleId, requestedStartTime.UtcDateTime))
                .ReturnsAsync(Succeed(scheduledStartTime.UtcDateTime));

            var result = await Subject.ScheduleOneOffScheduleRun(scheduleId, new ScheduleOneOffScheduleRunModel {StartTime = requestedStartTime});

            var resultContent = JsonConvert.DeserializeObject<ScheduleOneOffScheduleRunResponseModel>(await result.Content.ReadAsStringAsync());
            resultContent.ScheduledTime.Should().Be(scheduledStartTime);
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnScheduleNotFound_ReturnsNotFoundResponseCode()
        {
            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(ResultHelper<DateTime>.Fail<ScheduleNotFoundError>());

            var result = await Subject.ScheduleOneOffScheduleRun(123, new ScheduleOneOffScheduleRunModel { StartTime = DateTimeOffset.UtcNow });

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnInvalidDateTimeFormat_ReturnsBadRequestResponse()
        {
            var result = await Subject.ScheduleOneOffScheduleRun(123, new {StartTime = "invalid"});

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
