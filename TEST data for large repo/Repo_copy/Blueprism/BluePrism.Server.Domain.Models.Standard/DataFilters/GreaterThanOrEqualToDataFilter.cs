using System;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class GreaterThanOrEqualToDataFilter<TValue> : DataFilter<TValue>
        where TValue : IComparable
    {
        public TValue GreaterThanOrEqualTo { get; set; }

        public override bool Equals(object obj) =>
          obj is GreaterThanOrEqualToDataFilter<TValue> e
              && e.GreaterThanOrEqualTo.Equals(GreaterThanOrEqualTo);

        public override int GetHashCode() => GreaterThanOrEqualTo.GetHashCode();
    }
}
