namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;

    public class EqualFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(Models.BasicFilterModel<TValue> filter) =>
            filter != null && filter.Eq != null && filter.GetType() == typeof(Models.BasicFilterModel<TValue>);

        public Domain.Filters.Filter<TValue> Map<TValue>(Models.BasicFilterModel<TValue> filter) =>
            new Domain.Filters.EqualsFilter<TValue>(filter.Eq);

        public Domain.Filters.Filter<TOutValue> Map<TInValue, TOutValue>(Models.BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter) =>
            new Domain.Filters.EqualsFilter<TOutValue>(valueConverter(filter.Eq));
    }
}
