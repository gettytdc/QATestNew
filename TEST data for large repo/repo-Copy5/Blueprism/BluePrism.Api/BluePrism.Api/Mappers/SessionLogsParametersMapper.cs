namespace BluePrism.Api.Mappers
{
    using SessionLogsParameters = Models.SessionLogsParameters;
    
    public static class SessionLogsParametersMapper
    {
        private const int  DefaultItemsPerPage = 10;

        public static Domain.SessionLogsParameters ToDomainObject(this SessionLogsParameters sessionLogsParameters) =>
            new Domain.SessionLogsParameters
            {
                ItemsPerPage = sessionLogsParameters.ItemsPerPage ?? DefaultItemsPerPage,
                PagingToken = sessionLogsParameters.PagingToken.ToDomainPagingToken()
            };
    }
}
