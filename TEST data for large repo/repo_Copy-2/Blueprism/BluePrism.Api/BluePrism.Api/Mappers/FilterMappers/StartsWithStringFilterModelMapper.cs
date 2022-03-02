namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Func;
    using Models;

    public class StartsWithStringFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(BasicFilterModel<TValue> filter)
        {
            if (filter?.GetType() != typeof(StartsWithStringFilterModel))
                return false;

            var stringFilter = filter as StartsWithStringFilterModel;

            return stringFilter != null
                   && !string.IsNullOrWhiteSpace(stringFilter.Strtw)
                   && string.IsNullOrWhiteSpace(stringFilter.Gte)
                   && string.IsNullOrWhiteSpace(stringFilter.Lte)
                   && string.IsNullOrWhiteSpace(stringFilter.Eq);
        }

        public Filter<TOutValue> Map<TInValue, TOutValue>(BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter = null) =>
            throw new NotImplementedException();

        public Filter<TValue> Map<TValue>(BasicFilterModel<TValue> filter) =>
            (filter as StartsWithStringFilterModel)?.Strtw?.Map(x => new StringStartsWithFilter(x))
            as Filter<TValue>
            ?? throw new ArgumentException($"TValue is not of a supported type ({typeof(TValue).FullName})");
    }
}
