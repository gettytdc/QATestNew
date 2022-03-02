namespace BluePrism.Api.Domain
{
    using System.Collections.Generic;
    using Func;

    public class ItemsPage<TItem> where TItem : class
    {
        public Option<string> PagingToken { get; set; }

        public IEnumerable<TItem> Items { get; set; }
    }
}
