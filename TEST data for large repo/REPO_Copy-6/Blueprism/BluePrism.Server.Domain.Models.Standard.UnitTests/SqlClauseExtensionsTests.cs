using System.Collections.Generic;
using System.Data;
using BluePrism.Server.Domain.Models.DataFilters;
using BluePrism.Server.Domain.Models.Extensions;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    [TestFixture]
    public class SqlClauseExtensionsTests
    {
        [Test]
        public void GetSqlWhereWithParametersStartingWithAndKeyword_ShouldReturnEmptyStringAndEmptyArrayOfParameters_WhenCalledWithEmptyList()
        {
            IReadOnlyCollection<SqlClause> sqlClauses = new List<SqlClause>().AsReadOnly();

            var result = sqlClauses.GetSqlWhereWithParametersStartingWithAndKeyword();

            result.sqlWhereClause.Should().Be(string.Empty);
            result.sqlParameters.Should().BeEmpty();
        }

        [Test]
        public void GetSqlWhereWithParametersStartingWithAndKeyword_ShouldReturnExpectedSqlWhereAndParametersResponse_WhenCalledWithOneSqlClauseItemInList()
        {
            var expectedSqlWhere = " and (Id = @id\r\n) ";
            var expectedSqlParameter = new Mock<IDbDataParameter>().Object;

            IReadOnlyCollection<SqlClause> sqlClauses = new List<SqlClause>()
            {
                new SqlClause
                {
                    SqlText = "Id = @id",
                    Parameters = new []
                    {
                        expectedSqlParameter
                    }
                }
            }.AsReadOnly();

            var result = sqlClauses.GetSqlWhereWithParametersStartingWithAndKeyword();

            result.sqlWhereClause.Should().Be(expectedSqlWhere);
            result.sqlParameters.Length.Should().Be(1);
            result.sqlParameters.GetValue(0).Should().Be(expectedSqlParameter);
        }

        [Test]
        public void GetSqlWhereWithParametersStartingWithAndKeyword_ShouldReturnExpectedSqlWhereAndParametersResponse_WhenCalledWithMoreThanOneSqlClauseItemInList()
        {
            var expectedSqlWhere = " and (Id = @id and CreatedDate = @createdDate\r\n) ";
            var firstParameter = new Mock<IDbDataParameter>().Object;
            var secondParameter = new Mock<IDbDataParameter>().Object;

            IReadOnlyCollection<SqlClause> sqlClauses = new List<SqlClause>()
            {
                new SqlClause
                {
                    SqlText = "Id = @id",
                    Parameters = new []
                    {
                        firstParameter
                    }
                },
                new SqlClause
                {
                    SqlText = "CreatedDate = @createdDate",
                    Parameters = new []
                    {
                        secondParameter
                    }
                }
            }.AsReadOnly();

            var result = sqlClauses.GetSqlWhereWithParametersStartingWithAndKeyword();

            result.sqlWhereClause.Should().Be(expectedSqlWhere);
            result.sqlParameters.Length.Should().Be(2);
            result.sqlParameters.GetValue(0).Should().Be(firstParameter);
            result.sqlParameters.GetValue(1).Should().Be(secondParameter);
        }

        [Test]
        public void GetSqlWhereWithParametersStartingWithWhereKeyword_ShouldReturnEmptyStringAndEmptyArrayOfParameters_WhenCalledWithEmptyList()
        {
            IReadOnlyCollection<SqlClause> sqlClauses = new List<SqlClause>().AsReadOnly();

            var result = sqlClauses.GetSqlWhereWithParametersStartingWithWhereKeyword();

            result.sqlWhereClause.Should().Be(string.Empty);
            result.sqlParameters.Should().BeEmpty();
        }

        [Test]
        public void GetSqlWhereWithParametersStartingWithWhereKeyword_ShouldReturnExpectedSqlWhereAndParametersResponse_WhenCalledWithOneSqlClauseItemInList()
        {
            var expectedSqlWhere = " where (Id = @id\r\n) ";
            var expectedSqlParameter = new Mock<IDbDataParameter>().Object;

            IReadOnlyCollection<SqlClause> sqlClauses = new List<SqlClause>()
            {
                new SqlClause()
                {
                    SqlText = "Id = @id",
                    Parameters = new []
                    {
                        expectedSqlParameter
                    }
                }
            }.AsReadOnly();

            var result = sqlClauses.GetSqlWhereWithParametersStartingWithWhereKeyword();

            result.sqlWhereClause.Should().Be(expectedSqlWhere);
            result.sqlParameters.Length.Should().Be(1);
            result.sqlParameters.GetValue(0).Should().Be(expectedSqlParameter);
        }
        
    }
}
