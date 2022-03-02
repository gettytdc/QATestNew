namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using Func;

    using static Func.OptionHelper;

    public static class SessionSortByMapper
    {

        private static readonly IDictionary<Models.SessionSortBy, Domain.SessionSortByProperty> SessionSortByMappings =
            new Dictionary<Models.SessionSortBy, Domain.SessionSortByProperty>
            {
                {Models.SessionSortBy.SessionNumberAsc, Domain.SessionSortByProperty.SessionNumberAsc},
                {Models.SessionSortBy.SessionNumberDesc, Domain.SessionSortByProperty.SessionNumberDesc},
                {Models.SessionSortBy.ProcessNameAsc, Domain.SessionSortByProperty.ProcessNameAsc},
                {Models.SessionSortBy.ProcessNameDesc, Domain.SessionSortByProperty.ProcessNameDesc},
                {Models.SessionSortBy.ResourceNameAsc, Domain.SessionSortByProperty.ResourceNameAsc},
                {Models.SessionSortBy.ResourceNameDesc, Domain.SessionSortByProperty.ResourceNameDesc},
                {Models.SessionSortBy.UserNameAsc, Domain.SessionSortByProperty.UserAsc},
                {Models.SessionSortBy.UserNameDesc, Domain.SessionSortByProperty.UserDesc},
                {Models.SessionSortBy.StatusAsc, Domain.SessionSortByProperty.StatusAsc},
                {Models.SessionSortBy.StatusDesc, Domain.SessionSortByProperty.StatusDesc},
                {Models.SessionSortBy.ExceptionTypeAsc, Domain.SessionSortByProperty.ExceptionTypeAsc},
                {Models.SessionSortBy.ExceptionTypeDesc, Domain.SessionSortByProperty.ExceptionTypeDesc},
                {Models.SessionSortBy.StartTimeAsc, Domain.SessionSortByProperty.StartTimeAsc},
                {Models.SessionSortBy.StartTimeDesc, Domain.SessionSortByProperty.StartTimeDesc},
                {Models.SessionSortBy.EndTimeAsc, Domain.SessionSortByProperty.EndTimeAsc},
                {Models.SessionSortBy.EndTimeDesc, Domain.SessionSortByProperty.EndTimeDesc},
                {Models.SessionSortBy.LatestStageAsc, Domain.SessionSortByProperty.LatestStageAsc},
                {Models.SessionSortBy.LatestStageDesc, Domain.SessionSortByProperty.LatestStageDesc},
                {Models.SessionSortBy.StageStartedAsc, Domain.SessionSortByProperty.StageStartedAsc},
                {Models.SessionSortBy.StageStartedDesc, Domain.SessionSortByProperty.StageStartedDesc},
            };
        public static Option<Domain.SessionSortByProperty> GetProcessSessionSortByModelName(Models.SessionSortBy? value)
        {
            var sortBy = value ?? Models.SessionSortBy.SessionNumberAsc;

            return SessionSortByMappings.TryGetValue(sortBy, out var processSessionProperty)
                ? Some(processSessionProperty)
                : None<Domain.SessionSortByProperty>();
        }
    }
}
