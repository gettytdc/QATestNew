namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;

    public class NullFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(Models.BasicFilterModel<TValue> filter) =>
            filter == null
            || (filter.Eq == null && filter.GetType() == typeof(Models.BasicFilterModel<TValue>));

        public Domain.Filters.Filter<TOutValue> Map<TInValue, TOutValue>(Models.BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter = null) =>
            new Domain.Filters.NullFilter<TOutValue>();

        public Domain.Filters.Filter<TValue> Map<TValue>(Models.BasicFilterModel<TValue> filter) =>
            new Domain.Filters.NullFilter<TValue>();
    }
}
