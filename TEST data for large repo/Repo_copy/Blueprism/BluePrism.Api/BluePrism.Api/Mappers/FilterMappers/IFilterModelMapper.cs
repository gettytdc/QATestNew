namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;

    public interface IFilterModelMapper
    {
        bool CanMap<TValue>(Models.BasicFilterModel<TValue> filter);

        Domain.Filters.Filter<TOutValue> Map<TInValue, TOutValue>(Models.BasicFilterModel<TInValue> filter,
                Func<TInValue, TOutValue> valueConverter);

        Domain.Filters.Filter<TValue> Map<TValue>(Models.BasicFilterModel<TValue> filter);
    }
}
