namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public static class ParameterCollectionExtensionMethods
    {
        public static IDataParameter ByName(this IEnumerable<IDataParameter> @this, string name) =>
            @this.Single(p => p.ParameterName.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static TValue ValueByName<TValue>(this IEnumerable<IDataParameter> @this, string name) =>
            (TValue)ByName(@this, name).Value;
    }
}
