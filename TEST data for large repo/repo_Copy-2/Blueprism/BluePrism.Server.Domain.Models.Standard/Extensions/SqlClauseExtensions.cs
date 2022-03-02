using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using BluePrism.Server.Domain.Models.DataFilters;

namespace BluePrism.Server.Domain.Models.Extensions
{
    public static class SqlClauseExtensions
    {
        private enum FirstSqlKeyword
        {
            Where,
            And
        }

        public static (string sqlWhereClause, Array sqlParameters) GetSqlWhereWithParametersStartingWithWhereKeyword
            (this IReadOnlyCollection<SqlClause> @this) => GetSqlWhereWithParametersAndStartingSqlKeyword(@this, FirstSqlKeyword.Where);

        public static (string sqlWhereClause, Array sqlParameters) GetSqlWhereWithParametersStartingWithAndKeyword
            (this IReadOnlyCollection<SqlClause> @this) => GetSqlWhereWithParametersAndStartingSqlKeyword(@this, FirstSqlKeyword.And);

        private static (string sqlWhereClause, Array sqlParameters) GetSqlWhereWithParametersAndStartingSqlKeyword(this IReadOnlyCollection<SqlClause> @this, FirstSqlKeyword firstSqlFirstKeyword)
        {
            if (@this.Count == 0) return (string.Empty, Array.Empty<IDbDataParameter>());

            var sqlStringBuilder = new StringBuilder();
            
            sqlStringBuilder.Append($" {firstSqlFirstKeyword.ToString().ToLower()}");

            sqlStringBuilder.Append(" (");
            sqlStringBuilder.AppendLine(string.Join(" and ", @this.Select(x => x.SqlText)));
            sqlStringBuilder.Append(") ");

            var allParameters = @this.SelectMany(x => x.Parameters).ToArray();

            return (sqlStringBuilder.ToString(), allParameters);
        }
    }
}
