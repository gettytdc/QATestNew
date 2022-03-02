using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BluePrism.Server.Domain.Models.Pagination;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public static class DataFilterSqlGenerator
    {
        private const string DateTimeFormatValue = "yyyy-MM-dd HH:mm:ss.fff";

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
        this DataFilter<TValue> dataFilter,
        IDbCommand command,
        string columnName)
        where TValue : IComparable
        {
            switch (dataFilter)
            {
                case EqualsDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                case GreaterThanOrEqualToDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                case LessThanOrEqualToDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                case RangeDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                case NullDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                case MultiValueDataFilter<TValue> f:
                    return f.GetSqlWhereClauses(command, columnName);

                default:
                    // In C# 7.0 we have to check string filters here
                    if (dataFilter is ContainsDataFilter)
                        return (dataFilter as ContainsDataFilter).GetSqlWhereClauses<TValue>(command, columnName);

                    if (dataFilter is StartsWithDataFilter)
                        return (dataFilter as StartsWithDataFilter).GetSqlWhereClauses<TValue>(command, columnName);

                    throw new ArgumentException($"Unrecognized filter type: {dataFilter?.GetType().FullName ?? "<null>"}",
                        nameof(dataFilter));
            }
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this MultiValueDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            if (!dataFilter.Any())
                return new NullDataFilter<TValue>().GetSqlWhereClauses(command, columnName);

            var sqlWhereClauses = dataFilter
                .Select(x => x.GetSqlWhereClauses(command, columnName))
                .SelectMany(x => x)
                .ToList();

            var sqlText = string.Join(" OR ", sqlWhereClauses.Select(x => x.SqlText));
            var parameters = sqlWhereClauses.Select(x => x.Parameters).SelectMany(x => x).ToArray();

            return new[] { new SqlClause { SqlText = $"({sqlText})", Parameters = parameters } };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this EqualsDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var parameter = command.CreateParameterForValue(dataFilter.EqualTo);

            var sqlText = $"{columnName} = @{parameter.ParameterName}";

#pragma warning disable S2583 // Conditionally executed code should be reachable
            if (parameter.Value is bool value && !value)
#pragma warning restore S2583 // Conditionally executed code should be reachable
            {
                sqlText = $"({columnName} IS NULL OR {columnName} = @{parameter.ParameterName})";
            }

            return new[] { new SqlClause { SqlText = sqlText, Parameters = new[] { parameter } } };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this RangeDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var greaterThanParameter = command.CreateParameterForValue(dataFilter.GreaterThanOrEqualTo);
            var lessThanParameter = command.CreateParameterForValue(dataFilter.LessThanOrEqualTo);

            return new[]
            {
                new SqlClause
                {
                    SqlText = $"{columnName} >= @{greaterThanParameter.ParameterName}",
                    Parameters = new[] { greaterThanParameter },
                },
                new SqlClause
                {
                    SqlText = $"{columnName} <= @{lessThanParameter.ParameterName}",
                    Parameters = new[] { lessThanParameter },
                },
            };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this GreaterThanOrEqualToDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var parameter = command.CreateParameterForValue(dataFilter.GreaterThanOrEqualTo);

            return new[]
            {
                new SqlClause { SqlText = $"{columnName} >= @{parameter.ParameterName}", Parameters = new[] { parameter }, },
            };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this ContainsDataFilter dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var parameter = command.CreateParameterForValue(dataFilter.ContainsValue);

            return new[]
            {
                new SqlClause
                {
                    SqlText = $"CHARINDEX(@{parameter.ParameterName}, {columnName}) > 0", Parameters = new[] { parameter }
                }
            };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this StartsWithDataFilter dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var parameter = command.CreateParameterForValue(dataFilter.StartsWith);

            return new[]
            {
                new SqlClause
                {
                    SqlText = $"CHARINDEX(@{parameter.ParameterName}, {columnName}) = 1", Parameters = new[] { parameter }
                }
            };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this LessThanOrEqualToDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
        {
            var parameter = command.CreateParameterForValue(dataFilter.LessThanOrEqualTo);

            return new[]
            {
                new SqlClause { SqlText = $"{columnName} <= @{parameter.ParameterName}", Parameters = new[] { parameter }, },
            };
        }

        public static SqlClause GetSqlWhereClauses<TToken, TTokenId>(
            this PagingTokenDataFilter<TToken, TTokenId> dataFilter,
            IDbCommand command,
            string columnName,
            string sortDirection,
            string idColumnName)
            where TToken : BasePagingToken<TTokenId>, IComparable
        {
            var previousSortColumnValue = dataFilter.PagingToken.GetSqlPreviousSortColumnValue();
            var sortColumnParameter = command.CreateParameterForValue(previousSortColumnValue);
            var idParameter = command.CreateParameterForValue(dataFilter.PagingToken.PreviousIdValue);

            var orderDirection = sortDirection == "desc" ? "<" : ">";

            var sql = $@"(isnull({columnName}, '') { orderDirection } @{ sortColumnParameter.ParameterName } or
                    isnull({columnName}, '') = @{ sortColumnParameter.ParameterName }
                        and {idColumnName} { orderDirection } @{idParameter.ParameterName})";

            return new SqlClause
            {
                SqlText = sql,
                Parameters = new[] { sortColumnParameter, idParameter }
            };
        }

        public static SqlClause GetSqlWhereClauses<TToken, TTokenId>(
            this PagingTokenDataFilter<TToken, TTokenId> dataFilter,
            IDbCommand command,
            string idColumnName,
            bool isPositiveCursorSearch = true)
            where TToken : BasePagingToken<TTokenId>, IComparable
        {
            var idParameter = command.CreateParameterForValue(dataFilter.PagingToken.PreviousIdValue);

            var moreThanLessThan = isPositiveCursorSearch ? ">" : "<";

            var sql = $"{idColumnName} {moreThanLessThan} @{idParameter.ParameterName}";

            return new SqlClause
            {
                SqlText = sql,
                Parameters = new[] { idParameter }
            };
        }

        public static IReadOnlyCollection<SqlClause> GetSqlWhereClauses<TValue>(
            this NullDataFilter<TValue> dataFilter,
            IDbCommand command,
            string columnName)
            where TValue : IComparable
            =>
                new SqlClause[0];

        private static IDbDataParameter CreateParameterForValue(this IDbCommand command, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = GetRandomParameterName();
            parameter.Value = GetParameterValue(value);
            return parameter;
        }

        private static object GetParameterValue(object value)
        {
            switch (value)
            {
                case DateTimeOffset x:
                    return x.UtcDateTime.ToString(DateTimeFormatValue);
                case DateTime x:
                    return x.ToString(DateTimeFormatValue);
                default:
                    return value;
            }
        }

        private static string GetRandomParameterName() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .TrimEnd('=')
                .Replace('+', '_')
                .Replace('/', '_');
    }
}
