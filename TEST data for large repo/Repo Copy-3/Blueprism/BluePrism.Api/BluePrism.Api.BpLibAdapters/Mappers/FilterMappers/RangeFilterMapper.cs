namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class RangeFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is RangeFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is RangeFilter<TValue> f
                ? new RangeDataFilter<TValue>
                {
                    GreaterThanOrEqualTo = f.GreaterThanOrEqualTo, LessThanOrEqualTo = f.LessThanOrEqualTo,
                }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter,
            Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable where TOutValue : IComparable =>
            filter is RangeFilter<TInValue> f
                ? new RangeDataFilter<TOutValue>
                {
                    GreaterThanOrEqualTo = valueConverter(f.GreaterThanOrEqualTo),
                    LessThanOrEqualTo = valueConverter(f.LessThanOrEqualTo),
                }
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

    }
}
