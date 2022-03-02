namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using Func;
    using Server.Domain.Models.Pagination;

    public static class PagingTokenMapper<TOut, TIn> where TOut : BasePagingToken<TIn>, new()
    {
        public static Option<TOut> ToBluePrismPagingToken(Option<Domain.PagingTokens.PagingToken<TIn>> pagingToken)
        {
            switch (pagingToken)
            {
                case Some<Domain.PagingTokens.PagingToken<TIn>> t:
                    return OptionHelper.Some(new TOut
                    {
                        PreviousIdValue = t.Value.PreviousIdValue,
                        DataType = t.Value.DataType,
                        PreviousSortColumnValue = t.Value.PreviousSortColumnValue,
                    });
                default:
                    return OptionHelper.None<TOut>();
            }
        }
    }
}
