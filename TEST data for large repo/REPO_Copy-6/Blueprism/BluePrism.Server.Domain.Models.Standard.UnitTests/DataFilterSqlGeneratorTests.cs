
using System;
using System.Globalization;
using BluePrism.Server.Domain.Models.Pagination;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using DataFilters;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DataFilterSqlGeneratorTests
    {
        private const string ColumnName = "column1";

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithMultiValueDataFilter()
        {
            const string value1 = "value1";
            const string value2 = "value2";

            var equalsDataFilters = new MultiValueDataFilter<string>(new List<DataFilter<string>>
            {
                new EqualsDataFilter<string> {EqualTo = value1}, new EqualsDataFilter<string> {EqualTo = value2},
            });

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .SetupSequence(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = equalsDataFilters.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult =
                $"({ColumnName} = @{sqlClause.Parameters[0].ParameterName} OR {ColumnName} = @{sqlClause.Parameters[1].ParameterName})";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(value1);
            sqlClause.Parameters[1].Value.Should().Be(value2);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnEmptySqlClause_WhenCalledWithEmptyMultiValueDataFilter() =>
            MultiValueDataFilter<string>.Empty()
                .GetSqlWhereClauses(Mock.Of<IDbCommand>(), ColumnName)
                .Should()
                .BeEmpty();

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithStringEqualsDataFilter()
        {
            const string equalToValue = "value1";
            var filter = new EqualsDataFilter<string> {EqualTo = equalToValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"{ColumnName} = @{sqlClause.Parameters[0].ParameterName}";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(equalToValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithBooleanEqualsDataFilter()
        {
            var filter = new EqualsDataFilter<bool> {EqualTo = false};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"({ColumnName} IS NULL OR {ColumnName} = @{sqlClause.Parameters[0].ParameterName})";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(false);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithRangeDataFilter()
        {
            const string greaterThanOrEqualToValue = "A";
            const string lessThanOrEqualToValue = "C";
            var filter = new RangeDataFilter<string> {GreaterThanOrEqualTo = greaterThanOrEqualToValue, LessThanOrEqualTo = lessThanOrEqualToValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .SetupSequence(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClauses = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).ToArray();

            var expectedResult1 = $"{ColumnName} >= @{sqlClauses[0].Parameters[0].ParameterName}";
            var expectedResult2 = $"{ColumnName} <= @{sqlClauses[1].Parameters[0].ParameterName}";

            sqlClauses[0].SqlText.Should().Be(expectedResult1);
            sqlClauses[0].Parameters[0].Value.Should().Be(greaterThanOrEqualToValue);
            sqlClauses[1].SqlText.Should().Be(expectedResult2);
            sqlClauses[1].Parameters[0].Value.Should().Be(lessThanOrEqualToValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithGreaterThanOrEqualToDataFilter()
        {
            const string greaterThanOrEqualToValue = "A";
            var filter = new GreaterThanOrEqualToDataFilter<string> {GreaterThanOrEqualTo = greaterThanOrEqualToValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"{ColumnName} >= @{sqlClause.Parameters[0].ParameterName}";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(greaterThanOrEqualToValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithLessThanOrEqualToDataFilter()
        {
            const string lessThanOrEqualToValue = "A";
            var filter = new LessThanOrEqualToDataFilter<string> {LessThanOrEqualTo = lessThanOrEqualToValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"{ColumnName} <= @{sqlClause.Parameters[0].ParameterName}";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(lessThanOrEqualToValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithContainsDataFilter()
        {
            const string containsValue = "A";
            var filter = new ContainsDataFilter {ContainsValue = containsValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"CHARINDEX(@{sqlClause.Parameters[0].ParameterName}, {ColumnName}) > 0";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(containsValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnValidSqlClause_WhenCalledWithStartsWithDataFilter()
        {
            const string startsWithValue = "A";
            var filter = new StartsWithDataFilter {StartsWith = startsWithValue};

            var mockDbCommand = new Mock<IDbCommand>();
            mockDbCommand
                .Setup(x => x.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());

            var sqlClause = filter.GetSqlWhereClauses(mockDbCommand.Object, ColumnName).First();

            var expectedResult = $"CHARINDEX(@{sqlClause.Parameters[0].ParameterName}, {ColumnName}) = 1";

            sqlClause.SqlText.Should().Be(expectedResult);
            sqlClause.Parameters[0].Value.Should().Be(startsWithValue);
        }

        [Test]
        public void GetSqlWhereClauses_ShouldReturnNoSqlClause_WhenCalledWithNullDataFilter()
        {
            var filter = new NullDataFilter<string>();

            var sqlClause = filter.GetSqlWhereClauses(Mock.Of<IDbCommand>(), ColumnName);

            sqlClause.Should().BeEmpty();
        }

        [TestCase("asc","loaded","ident", "isnull(loaded, '') > @Parameter1 or (isnull(loaded, '') = @Parameter1 and ident > @Parameter2)")]
        [TestCase("desc", "loaded", "ident", "isnull(loaded, '') < @Parameter1 or (isnull(loaded, '') = @Parameter1 and ident < @Parameter2)")]
        public void GetSqlWhereClauses_ShouldReturnExpectedSqlClause_WhenCalledWith(string sortDirection, string sortByColumn, string identityColumn, string expectedSqlClause)
        {
            var mockParameter1 = new Mock<IDbDataParameter>();
            mockParameter1.Setup(x => x.ParameterName).Returns("Parameter1");

            var mockParameter2 = new Mock<IDbDataParameter>();
            mockParameter2.Setup(x => x.ParameterName).Returns("Parameter2");

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.SetupSequence(x => x.CreateParameter())
                       .Returns(mockParameter1.Object)
                       .Returns(mockParameter2.Object);

            var workQueueItemPagingToken = new PagingTokenDataFilter<WorkQueueItemPagingToken, long>()
                {
                    PagingToken = new WorkQueueItemPagingToken()
                    {
                        DataType = "Int64",
                        PreviousIdValue = 1,
                        PreviousSortColumnValue = "2",
                    }
                };
            var result = workQueueItemPagingToken.GetSqlWhereClauses(mockCommand.Object, sortByColumn, sortDirection, identityColumn);

            CultureInfo.CurrentCulture.CompareInfo.Compare(result.SqlText, expectedSqlClause, CompareOptions.IgnoreSymbols).Should().Be(0);
        }
    }
}
