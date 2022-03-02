namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using FilterMappers;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;
    using ProcessSessionParameters = Server.Domain.Models.ProcessSessionParameters;

    public static class ProcessSessionParametersMapper
    {
        public static ProcessSessionParameters ToBluePrismObject(this Domain.SessionParameters sessionParameters) =>
            new ProcessSessionParameters
            {
                SortBy = (ProcessSessionSortByProperty)sessionParameters.SortBy,
                ItemsPerPage = sessionParameters.ItemsPerPage,
                ProcessName = sessionParameters.ProcessName.ToBluePrismObject(),
                SessionNumber = sessionParameters.SessionNumber.ToBluePrismObject(),
                ResourceName = sessionParameters.ResourceName.ToBluePrismObject(),
                User = sessionParameters.User.ToBluePrismObject(),
                Status = sessionParameters.Status.ToBluePrismObject(x => (SessionStatus)x),
                StartTime = sessionParameters.StartTime.ToBluePrismObject(),
                EndTime = sessionParameters.EndTime.ToBluePrismObject(),
                LatestStage = sessionParameters.LatestStage.ToBluePrismObject(),
                StageStarted = sessionParameters.StageStarted.ToBluePrismObject(),
                PagingToken = PagingTokenMapper<SessionsPagingToken, long>.ToBluePrismPagingToken(sessionParameters.PagingToken)
            };
    }
}
