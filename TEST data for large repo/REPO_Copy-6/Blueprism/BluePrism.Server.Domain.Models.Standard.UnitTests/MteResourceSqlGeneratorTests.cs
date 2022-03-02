using System;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using BluePrism.Server.Domain.Models.DataFilters;
using System.Data.SqlClient;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    [TestFixture]
    public class MteResourceSqlGeneratorTests
    { 
        [Test]
        public void Ctor_ShouldThrowException_WhenTokenMissingFromSql()
        {
            Action act = () => new MteResourceSqlGenerator("select 1 from Table");
            act
                .ShouldThrow<ArgumentException>()
                .Where(message => message.Message.Contains(MteResourceSqlGenerator.MteToken));
        }

        [Test]
        public void ReplaceTokenAndAddParameters_ShouldThrowException_WhenSqlCommandIsNull()
        {
            var mte = new MteResourceSqlGenerator("select 1 from Table " + MteResourceSqlGenerator.MteToken);
            Action act = () => mte.ReplaceTokenAndAddParameters(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ReplaceTokenAndAddParameters_CreateCorrectSqlAndNoParameters_WhenResourcePermissionAreEmpty()
        {
            var sqlCommand = new SqlCommand();
            var mte = new MteResourceSqlGenerator("select 1 from Table" + MteResourceSqlGenerator.MteToken);

            var query = mte.ReplaceTokenAndAddParameters(sqlCommand).Trim();

            query.Should().Be("select 1 from Table");
            sqlCommand.Parameters.Count.Should().Be(0);
        }

        [Test]
        public void ReplaceTokenAndAddParameters_ShouldAddJoinStatement_WhenResourcesHaveValues()
        {
            var mte = new MteResourceSqlGenerator("select 1 from Table " + MteResourceSqlGenerator.MteToken);

            var query = mte.ReplaceTokenAndAddParameters(new SqlCommand(), new[] { 1, 2, 3 }, new[] { 42, 80 });

            query.Should().Contain("inner join ufn_GetResourcesWithPermissionOnRole");            
        }

        [Test]
        public void ReplaceTokenAndAddParameters_ShouldAddRelevantParameters_WhenResourcesHaveValues()
        {
            var mte = new MteResourceSqlGenerator("select 1 from Table " + MteResourceSqlGenerator.MteToken);
            var cmd = new SqlCommand();
            mte.ReplaceTokenAndAddParameters(cmd, new[] { 1, 2, 3 }, new int[] { 4, 5, 6 });

            var commandParams = cmd
                .Parameters
                .Cast<SqlParameter>()
                .Select(parameter => parameter.ParameterName)
                .ToList();

            commandParams.Should().Contain("resourcePermissions");
        }
    }
}
