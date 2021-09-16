namespace BluePrism.Api.Mappers
{
    using System;
    using System.Linq;

    public static class ItemsPageMapper
    {
        public static Models.ItemsPageModel<TModelItem> ToModelItemsPage<TDomainItem, TModelItem>(this Domain.ItemsPage<TDomainItem> domainItemsPage, Func<TDomainItem, TModelItem> itemsConverter)
            where TDomainItem : class
            where TModelItem : class =>
            new Models.ItemsPageModel<TModelItem>()
            {
                Items = domainItemsPage.Items.Select(itemsConverter),
                PagingToken = domainItemsPage.PagingToken is Func.Some<string> t ? t.Value : null
            };
    }
}
