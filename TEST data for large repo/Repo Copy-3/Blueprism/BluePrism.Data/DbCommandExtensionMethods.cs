namespace BluePrism.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Utilities.Functional;

    public static class DbCommandExtensionMethods
    {
        public static IDbCommand AddParameter(this IDbCommand @this, string name, object value)
        {
            @this.CreateParameter()
                .Tee(x => x.ParameterName = name)
                .Tee(x => x.Value = value)
                .Map(@this.Parameters.Add);

            return @this;
        }

        public static IDbCommand AddEnumerable<T>(this IDbCommand @this, string name, IEnumerable<T> items)
        {
            if (string.IsNullOrWhiteSpace(name))
            { 
                throw new ArgumentException("sql parameter name cannot be null or whitespace", nameof(name)); 
            }

            int i = 0;
            foreach(T value in items)
            {
                if (value == null)
                    @this.AddParameter($"{name}{i}", DBNull.Value);
                else
                    @this.AddParameter($"{name}{i}", value);
                ++i;
            }
            return @this;
        }

        public static IDbCommand SetConnection(this IDbCommand @this, IDbConnection connection) =>
            @this.Tee(x => x.Connection = connection);

    }
}