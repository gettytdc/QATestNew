namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Filters;

    public static class MultiValueFilterMapper
    {
        public static MultiValueFilter<TValue> ToMultiValueFilter<TValue>(this IEnumerable<TValue> values) =>
            new MultiValueFilter<TValue>(values.Select(v => new EqualsFilter<TValue>(v)));
    }
}
