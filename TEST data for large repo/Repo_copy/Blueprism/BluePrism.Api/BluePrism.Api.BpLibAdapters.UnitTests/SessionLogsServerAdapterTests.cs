namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.Errors;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models;
    using Utilities.Testing;
    using SessionLogsParameters = Domain.SessionLogsParameters;

    [TestFixture]
    public class SessionLogsServerAdapterTests : UnitTestBase<SessionLogsServerAdapter>
    {
        [Test]
        public async Task GetLogs_OnSuccess_ReturnsSuccess()
        {
            var sessionNumber = 1234;

            GetMock<IServer>()
                .Setup(m => m.GetLogs(sessionNumber, It.IsAny<Server.Domain.Models.SessionLogsParameters>()))
                .Returns(SessionLogsHelper.GetEmptyLogEntries);

            var result = await ClassUnderTest.GetLogs(sessionNumber, new SessionLogsParameters());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetLogs_OnSuccess_ReturnsExpectedData()
        {
            var sessionNumber = 1234;

            var testData = SessionLogsHelper.GetTestBluePrismLogEntries(3);

            GetMock<IServer>()
                .Setup(m => m.GetLogs(sessionNumber, It.IsAny<Server.Domain.Models.SessionLogsParameters>()))
                .Returns(testData);

            var expectedResult = testData.Select(x => x.ToDomainObject());
            var result = await ClassUnderTest.GetLogs(sessionNumber, new SessionLogsParameters());

            ((Success<ItemsPage<SessionLogItem>>)result).Value.Items.ShouldRuntimeTypesBeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetLogs_OnSuccess_ReturnsExpectedPagingToken()
        {
            var testLogEntries = SessionLogsHelper.GetTestBluePrismLogEntries(3, DateTimeOffset.UtcNow);

            var sessionLogsParameters = new SessionLogsParameters{ItemsPerPage = 2};

            var testPagingToken = new PagingToken<long>
            {
                PreviousIdValue = testLogEntries.OrderBy(x => x.LogId).Last().LogId,
                DataType = testLogEntries.OrderBy(x => x.LogId).Last().StartDate.GetType().Name,
                ParametersHashCode = sessionLogsParameters.GetHashCodeForValidation(),
            }.ToString();

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<Server.Domain.Models.SessionLogsParameters>()))
                .Returns(testLogEntries);

            var result = await ClassUnderTest.GetLogs(1234, sessionLogsParameters);
            var resultValue = ((Success<ItemsPage<SessionLogItem>>)result).Value.PagingToken;

            ((Some<string>)resultValue).Value.Should().Be(testPagingToken);
        }

        [Test]
        public async Task GetLogs_OnSuccessWhenNoMoreItemsLeftToReturn_ReturnsNonePagingToken()
        {
            var testLogEntries = SessionLogsHelper.GetTestBluePrismLogEntries(3, DateTimeOffset.UtcNow);

            var sessionLogsParameters = new SessionLogsParameters { ItemsPerPage = 10 };

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<Server.Domain.Models.SessionLogsParameters>()))
                .Returns(testLogEntries);

            var result = await ClassUnderTest.GetLogs(1234, sessionLogsParameters);
            var resultValue = ((Success<ItemsPage<SessionLogItem>>)result).Value.PagingToken;

            resultValue.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        public async Task GetLogs_OnPermissionException_ReturnsPermissionError()
        {
            var sessionLogsParameters = new SessionLogsParameters { ItemsPerPage = 10 };

            GetMock<IServer>()
                .Setup(m => m.GetLogs(1234, It.IsAny<Server.Domain.Models.SessionLogsParameters>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetLogs(1234, sessionLogsParameters);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetLogParameters_OnSuccess_ReturnsSuccess()
        {
            const int sessionNumber = 1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, It.IsAny<long>()))
                .Returns(string.Empty);

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetLogParameters_OnSuccess_ReturnsParameters()
        {
            const int sessionNumber = 1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, It.IsAny<long>()))
                .Returns(@"<parameters><inputs><input name=""textName"" type=""text"" value=""abc"" /></inputs></parameters>");

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            var resultItem = ((Success<SessionLogItemParameters>)result).Value.Inputs.First();
            resultItem.Key.Should().Be("textName");
            resultItem.Value.ShouldBeEquivalentTo(new DataValue
            {
                ValueType = DataValueType.Text,
                Value = "abc"
            });
        }

        [Test]
        public async Task GetLogParameters_OnSuccessWithEmptyString_ReturnsEmptyParameters()
        {
            const int sessionNumber = 1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, It.IsAny<long>()))
                .Returns(string.Empty);

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            ((Success<SessionLogItemParameters>)result).Value.Inputs.Should().BeEmpty();
            ((Success<SessionLogItemParameters>)result).Value.Outputs.Should().BeEmpty();
        }

        [Test]
        public async Task GetLogParameters_OnFailingToFindSession_ReturnsSessionNotFoundError()
        {
            const int noSessionFoundId = -1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(noSessionFoundId);

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        public async Task GetLogParameters_OnPermissionExceptionThrownAtGetSessionNumber_ReturnsPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetLogParameters_OnPermissionExceptionThrownAtGetSessionAttributeXml_ReturnsPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(1);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(It.IsAny<int>(), It.IsAny<long>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetLogParameters_OnFailingToFindLog_ReturnsLogNotFoundError()
        {
            const int sessionNumber = 1;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(sessionNumber);

            GetMock<IServer>()
                .Setup(m => m.GetSessionAttributeXml(sessionNumber, It.IsAny<long>()))
                .Returns((string)null);

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<LogNotFoundError>>();
        }
    }
}
