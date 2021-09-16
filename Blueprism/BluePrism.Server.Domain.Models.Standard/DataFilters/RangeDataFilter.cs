using System;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class RangeDataFilter<TValue> : DataFilter<TValue>
        where TValue : IComparable
    {
        public TValue GreaterThanOrEqualTo { get; set; }
        public TValue LessThanOrEqualTo { get; set; }
    }
}
