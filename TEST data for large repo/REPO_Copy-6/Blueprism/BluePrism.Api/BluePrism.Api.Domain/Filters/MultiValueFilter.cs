namespace BluePrism.Api.Domain.Filters
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Func;

    public class MultiValueFilter<TValue> : Filter<TValue>, IEnumerable<Filter<TValue>>
    {
        private readonly IReadOnlyCollection<Filter<TValue>> _filters;

        public MultiValueFilter(IEnumerable<Filter<TValue>> filters) => _filters = filters.ToList();

        public IEnumerator<Filter<TValue>> GetEnumerator() => _filters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj) =>
            obj is MultiValueFilter<TValue> e
            && e.SequenceEqual(_filters);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _filters.FirstOrDefault()?.GetHashCode() ?? 0;
                _filters.Skip(1).ForEach(x => hashCode = (hashCode * 397) ^ (x != null ? x.GetHashCode() : 0)).Evaluate();
                return hashCode;
            }
        }
    }
}
