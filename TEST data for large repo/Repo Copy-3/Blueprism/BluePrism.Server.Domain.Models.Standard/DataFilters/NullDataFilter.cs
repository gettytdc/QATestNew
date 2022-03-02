using System;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class NullDataFilter<TValue> : DataFilter<TValue>
        where TValue : IComparable
    {
    }
}
