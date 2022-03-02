namespace BluePrism.Api.Mappers
{
    using System;
    using Models;

    public static class ResourcesSummaryUtilizationParametersMapper
    {
        public static Domain.ResourcesSummaryUtilizationParameters ToDomainObject(this ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters) =>
            new Domain.ResourcesSummaryUtilizationParameters
            {
                StartDate = resourcesSummaryUtilizationParameters.StartDate,
                EndDate = resourcesSummaryUtilizationParameters.EndDate,
                ResourceIds = resourcesSummaryUtilizationParameters.ResourceIds ?? new Guid[] { }
            };
    }
}
