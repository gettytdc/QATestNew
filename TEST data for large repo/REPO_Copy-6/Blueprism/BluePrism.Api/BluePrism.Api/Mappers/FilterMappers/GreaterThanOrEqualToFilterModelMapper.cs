namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;
    using Extensions;

    public class GreaterThanOrEqualToFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(Models.BasicFilterModel<TValue> filter)
        {
            if (filter?.GetType() != typeof(Models.RangeFilterModel<TValue>))
                return false;

            var rangeFilter = (Models.RangeFilterModel<TValue>)filter;

            return rangeFilter.Eq == null && rangeFilter.Gte != null && rangeFilter.Lte == null;
        }

        public Domain.Filters.Filter<TOutValue> Map<TInValue, TOutValue>(Models.BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter) =>
            new Domain.Filters.GreaterThanOrEqualToFilter<TOutValue>(valueConverter(filter.GetRangeFilterOrThrow().Gte));

        public Domain.Filters.Filter<TValue> Map<TValue>(Models.BasicFilterModel<TValue> filter) =>
            new Domain.Filters.GreaterThanOrEqualToFilter<TValue>(filter.GetRangeFilterOrThrow().Gte);

    }
}
