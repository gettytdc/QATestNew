namespace BluePrism.Api.Extensions
{
    using System;
    using Models;

    public static class FilterModelExtensions
    {
        public static RangeFilterModel<TValue> GetRangeFilterOrThrow<TValue>(this BasicFilterModel<TValue> filter)
        {
            var rangeFilter = filter as RangeFilterModel<TValue>;

            if (rangeFilter == null)
                throw new ArgumentException($"Incorrect filter type supplied: {filter}");

            return rangeFilter;
        }
    }
}
