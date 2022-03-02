namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class StringStartsWithFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is StringStartsWithFilter;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is StringStartsWithFilter
                ? new StartsWithDataFilter { StartsWith = (filter as StringStartsWithFilter).StartsWith } as DataFilter<TValue>
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter = null)
            where TInValue : IComparable where TOutValue : IComparable
            => filter is StringStartsWithFilter
                ? new StartsWithDataFilter { StartsWith = (filter as StringStartsWithFilter).StartsWith } as DataFilter<TOutValue>
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");
    }
}
