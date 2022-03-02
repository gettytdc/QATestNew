namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using SessionStatus = Domain.SessionStatus;

    using static Func.OptionHelper;

    [TestFixture]
    public class SessionParametersMapperTests
    {
        private const string PagingToken = "eyJQcmV2aW91c0lkVmFsdWUiOjM4LCJQcmV2aW91c1NvcnRDb2x1bW5WYWx1ZSI6ImFkbWluIiwiRGF0YVR5cGUiOiJTdHJpbmciLCJQYXJhbWV0ZXJzSGFzaENvZGUiOjMyMTg0Njk0Nn0=";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(),
                new NullFilterModelMapper(),
                new RangeFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new StartsWithOrContainsStringFilterModelMapper(),
            };

            FilterModelMapper.SetFilterModelMappers(filterMappers);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var processSessionParametersDomain = new Domain.SessionParameters
            {
                StartTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1))),
                EndTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1))),
                ItemsPerPage = 10,
                LatestStage = new StringStartsWithFilter("test"),
                PagingToken = Some(new PagingToken<long> { DataType = "String", PreviousIdValue = 38, ParametersHashCode = "321846946", PreviousSortColumnValue = "admin" }),
                StageStarted = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1))),
                SessionNumber = new EqualsFilter<string>("10"),
                ProcessName = new StringStartsWithFilter("test"),
                ResourceName = new StringStartsWithFilter("test"),
                Status = new MultiValueFilter<SessionStatus>(new Filter<SessionStatus>[]
                {
                    new EqualsFilter<SessionStatus>(SessionStatus.Running),
                    new EqualsFilter<SessionStatus>(SessionStatus.Completed)
                }),
                User = new StringStartsWithFilter("bob")
            };

            var processSessionParametersModel = new SessionParameters
            {
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1)) },
                ItemsPerPage = 10,
                LatestStage = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                PagingToken = PagingToken,
                StageStarted = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 05, 313, TimeSpan.FromHours(1)) },
                SessionNumber = new StartsWithOrContainsStringFilterModel { Eq = "10" },
                ProcessName = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                ResourceName = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                Status = new[] { Models.SessionStatus.Completed, Models.SessionStatus.Running },
                UserName = new StartsWithOrContainsStringFilterModel { Strtw = "bob" }
            };

            var result = processSessionParametersModel.ToDomainObject();

            processSessionParametersDomain.ShouldRuntimeTypesBeEquivalentTo(result);
        }

        [Test]
        public void ToDomainObject_ShouldThrowArgumentException_WhenInvalidSessionStatusSupplied()
        {
            Action action = () => new SessionParameters { Status = new[] { (Models.SessionStatus)(-1) } }.ToDomainObject();
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToDomainObject_ShouldThrowArgumentException_WhenInvalidSortbyValueSupplied()
        {
            Action action = () => new SessionParameters { SortBy = (SessionSortBy)(-1) }.ToDomainObject();
            action.ShouldThrow<ArgumentException>();
        }

        [TestCaseSource(nameof(MappedStatuses))]
        public void ToDomainObject_ShouldReturnCorrectSessionStatusArray_WhenCalled(Models.SessionStatus modelSessionStatus, SessionStatus domainSessionStatus)
        {
            var processSessionParametersModel = new SessionParameters
            {
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                ItemsPerPage = 10,
                PagingToken = PagingToken,
                LatestStage = new StartsWithStringFilterModel { Strtw = "test" },
                StageStarted = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                SessionNumber = new StartsWithStringFilterModel { Eq = "10" },
                ProcessName = new StartsWithStringFilterModel { Strtw = "test" },
                ResourceName = new StartsWithStringFilterModel { Strtw = "test" },
                Status = new[] { modelSessionStatus },
                UserName = new StartsWithStringFilterModel { Strtw = "bob" }
            };

            var status = processSessionParametersModel.ToDomainObject().Status;
            status.Should().BeOfType<MultiValueFilter<SessionStatus>>();
            ((MultiValueFilter<SessionStatus>)status).Cast<EqualsFilter<SessionStatus>>().First().EqualTo.Should().Be(domainSessionStatus);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectSessionStatusArray_WhenCalledWithMultipleValidSessionStatuses()
        {
            var processSessionParametersModel = new SessionParameters
            {
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                ItemsPerPage = 10,
                PagingToken = PagingToken,
                LatestStage = new StartsWithStringFilterModel { Strtw = "test" },
                StageStarted = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                SessionNumber = new StartsWithOrContainsStringFilterModel { Eq = "10" },
                ProcessName = new StartsWithStringFilterModel { Strtw = "test" },
                ResourceName = new StartsWithStringFilterModel { Strtw = "test" },
                Status = new[] { Models.SessionStatus.Stopped, Models.SessionStatus.Completed },
                UserName = new StartsWithStringFilterModel { Strtw = "bob" }
            };

            var status = processSessionParametersModel.ToDomainObject().Status;
            status.Should().BeOfType<MultiValueFilter<SessionStatus>>();
            ((MultiValueFilter<SessionStatus>)status).Cast<EqualsFilter<SessionStatus>>().Any(x => x.EqualTo.Equals(SessionStatus.Stopped)).Should().BeTrue();
            ((MultiValueFilter<SessionStatus>)status).Cast<EqualsFilter<SessionStatus>>().Any(x => x.EqualTo.Equals(SessionStatus.Completed)).Should().BeTrue();
        }

        [Test]
        public void ToDomainObject_ShouldReturnEmptySessionStatusArray_WhenNoStatusParametersSupplied()
        {
            var processSessionParametersModel = new SessionParameters
            {
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                ItemsPerPage = 10,
                LatestStage = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                PagingToken = PagingToken,
                StageStarted = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 15, 11, 7, 5, 313, TimeSpan.FromHours(1)) },
                SessionNumber = new StartsWithOrContainsStringFilterModel { Eq = "10" },
                ProcessName = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                ResourceName = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                Status = new Models.SessionStatus[0],
                UserName = new StartsWithOrContainsStringFilterModel { Strtw = "bob" }
            };

            var status = processSessionParametersModel.ToDomainObject().Status;
            status.Should().BeOfType<MultiValueFilter<SessionStatus>>();
            ((MultiValueFilter<SessionStatus>)status).Should().BeEmpty();
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithNullPagingToken()
        {
            var sessionParameters = new SessionParameters() { ItemsPerPage = 5, PagingToken = null };

            var expected = new Domain.SessionParameters() { ItemsPerPage = 5, PagingToken = None<PagingToken<long>>(), };

            var actual = sessionParameters.ToDomainObject();

            actual.PagingToken.Should().Be(expected.PagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithPagingToken()
        {
            var sessionParameters = new SessionParameters { ItemsPerPage = 7 };

            var domainPagingToken = new PagingToken<long>
            {
                DataType = "Int32",
                PreviousIdValue = 1,
                ParametersHashCode = sessionParameters.ToDomainObject().GetHashCodeForValidation()
            };

            var expected = new Domain.SessionParameters() { ItemsPerPage = 7, PagingToken = Some(domainPagingToken) };

            sessionParameters.PagingToken = domainPagingToken.ToString();

            var actual = sessionParameters.ToDomainObject();

            ((Some<PagingToken<long>>)actual.PagingToken).Value
                .ShouldBeEquivalentTo(((Some<PagingToken<long>>)expected.PagingToken).Value);
        }

        [Test]
        public void ToDomainObject_ShouldReturnSamePagingToken_WhenParametersNotChanged()
        {
            var originalSessionParameters = new SessionParameters
            {
                ItemsPerPage = 3,
                UserName = new StartsWithStringFilterModel(),
            };
            var updateSessionParameters = new SessionParameters
            {
                ItemsPerPage = 3,
                UserName = new StartsWithStringFilterModel(),
            };

            var originalPagingTokenPagingToken = new PagingToken<long>()
            {
                DataType = "Int32",
                PreviousIdValue = 1,
                ParametersHashCode = originalSessionParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>()
            {
                DataType = "Int32",
                PreviousIdValue = 1,
                ParametersHashCode = updateSessionParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.ShouldBeEquivalentTo(updatedPagingTokenPagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnDifferentPagingToken_WhenParametersChanged()
        {
            var originalSessionParameters = new SessionParameters
            {
                ItemsPerPage = 3,
                UserName = new StartsWithStringFilterModel(),
            };
            var updateSessionParameters = new SessionParameters
            {
                ItemsPerPage = 3,
                UserName = null,
            };

            var originalPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "Int32",
                PreviousIdValue = 1,
                ParametersHashCode = originalSessionParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "Int32",
                PreviousIdValue = 1,
                ParametersHashCode = updateSessionParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.Should().NotBe(updatedPagingTokenPagingToken);
        }

        private static IEnumerable<TestCaseData> MappedStatuses() =>
            new[]
            {
                (Models.SessionStatus.Completed, SessionStatus.Completed),
                (Models.SessionStatus.Pending, SessionStatus.Pending),
                (Models.SessionStatus.Running, SessionStatus.Running),
                (Models.SessionStatus.Warning, SessionStatus.Warning),
                (Models.SessionStatus.Stopping, SessionStatus.Stopping),
                (Models.SessionStatus.Stopped, SessionStatus.Stopped),
                (Models.SessionStatus.Terminated, SessionStatus.Terminated)
            }.ToTestCaseData();
    }
}
