namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public interface IFilterMapper
    {
        bool CanMap<TValue>(Filter<TValue> filter) where TValue : IComparable;
        DataFilter<TValue> Map<TValue>(Filter<TValue> filter) where TValue : IComparable;
        DataFilter<TOutValue> Map<TInValue, TOutValue>(Filter<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
            where TInValue : IComparable
            where TOutValue : IComparable;
    }
}
