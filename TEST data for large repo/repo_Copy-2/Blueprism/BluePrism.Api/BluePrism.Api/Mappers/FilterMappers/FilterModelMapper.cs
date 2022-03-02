namespace BluePrism.Api.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Filters;

    public static class FilterModelMapper
    {
        private static IReadOnlyCollection<IFilterModelMapper> _filterModelMappers;

        public static void SetFilterModelMappers(IReadOnlyCollection<IFilterModelMapper> filterModelMappers) =>
            _filterModelMappers = filterModelMappers;

        public static Filter<TValue> ToDomain<TValue>(this Models.BasicFilterModel<TValue> model)
        {
            var baseFilter = model?.GetLowestBaseFilter();
            return
                _filterModelMappers.SingleOrDefault(x => x.CanMap(baseFilter))?.Map(baseFilter)
                ?? throw new ArgumentException($"Unexpected filter type: {model?.GetType().FullName ?? "<null>"}", nameof(model));
        }

        public static Filter<TOutValue> ToDomain<TInValue, TOutValue>(
            this Models.BasicFilterModel<TInValue> model,
            Func<TInValue, TOutValue> valueConverter)
        {
            var baseFilter = model?.GetLowestBaseFilter();
            return
                _filterModelMappers.SingleOrDefault(x => x.CanMap(baseFilter))?.Map(baseFilter, valueConverter)
                ?? throw new ArgumentException($"Unexpected filter type: {model?.GetType().FullName ?? "<null>"}", nameof(model));
        }

        public static Filter<TOutValue> ToDomain<TInValue, TOutValue>(
            this Models.BasicFilterModel<TInValue>[] models,
            Func<TInValue, TOutValue> valueConverter) =>
            new MultiValueFilter<TOutValue>(models.Select(model => model.ToDomain(valueConverter)));

        public static Filter<TValue> ToDomain<TValue>(this Models.RangeFilterModel<TValue> model)
            where TValue : IComparable
            =>
                model != null ? ((Models.BasicFilterModel<TValue>)model).ToDomain() : new NullFilter<TValue>();

        public static Domain.Filters.Filter<string> ToDomain(this Models.StartsWithOrContainsStringFilterModel model)
            =>
                model != null ? ((Models.BasicFilterModel<string>)model).ToDomain() : new NullFilter<string>();

        public static bool CanMap<TValue>(this Models.BasicFilterModel<TValue> model)
        {
            var baseFilter = model?.GetLowestBaseFilter();
            return _filterModelMappers.Any(x => x.CanMap(baseFilter));
        }
    }
}
