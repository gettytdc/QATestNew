using System;
using System.Data.SqlClient;

namespace BluePrism.Core.Extensions
{
    public static class SqlParamaterCollectionExtensions
    {
        public static SqlParameter AddNullableWithValue(this SqlParameterCollection @this, string parameterName, object value)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("sql parameter name cannot be null or whitespace", nameof(parameterName));
            }

            return value is null
                ? @this.AddWithValue(parameterName, DBNull.Value)
                : @this.AddWithValue(parameterName, value);
        }
    }
}