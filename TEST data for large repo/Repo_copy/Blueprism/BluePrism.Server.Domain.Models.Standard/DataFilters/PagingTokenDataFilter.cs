using System;
using BluePrism.Server.Domain.Models.Pagination;

namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class PagingTokenDataFilter<TToken, TTokenId> : DataFilter<TToken>
        where TToken : BasePagingToken<TTokenId>, IComparable 
    {
        public TToken PagingToken { get; set; }
    }
}
