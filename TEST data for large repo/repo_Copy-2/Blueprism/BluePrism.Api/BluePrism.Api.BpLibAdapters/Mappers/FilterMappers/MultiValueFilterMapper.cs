namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using System.Linq;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public class MultiValueFilterMapper : IFilterMapper
    {
        public bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable =>
            filter is MultiValueFilter<TValue>;

        public DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable
        {
            if (!(filter is MultiValueFilter<TValue> multiValueFilter))
                throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

            if (!multiValueFilter.Any())
                return MultiValueDataFilter<TValue>.Empty();

            return new MultiValueDataFilter<TValue>(multiValueFilter.Select(valueFilter => valueFilter.ToBluePrismObject()));
        }

        public DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable
            where TOutValue : IComparable
        {
            if (!(filter is MultiValueFilter<TInValue> multiValueFilter))
                throw new ArgumentException($"Unexpected filter type: {filter.GetType().FullName}");

            if (!multiValueFilter.Any())
                return MultiValueDataFilter<TOutValue>.Empty();

            return new MultiValueDataFilter<TOutValue>(multiValueFilter.Select(valueFilter => valueFilter.ToBluePrismObject(valueConverter)));
        }
    }
}
