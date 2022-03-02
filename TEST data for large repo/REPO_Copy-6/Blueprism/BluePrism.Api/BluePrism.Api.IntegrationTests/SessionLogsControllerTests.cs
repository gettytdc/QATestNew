namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters;
    using BpLibAdapters.Extensions;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using ControllerClients;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Mappers;
    using Models;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models;
    using SessionLogsParameters = Server.Domain.Models.SessionLogsParameters;

    [TestFixture]
    public class SessionLogsControllerTests : ControllerTestBase<SessionLogsControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object).As<IBluePrismServerFactory>();
                    return builder;
                });
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);
            });

        [Test]
        public async Task GetSessionLogs_OnSuccess_ReturnsOkStatusCode()
        {
            var sessionId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1234);

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .Returns(SessionLogsHelper.GetTestBluePrismLogEntries(1, DateTime.UtcNow.AddDays(1).AddHours(1).ToDateTimeOffset()));

            var result = await Subject.GetSessionLogs(sessionId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSessionLogs_OnSuccess_ReturnsSessionLogs()
        {
            var sessionId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1).AddHours(1).ToDateTimeOffset();
            var testLogEntries = SessionLogsHelper.GetTestBluePrismLogEntries(3, startTime);

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1234);

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .Returns(testLogEntries);

            var result = await Subject.GetSessionLogs(sessionId)
                .Map(x => x.Content.ReadAsAsync<Models.ItemsPageModel<SessionLogItemModel>>());

            result.Items.Count().Should().Be(3);

            var expectedItem = testLogEntries.First().ToDomainObject().ToModel();
            var item = result.Items.First();

            item.LogId.Should().Be(expectedItem.LogId);
            item.StageName.Should().Be(expectedItem.StageName);
            item.StageType.Should().Be(expectedItem.StageType);
            item.ResourceStartTime?.ToString("yyyy-MM-dd hh:mm:ss").Should().Be(expectedItem.ResourceStartTime?.ToString("yyyy-MM-dd hh:mm:ss"));
            item.Result.Should().Be(expectedItem.Result);
            item.HasParameters.Should().Be(expectedItem.HasParameters);
        }

        [Test]
        public async Task GetSessionLogs_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenSessionLogsHasNoItems()
        {
            var sessionId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1234);

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .Returns(Array.Empty<clsSessionLogEntry>());

            var result = await Subject.GetSessionLogs(sessionId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var sessionLogsPage = await result.Map(x => x.Content.ReadAsAsync<Models.ItemsPageModel<SessionLogItemModel>>());
            sessionLogsPage.Items.Should().BeEmpty();
            sessionLogsPage.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task GetSessionLogs_ShouldReturnSessionLogsWithPagingToken_WhenSuccessful()
        {
            var sessionLogEntries = SessionLogsHelper.GetTestBluePrismLogEntries(3, DateTimeOffset.UtcNow);

            var lastItem = sessionLogEntries.OrderBy(x => x.LogId).Last();

            var sessionLogsParameters = new Models.SessionLogsParameters { ItemsPerPage = 2 };

            var testPagingToken = new PagingToken<long>
            {
                PreviousIdValue = lastItem.LogId,
                DataType = lastItem.StartDate.GetType().Name,
                ParametersHashCode = sessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            sessionLogsParameters.PagingToken = testPagingToken.ToString();

            var testQueueItems = sessionLogEntries.Select(x => x.ToDomainObject().ToModel()).ToList();
            GetMock<IServer>()
                .Setup(m => m.GetLogs(It.IsAny<int>(), It.IsAny<SessionLogsParameters>()))
                .Returns(sessionLogEntries);

            var result = await Subject.GetSessionLogsWithParameters(Guid.Empty, sessionLogsParameters)
                .Map(x => x.Content.ReadAsAsync<Models.ItemsPageModel<SessionLogItemModel>>());

            result.Items.ShouldBeEquivalentTo(testQueueItems);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

         [Test]
         public async Task GetSessionLogs_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
         {
             var logEntries = SessionLogsHelper.GetTestBluePrismLogEntries(3);

             var lastItem = logEntries.OrderBy(x => x.LogId).Last();

             var initialSessionLogsParameters = new Models.SessionLogsParameters { ItemsPerPage = 2 };

             var testPagingToken = new PagingToken<long>
             {
                 PreviousIdValue = lastItem.LogId,
                 DataType = lastItem.LogId.GetType().Name,
                 ParametersHashCode = initialSessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
             };

             var testSessionLogsParameters = new Models.SessionLogsParameters()
             {
                 ItemsPerPage = 3, PagingToken = testPagingToken.ToString()
             };

             var result = await Subject.GetSessionLogsWithParameters(Guid.Empty, testSessionLogsParameters);
             result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
         }

        [Test]
        public async Task GetSessionLogs_OnMissingSession_ReturnsNotFoundStatusCode()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(-1);

            var result = await Subject.GetSessionLogs(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetSessionLogs_OnPermissionDenied_ReturnsForbiddenStatusCode()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(1);

            GetMock<IServer>()
                .Setup(m => m.GetLogs(It.IsAny<int>(), It.IsAny<SessionLogsParameters>()))
                .Throws<PermissionException>();

            var result = await Subject.GetSessionLogs(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetSessionLogParameters_OnSuccess_ShouldReturnHttpStatus200()
        {
            var sessionId = Guid.NewGuid();
            var logId = 1234;

            var sessionNumber = 1;
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, logId))
                .Returns(string.Empty);

            var result = await Subject.GetSessionLogParameters(sessionId, logId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSessionLogParameters_OnSuccess_ReturnsSessionLogParameters()
        {
            var sessionId = Guid.NewGuid();
            var logId = 1234;
            var sessionNumber = 1;
            var xml = $@"<parameters><inputs><input name=""test"" type=""text"" value=""abc"" /></inputs><outputs /></parameters>";

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, logId))
                .Returns(xml);

            var result = await Subject.GetSessionLogParameters(sessionId, logId)
                .Map(x => x.Content.ReadAsAsync<Models.SessionLogParametersModel>());

            var expectedResult = xml.ToSessionLogItemParameters().ToModel();

            result.Inputs.ShouldBeEquivalentTo(expectedResult.Inputs);
            result.Outputs.ShouldBeEquivalentTo(expectedResult.Outputs);
        }

        [Test]
        public async Task GetSessionLogParameters_OnMissingSession_ReturnsNotFoundStatusCode()
        {
            var sessionId = Guid.NewGuid();
            var logId = 1234;
            var sessionNumber = -1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, logId))
                .Returns(string.Empty);

            var result = await Subject.GetSessionLogParameters(sessionId, logId);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetSessionLogParameters_OnMissingLog_ReturnsNotFoundStatusCode()
        {
            var sessionId = Guid.NewGuid();
            var logId = 1234;
            var sessionNumber = 1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, logId))
                .Returns((string)null);

            var result = await Subject.GetSessionLogParameters(sessionId, logId);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetSessionLogParameters_WhenNoPermissionToGetSessionNumber_ReturnsForbiddenStatusCode()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Throws<PermissionException>();

            var result = await Subject.GetSessionLogParameters(Guid.NewGuid(), 1);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetSessionLogParameters_WhenNoPermissionToGetSessionLogs_ReturnsForbiddenStatusCode()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(1);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(It.IsAny<int>(), It.IsAny<long>()))
                .Throws<PermissionException>();

            var result = await Subject.GetSessionLogParameters(Guid.NewGuid(), 1);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
