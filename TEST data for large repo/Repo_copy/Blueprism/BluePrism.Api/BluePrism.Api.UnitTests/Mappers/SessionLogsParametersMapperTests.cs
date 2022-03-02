namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;

    using static Func.OptionHelper;

    [TestFixture]
    public class SessionLogsParametersMapperTests
    {
        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedData_WhenCalled()
        {
            var sessionLogsParametersDomain = new Domain.SessionLogsParameters
            {
                ItemsPerPage = 10,
                PagingToken = None<PagingToken<long>>()
            };
            var sessionLogsParametersModel = new SessionLogsParameters { ItemsPerPage = 10 };

            var result = sessionLogsParametersModel.ToDomainObject();

            result.ShouldBeEquivalentTo(sessionLogsParametersDomain);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithNullPagingToken()
        {
            var sessionLogsParameters = new SessionLogsParameters
            {
                ItemsPerPage = 5,
                PagingToken = null,
            };

            var expected = new Domain.SessionLogsParameters()
            {
                ItemsPerPage = 5,
                PagingToken = None<PagingToken<long>>(),
            };

            var actual = sessionLogsParameters.ToDomainObject();

            actual.PagingToken.Should().Be(expected.PagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithPagingToken()
        {
            var sessionLogsParameters = new SessionLogsParameters { ItemsPerPage = 7 };

            var domainPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = "2021-02-19T11:12:58.1400000+00:00",
                PreviousIdValue = 75,
                ParametersHashCode = sessionLogsParameters.ToDomainObject().GetHashCodeForValidation()
            };
            var expected = new Domain.SessionLogsParameters
            {
                ItemsPerPage = 7,
                PagingToken = Some(domainPagingToken),
            };

            sessionLogsParameters.PagingToken = domainPagingToken.ToString();

            var actual = sessionLogsParameters.ToDomainObject();

            ((Some<PagingToken<long>>)actual.PagingToken).Value
                .ShouldRuntimeTypesBeEquivalentTo(((Some<PagingToken<long>>)expected.PagingToken).Value);
        }
        [Test]
        public void ToDomainObject_ShouldReturnSamePagingToken_WhenParametersNotChanged()
        {
            var originalSessionLogsParameters = new SessionLogsParameters {ItemsPerPage = 10};
            var updateSessionLogsParameters = new SessionLogsParameters {ItemsPerPage = 10};

            var originalPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = 75,
                ParametersHashCode = originalSessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = 75,
                ParametersHashCode = updateSessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.ShouldBeEquivalentTo(updatedPagingTokenPagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnDifferentPagingToken_WhenParametersChanged()
        {
            var originalSessionLogsParameters = new SessionLogsParameters {ItemsPerPage = 10};
            var updateSessionLogsParameters = new SessionLogsParameters {ItemsPerPage = 7};

            var originalPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = 75,
                ParametersHashCode = originalSessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = 75,
                ParametersHashCode = updateSessionLogsParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.Should().NotBe(updatedPagingTokenPagingToken);
        }
    }
}
