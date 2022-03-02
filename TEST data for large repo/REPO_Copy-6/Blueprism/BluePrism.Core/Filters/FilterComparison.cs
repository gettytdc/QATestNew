namespace BluePrism.Core.Filters
{
    /// <summary>
    /// An enumeration of the comparison types supported by a filter
    /// </summary>
    public enum FilterComparison
    {
        None = 0,
        Equals,
        NotEquals,
        HasAnyFlags,
        HasNoFlags,
        Like,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
}
