namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class LessThanOrEqualToFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is LessThanOrEqualToFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is LessThanOrEqualToFilter<TValue> f
                ? new LessThanOrEqualToDataFilter<TValue> { LessThanOrEqualTo = f.LessThanOrEqualTo }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable
            where TOutValue : IComparable =>
            filter is LessThanOrEqualToFilter<TInValue> f
                ? new LessThanOrEqualToDataFilter<TOutValue> { LessThanOrEqualTo = valueConverter(f.LessThanOrEqualTo) }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");
    }
}
