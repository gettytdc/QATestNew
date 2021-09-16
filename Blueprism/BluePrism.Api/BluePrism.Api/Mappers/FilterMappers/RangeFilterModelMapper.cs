namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;
    using Extensions;

    public class RangeFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(Models.BasicFilterModel<TValue> filter) =>
            filter is Models.RangeFilterModel<TValue> rangeFilter
            && rangeFilter.Eq == null && rangeFilter.Gte != null && rangeFilter.Lte != null;

        public Domain.Filters.Filter<TOutValue> Map<TInValue, TOutValue>(Models.BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter)
        {
            var rangeFilter = filter.GetRangeFilterOrThrow();

            return new Domain.Filters.RangeFilter<TOutValue>(valueConverter(rangeFilter.Gte), valueConverter(rangeFilter.Lte));
        }
        
        public Domain.Filters.Filter<TValue> Map<TValue>(Models.BasicFilterModel<TValue> filter)
        {
            var rangeFilter = filter.GetRangeFilterOrThrow();

            return new Domain.Filters.RangeFilter<TValue>(rangeFilter.Gte, rangeFilter.Lte);
        }   
    }
}
