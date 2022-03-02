namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class GreaterThanOrEqualToFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is GreaterThanOrEqualToFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is GreaterThanOrEqualToFilter<TValue> f
                ? new GreaterThanOrEqualToDataFilter<TValue> { GreaterThanOrEqualTo = f.GreaterThanOrEqualTo }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable
            where TOutValue : IComparable =>
            filter is GreaterThanOrEqualToFilter<TInValue> f
                ? new GreaterThanOrEqualToDataFilter<TOutValue> { GreaterThanOrEqualTo = valueConverter(f.GreaterThanOrEqualTo) }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

    }
}
