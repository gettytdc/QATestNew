namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using Func;

    public static class SortByMapper<TItem>
    {
        public static Option<TItem> GetSortByModelName(IDictionary<string, TItem> sortByMappings, string value, TItem defaultSort)
        {
            if (string.IsNullOrEmpty(value))
                return OptionHelper.Some(defaultSort);

            return sortByMappings.TryGetValue(value.ToLower(), out var prop)
                ? OptionHelper.Some(prop)
                : OptionHelper.None<TItem>();
        }
    }
}
