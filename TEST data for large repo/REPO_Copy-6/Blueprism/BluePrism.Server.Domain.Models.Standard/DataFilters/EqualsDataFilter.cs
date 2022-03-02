using System;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class EqualsDataFilter<TValue> : DataFilter<TValue>
        where TValue : IComparable
    {
        public TValue EqualTo { get; set; }

        public override bool Equals(object obj) =>
           obj is EqualsDataFilter<TValue> e
               && e.EqualTo.Equals(EqualTo);

        public override int GetHashCode() => EqualTo.GetHashCode();
    }
}
