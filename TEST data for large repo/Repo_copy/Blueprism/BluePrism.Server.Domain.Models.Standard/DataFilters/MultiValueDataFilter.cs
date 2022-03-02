namespace BluePrism.Server.Domain.Models.DataFilters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class MultiValueDataFilter<TValue> : DataFilter<TValue>, IEnumerable<DataFilter<TValue>>
        where TValue : IComparable
    {
        private readonly IReadOnlyCollection<DataFilter<TValue>> _dataFilters;

        public MultiValueDataFilter(IEnumerable<DataFilter<TValue>> dataFilters) =>
            _dataFilters = dataFilters.ToList();
        
        public IEnumerator<DataFilter<TValue>> GetEnumerator() =>
            _dataFilters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static MultiValueDataFilter<TValue> Create(IEnumerable<DataFilter<TValue>> dataFilters) =>
            new MultiValueDataFilter<TValue>(dataFilters);

        public static MultiValueDataFilter<TValue> Empty() =>
            new MultiValueDataFilter<TValue>(Array.Empty<DataFilter<TValue>>());

        public override bool Equals(object obj) =>
            obj is MultiValueDataFilter<TValue> e
                && e.SequenceEqual(_dataFilters);

        public override int GetHashCode() => _dataFilters.GetHashCode();
    }
}
