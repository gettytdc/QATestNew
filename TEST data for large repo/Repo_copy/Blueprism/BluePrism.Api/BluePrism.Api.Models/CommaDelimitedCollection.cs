namespace BluePrism.Api.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.ModelBinding;

    [ModelBinder(typeof(CommaDelimitedCollectionModelBinder))]
    public class CommaDelimitedCollection<TItem> : List<TItem>
    {
        public bool HasBoundData { get; }
        public string RawStringValues { get; }

        public CommaDelimitedCollection()
        { }

        public CommaDelimitedCollection(string rawStringValues)
        {
            HasBoundData = false;
            RawStringValues = rawStringValues;
        }

        public CommaDelimitedCollection(IEnumerable<TItem> values)
            : base(values) =>
            HasBoundData = true;

        public CommaDelimitedCollection(IEnumerable<object> objectValues)
            : this(objectValues.Cast<TItem>()) =>
            HasBoundData = true;

        public static implicit operator CommaDelimitedCollection<TItem>(TItem[] values) =>
            new CommaDelimitedCollection<TItem>(values);
    }
}
