namespace BluePrism.Api.Mappers
{
    using Domain.PagingTokens;
    using Func;
    using Models;
    using static Func.OptionHelper;

    public static class PagingTokenMapper
    {
        public static Option<PagingToken<T>> ToDomainPagingToken<T>(this PagingTokenModel<T> pagingToken) =>
            PagingTokenShouldBeNone(pagingToken)
                ? None<PagingToken<T>>()
                : Some(new PagingToken<T>
                {
                    DataType = pagingToken.DataType,
                    PreviousIdValue = pagingToken.PreviousIdValue,
                    PreviousSortColumnValue = pagingToken.PreviousSortColumnValue,
                    ParametersHashCode = pagingToken.ParametersHashCode,
                });

        private static bool PagingTokenShouldBeNone<T>(PagingTokenModel<T> pagingToken) =>
            pagingToken == null
            || pagingToken.GetPagingTokenState() == PagingTokenState.Empty
            || pagingToken.GetPagingTokenState() == PagingTokenState.Malformed;
    }
}
