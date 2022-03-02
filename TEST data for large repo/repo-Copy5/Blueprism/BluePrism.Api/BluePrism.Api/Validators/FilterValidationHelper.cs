namespace BluePrism.Api.Validators
{
    using System;
    using System.Data.SqlTypes;
    using Mappers.FilterMappers;
    using Models;

    public static class FilterValidationHelper
    {
        private static readonly DateTimeOffset MinValidDateTimeOffset = new DateTimeOffset(SqlDateTime.MinValue.Value);
        private static readonly DateTimeOffset MaxValidDateTimeOffset = new DateTimeOffset(SqlDateTime.MaxValue.Value);

        public static bool BeValidFilter<TValue>(BasicFilterModel<TValue> model) =>
             model.CanMap();

        public static bool BeValidRangeFilter(RangeFilterModel<int?> model) =>
            model == null ||
            ((model.Lte >= 0 || model.Lte == null) &&
             (model.Gte >= 0 || model.Gte == null) &&
             (model.Eq >= 0 || model.Eq == null));

        public static bool BeValidRangeFilterOfDateTimeOffset(RangeFilterModel<DateTimeOffset?> model) =>
            model == null ||
                (IsValidDateTimeOffset(model.Eq) && IsValidDateTimeOffset(model.Lte) && IsValidDateTimeOffset(model.Gte));

        private static bool IsValidDateTimeOffset(DateTimeOffset? dateTimeOffset) =>
            dateTimeOffset == null ||
                (dateTimeOffset.Value.CompareTo(MinValidDateTimeOffset) > 0 && dateTimeOffset.Value.CompareTo(MaxValidDateTimeOffset) < 0);
    }
}
