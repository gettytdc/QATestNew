namespace BluePrism.Api.BpLibAdapters.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Filters;
    using Server.Domain.Models.DataFilters;

    public static class FilterMapper
    {
        private static IReadOnlyCollection<IFilterMapper> _filterMappers;

        public static void SetFilterMappers(IReadOnlyCollection<IFilterMapper> filterMappers) =>
            _filterMappers = filterMappers;

        public static DataFilter<TValue>  ToBluePrismObject<TValue>(this Filter<TValue> filter)
            where TValue : IComparable
            =>
                _filterMappers.SingleOrDefault(x => x.CanMap(filter))?.Map(filter)
                ?? throw new ArgumentException($"Unexpected filter type: {filter?.GetType().FullName}", nameof(filter));

        public static DataFilter<TOutValue> ToBluePrismObject<TInValue, TOutValue>(
            this Filter<TInValue> filter,
            Func<TInValue, TOutValue> valueConverter)
            where TOutValue : IComparable
            where TInValue : IComparable
            =>
                _filterMappers.SingleOrDefault(x => x.CanMap(filter))?.Map(filter, valueConverter)
                ?? throw new ArgumentException($"Unexpected filter type: {filter?.GetType().FullName}", nameof(filter));

        public static ContainsDataFilter ToBluePrismObject(this StringContainsFilter filter)
            => new ContainsDataFilter { ContainsValue = filter.Contains };

        public static NullDataFilter<TValue> ToBluePrismObject<TValue>(this NullFilter<TValue> filter)
            where TValue : IComparable
            => new NullDataFilter<TValue>();

        public static StartsWithDataFilter ToBluePrismObject(this StringStartsWithFilter filter)
            => new StartsWithDataFilter { StartsWith = filter.StartsWith };

        public static EqualsDataFilter<TValue> ToBluePrismObject<TValue>(this EqualsFilter<TValue> filter)
            where TValue : IComparable
            => new EqualsDataFilter<TValue> { EqualTo = filter.EqualTo };

        public static GreaterThanOrEqualToDataFilter<TValue> ToBluePrismObject<TValue>(
            this GreaterThanOrEqualToFilter<TValue> filter)
            where TValue : IComparable
            =>
                new GreaterThanOrEqualToDataFilter<TValue> { GreaterThanOrEqualTo = filter.GreaterThanOrEqualTo };

        public static LessThanOrEqualToDataFilter<TValue> ToBluePrismObject<TValue>(
            this LessThanOrEqualToFilter<TValue> filter)
            where TValue : IComparable
            =>
                new LessThanOrEqualToDataFilter<TValue> { LessThanOrEqualTo = filter.LessThanOrEqualTo };

        public static RangeDataFilter<TValue> ToBluePrismObject<TValue>(this RangeFilter<TValue> filter)
            where TValue : IComparable
            =>
                new RangeDataFilter<TValue>
                {
                    GreaterThanOrEqualTo = filter.GreaterThanOrEqualTo,
                    LessThanOrEqualTo = filter.LessThanOrEqualTo,
                };
    }
}
