namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;
    using Domain.Filters;
    using Func;
    using Models;

    public class StartsWithOrContainsStringFilterModelMapper : IFilterModelMapper
    {
        public bool CanMap<TValue>(BasicFilterModel<TValue> filter)
        {
            // ReSharper disable once UsePatternMatching
            // Unable to do pattern matching here. Pipeline fails with c#7.1 syntax error.
            var stringFilter = filter as StartsWithOrContainsStringFilterModel;

            return stringFilter != null
                   && !string.IsNullOrWhiteSpace(stringFilter.Ctn)
                   && string.IsNullOrWhiteSpace(stringFilter.Strtw)
                   && string.IsNullOrWhiteSpace(stringFilter.Gte)
                   && string.IsNullOrWhiteSpace(stringFilter.Lte)
                   && string.IsNullOrWhiteSpace(stringFilter.Eq);
        }

        public Filter<TOutValue> Map<TInValue, TOutValue>(BasicFilterModel<TInValue> filter, Func<TInValue, TOutValue> valueConverter = null) =>
            throw new NotImplementedException();

        public Filter<TValue> Map<TValue>(BasicFilterModel<TValue> filter) =>
            (filter as StartsWithOrContainsStringFilterModel)?.Ctn?.Map(x => new StringContainsFilter(x))
                as Filter<TValue>
            ?? throw new ArgumentException($"TValue is not of a supported type ({typeof(TValue).FullName})");
    }
}
