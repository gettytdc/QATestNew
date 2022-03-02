namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class EqualsFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is EqualsFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is EqualsFilter<TValue> f
                ? new EqualsDataFilter<TValue> {EqualTo = f.EqualTo}
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable
            where TOutValue : IComparable =>
            filter is EqualsFilter<TInValue> f
                ? new EqualsDataFilter<TOutValue> {EqualTo = valueConverter(f.EqualTo)}
                : throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");
    }
}
