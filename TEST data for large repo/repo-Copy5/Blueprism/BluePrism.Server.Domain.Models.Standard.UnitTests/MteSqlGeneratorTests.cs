using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using BluePrism.Server.Domain.Models.DataFilters;
using System.Data.SqlClient;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    [TestFixture]
    public class MteSqlGeneratorTests
    {
        private static IEnumerable<string> StringWithoutTokenValues => new string[] { "select 1 from Table", default(string) };

        [Test, TestCaseSource(nameof(StringWithoutTokenValues))]
        public void Ctor_ShouldThrowException_WhenTokenMissingFromSql(string sql)
        {
            Action act = () => new MteSqlGenerator(sql, "s");
            act
                .ShouldThrow<ArgumentException>()
                .Where(message => message.Message.Contains(MteSqlGenerator.MteToken));
        }

        private static IEnumerable<string> EmptyStringValues => new string[] { string.Empty, " ", default(string) };

        [Test, TestCaseSource(nameof(EmptyStringValues))]
        public void Ctor_ShouldThrowException_WhenFilterTableAliasMissingFromSql(string alias)
        {
            Action act = () => new MteSqlGenerator("select 1 from Table " + MteSqlGenerator.MteToken, alias, false);
            act
                .ShouldThrow<ArgumentException>()
                .Where(message => message.Message.Contains("filterTableAlias"));
        }

        private static IEnumerable<object[]> OtherCriteriaValues => new List<object[]>
        {
            new object[] { false, " where " },
            new object[] { true, " and " },
        };

        [Test, TestCaseSource(nameof(OtherCriteriaValues))]
        public void BuildQueryString_ShouldAddCorrectClause_WhenHasOtherCriteriaIsSet(bool hasOtherCriteria, string clauseStart)
        {
            var sql = "select 1 from Table as s ";
            var mte = new MteSqlGenerator(sql + MteSqlGenerator.MteToken, "s", hasOtherCriteria);
            var sqlCommand = new SqlCommand();
            mte.BuildQueryString(sqlCommand, new[] { 1, 2, 3 }, new[] { 4, 5 })
                .Should().Contain(sql + clauseStart);
        }

        [Test]
        public void BuildQueryString_ShouldThrowException_WhenSqlCommandIsNull()
        {
            var mte = new MteSqlGenerator("select 1 from Table as s WHERE " + MteSqlGenerator.MteToken, "s");
            Action act = () => mte.BuildQueryString(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void BuildQueryString_CreateCorrectSqlAndNoParameters_WhenUserRolesAreEmpty()
        {
            var sqlCommand = new SqlCommand();
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s", false);

            var query = mte.BuildQueryString(sqlCommand).Trim();

            query.Should().Be("select 1 from Table as s");
            sqlCommand.Parameters.Count.Should().Be(0);
        }

        private static IEnumerable<object[]> DropCreateTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0], true },
            new object[] { new int[0], new int[] { 4, 5, 6 }, true },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, true },
            new object[] { new int[] { 4, 5, 6 }, new int[0], false },
            new object[] { new int[0], new int[] { 4, 5, 6 }, false },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, false }
        };

        [Test, TestCaseSource(nameof(DropCreateTestValues))]
        public void BuildQueryString_ShouldAddTempTableStatement_WhenProcessesOrResourcesHaveValues(IReadOnlyCollection<int> resources, IReadOnlyCollection<int> processes, bool isDrop)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");

            var query = mte.BuildQueryString(new SqlCommand(), new[] { 1, 2, 3 }, resources, processes);

            query.Should().Contain((isDrop ? "drop" : "create") + " table #");
        }

        private static IEnumerable<object[]> InsertTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0] },
            new object[] {new int[0], new int[] { 4, 5, 6 } },
            new object[] {new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 } }
        };

        [Test, TestCaseSource(nameof(InsertTestValues))]
        public void BuildQueryString_ShouldAddInsertStatement_WhenProcessesOrResourcesHaveValues(IReadOnlyCollection<int> resources, IReadOnlyCollection<int> processes)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");

            var query = mte.BuildQueryString(new SqlCommand(), new[] { 1, 2, 3 }, resources, processes);

            query.Should().Contain("insert into #");
        }

        private static IEnumerable<object[]> InsertFunctionTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0], new string[] { "@resourcePermissions" } },
            new object[] { new int[0], new int[] { 4, 5, 6 }, new string[] { "@processPermissions" } },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new string[] { "@resourcePermissions", "@processPermissions" } }
        };

        [Test, TestCaseSource(nameof(InsertFunctionTestValues))]
        public void BuildQueryString_ShouldAddInsertFromFunction_WhenProcessesOrResourcesHaveValues(
            IReadOnlyCollection<int> resources,
            IReadOnlyCollection<int> processes,
            IReadOnlyCollection<string> paramNames)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");

            var query = mte.BuildQueryString(new SqlCommand(), new[] { 1, 2, 3 }, resources, processes);

            foreach (var parameterName in paramNames)
            {
                query.Should().Contain(parameterName);
            }
        }

        private static IEnumerable<object[]> SelectExistsTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0], new string[] { "FieldType = 'R'" } },
            new object[] { new int[0], new int[] { 4, 5, 6 }, new string[] { "FieldType = 'P'" } },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new string[] { "FieldType = 'R'", "FieldType = 'P'" } }
        };

        [Test, TestCaseSource(nameof(SelectExistsTestValues))]
        public void BuildQueryString_ShouldAddExistsCheckToQuery_WhenProcessesOrResourcesHaveValues(
            IReadOnlyCollection<int> resources,
            IReadOnlyCollection<int> processes,
            IReadOnlyCollection<string> existsFragments)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");

            var query = mte.BuildQueryString(new SqlCommand(), new[] { 1, 2, 3 }, resources, processes);

            foreach (var existsFragment in existsFragments)
            {
                query.Should().Contain(existsFragment);
            }
        }

        private static IEnumerable<object[]> InsertSelectUnionTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0], false },
            new object[] { new int[0], new int[] { 4, 5, 6 }, false },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, true }
        };

        [Test, TestCaseSource(nameof(InsertSelectUnionTestValues))]
        public void BuildQueryString_ShouldCreateUnion_WhenProcessesAndResourcesHaveValues(
            IReadOnlyCollection<int> resources,
            IReadOnlyCollection<int> processes,
            bool shouldCreateUnion)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");

            var query = mte.BuildQueryString(new SqlCommand(), new[] { 1, 2, 3 }, resources, processes);

            if (shouldCreateUnion)
            {
                query.Should().Contain("union all");
            }
            else
            {
                query.Should().NotContain("union all");
            }
        }

        private static IEnumerable<object[]> ParametersTestValues => new List<object[]>
        {
            new object[] { new int[] { 4, 5, 6 }, new int[0], new string[] { "resourcePermissions" } },
            new object[] { new int[0], new int[] { 4, 5, 6 }, new string[] { "processPermissions" } },
            new object[] { new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 }, new string[] { "resourcePermissions", "processPermissions" } }
        };

        [Test, TestCaseSource(nameof(ParametersTestValues))]
        public void BuildQueryString_ShouldAddRelevantParameters_WhenProcessesAndResourcesHaveValues(
            IReadOnlyCollection<int> resources,
            IReadOnlyCollection<int> processes,
            IReadOnlyCollection<string> paramNames)
        {
            var mte = new MteSqlGenerator("select 1 from Table as s " + MteSqlGenerator.MteToken, "s");
            var cmd = new SqlCommand();
            mte.BuildQueryString(cmd, new[] { 1, 2, 3 }, resources, processes);

            var commandParams = cmd
                .Parameters
                .Cast<SqlParameter>()
                .Select(parameter => parameter.ParameterName)
                .ToList();

            foreach (var parameterName in paramNames)
            {
                commandParams.Should().Contain(parameterName);
            }
        }
    }
}
