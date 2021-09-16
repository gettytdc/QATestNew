namespace BluePrism.Api.Models
{
    using System.Collections.Generic;

    public class ItemsPageModel<TItem> where TItem : class
    {
        public string PagingToken { get; set; }

        public IEnumerable<TItem> Items { get; set; }
    }
}
