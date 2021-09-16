namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using Func;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;

    using static Func.OptionHelper;

    public static class SessionLogsParametersMapper
    {
        public static SessionLogsParameters ToBluePrismObject(this Domain.SessionLogsParameters sessionLogsParameters) =>
            new SessionLogsParameters
            {
                ItemsPerPage = sessionLogsParameters.ItemsPerPage,
                PagingToken = sessionLogsParameters.PagingToken.ToBluePrismPagingToken()
            };

        private static Option<SessionLogsPagingToken> ToBluePrismPagingToken(this Option<Domain.PagingTokens.PagingToken<long>> pagingToken)
        {
            switch (pagingToken)
            {
                case Some<Domain.PagingTokens.PagingToken<long>> t:
                    return Some(new SessionLogsPagingToken
                    {
                        PreviousIdValue = t.Value.PreviousIdValue,
                        DataType = t.Value.DataType,
                    });
                default:
                    return None<SessionLogsPagingToken>();
            }
        }
    }
}
