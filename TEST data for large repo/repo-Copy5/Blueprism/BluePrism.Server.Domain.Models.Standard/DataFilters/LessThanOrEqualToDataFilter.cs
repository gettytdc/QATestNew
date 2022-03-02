using System;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class LessThanOrEqualToDataFilter<TValue> : DataFilter<TValue>
        where TValue : IComparable
    {
        public TValue LessThanOrEqualTo { get; set; }

        public override bool Equals(object obj) =>
         obj is LessThanOrEqualToDataFilter<TValue> e
             && e.LessThanOrEqualTo.Equals(LessThanOrEqualTo);

        public override int GetHashCode() => LessThanOrEqualTo.GetHashCode();
    }
}
