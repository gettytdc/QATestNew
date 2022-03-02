namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class NullFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is NullFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is NullFilter<TValue>
                ? new NullDataFilter<TValue>()
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter = null)
            where TInValue : IComparable where TOutValue : IComparable =>
            filter is NullFilter<TInValue>
                ? new NullDataFilter<TOutValue>()
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");
    }
}
